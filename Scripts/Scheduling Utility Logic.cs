using System;
using System.Collections.Generic;
using System.Linq;
using ETS.Core;
using ETS.Core.Api;
using ETS.Core.Api.Models;
using ETS.Core.Api.Models.Data;
using ETS.Core.Api.Models.DateAndTime;
using ETS.Core.Api.Models.ProductionScheduling;
using ETS.Core.Api.Models.ProductionScheduling.Plugins;
using ETS.Core.Enums;
using ETS.Core.Extensions;

namespace ETS.Core.Scripting
{
  /// ******************************************************************
  /// <summary>
  /// Capabilities are broken down into Regions based upon functionality.
  /// Each has a boolean to enable/disable it and a summary before it.
  /// 
  /// CURRENT LIST OF FUNCTIONALITY:
  ///  -Region Name [ Method Prefix ]
  ///  -SequencedScheduling [ SS ]
  ///  -MaterialScheduling [ MAT ]
  ///  -General Utilities [ Util ]
  /// </summary>
  /// ******************************************************************
  public class SimSchedUtil
  {
    private ApiService _api;
    private List<int> _unscheduleables;
    public SimSchedUtil(ApiService api)
    {
      _api = api;
      _unscheduleables = new List<int>();
    }
    
    #region SequencedScheduling
    /// ******************************************************************
    /// <summary>
    /// ParentTimes[ParentJobID, LastJob.EndDateTime]
    /// </summary>
    /// ******************************************************************
    private bool SS_Enabled = false;
    private Dictionary<int, DateTimeOffset> SS_ParentTimes;
    private List<int> SS_ParentUnscheduleables;
    private int SS_JobOffsetSeconds;
    
    public void SS_Enable(int jobOffsetDuration = 0)
    {
      SS_Enabled = true;
      SS_ParentTimes = new Dictionary<int, DateTimeOffset>();
      SS_ParentUnscheduleables = new List<int>();
      SS_JobOffsetSeconds = jobOffsetDuration;
    }
    
    /// ***********************************************************
    /// Checks for Parent in the ParentUnscheduleables
    /// ***********************************************************
    public Result<bool> SS_ShouldScheduleJob(int parentJobID)
    {
      var r = new Result<bool>();
      
      if(this.SS_ParentUnscheduleables.Contains(parentJobID)) { return r.AsSuccess(false); }
      
      return r.AsSuccess(true);
    }
    
    /// ***********************************************************
    /// Checks for start time in the ParentTimes
    /// ***********************************************************
    public Result<DateTimeOffset> SS_CalculateMinStartDateTimeForJobOnSystem(int jobID, int parentJobID, DateTimeOffset originalTime)
    {
      var r = new Result<DateTimeOffset>();
      if(!SS_Enabled) return r.AsSuccess(originalTime);

      // check if job has had a sibling already scheduled
      if(SS_ParentTimes.ContainsKey(parentJobID))
      {
        var rUtil = this.Util_ReturnLaterDateTimeOffset(originalTime, SS_ParentTimes[parentJobID]);
        if(!rUtil.Success) { return r.AsError(rUtil.Messages[0].Message); }
        else { return r.AsSuccess(rUtil.Return); }
      }
      return r.AsSuccess(originalTime);
    }
    
    /// ***********************************************************
    /// Updates the Parent in the Dictionary
    /// ***********************************************************
    public Result<bool> SS_PostScheduleInfoForJobSystems(int parentJobID, PsJobDurationInfo durationInfo, DateTimeOffset plannedStart)
    {
      var r = new Result<bool>();
      if(!SS_Enabled) return r.AsSuccess(true);
      
      if(parentJobID.IsNotNull()) 
      { 
        SS_ParentTimes[parentJobID] = plannedStart.AddSeconds(durationInfo.TotalSeconds + SS_JobOffsetSeconds); 
      }
      return r.AsSuccess(true);
    }
    
    /// ***********************************************************
    /// Adds ID list to ParentUnscheduleables
    /// ***********************************************************
    public Result<bool> SS_PostScheduleJobs(List<int> parentJobIDs)
    {
      var r = new Result<bool>();
      
      SS_ParentUnscheduleables.AddRange(parentJobIDs);
      
      return r.AsSuccess(true);
    }
    #endregion
    
    #region MaterialScheduling
    /// ******************************************************************
    /// <summary>
    /// Inventory -> Dictionary[MaterialID, List[Items]]
    /// JobNeeds -> Dictionary[JobID, Dictionary[MaterialID, Quantity]]
    /// </summary>
    /// ******************************************************************
    private bool MAT_Enabled = false;
    
    private Dictionary<int, DateTimeOffset> MAT_JobStart;
    private Dictionary<int, List<DbItem>> MAT_Inventory;
    private Dictionary<int, Dictionary<int, double>> MAT_JobNeeds;
    
    public void MAT_Enable()
    {
      MAT_Enabled = true;
      
      MAT_JobStart = new Dictionary<int, DateTimeOffset>();
      MAT_Inventory = new Dictionary<int, List<DbItem>>();
      MAT_JobNeeds = new Dictionary<int, Dictionary<int, double>>();
    }
    
    /// ******************************************************************
    /// Prepare the Inventory and JobNeeds dictionaries
    /// ******************************************************************
    public Result<bool> MAT_Init()
    {
      var r = new Result<bool>();
      if(!MAT_Enabled) { return r.AsSuccess(true); }
      
      try
      {
        // get list of all items, ordered by valid date
        //var siteID = _api.Site.GetCurrentSiteID();
        var siteID = 5; // TEMPORARY DUE TO A BUG (exists in 11.1 beta)
        var itemSql = "SELECT i.* FROM tItem i JOIN tLocation l ON i.LocationID = l.ID WHERE l.SiteID = {0} ORDER BY ValidFromDateTime".FormatWith(siteID);
        var itemList = _api.Data.DbItem.GetList.WithSql(itemSql);

        // split into dictionary - foreach item - add to list for matching material id
        foreach(DbItem item in itemList)
        {
          if(item.MaterialID.IsNull()) continue;
          if(!MAT_Inventory.ContainsKey(item.MaterialID))
          {
            MAT_Inventory[item.MaterialID] = new List<DbItem>();
          }
          var matList = MAT_Inventory[item.MaterialID];
          matList.Add(item);
          MAT_Inventory[item.MaterialID] = matList;
        }
      }
      catch(Exception ex)
      {
        _api.Util.LogCustom.WriteException(ex);
        _api.Util.LogCustom.WriteInformation(ex.StackTrace, "", "");
        return r.AsError(ex.Message);
      }
      
      return r.AsSuccess(true);
    }
    
    /// ******************************************************************
    /// Sets JobStart based upon Materials 
    /// ******************************************************************
    public Result<bool> MAT_PreScheduleInfoForJobSystems(int jobID, DateTimeOffset originalTime)
    {
      var r = new Result<bool>();
      if(!MAT_Enabled) { return r.AsSuccess(true); }
      if(_unscheduleables.Contains(jobID)) { return r.AsError("Job ID {0} is Unscheduleable".FormatWith(jobID)); }
      
      try
      {
        // check _jobNeeds, if doesn't exist, create 
        if(!MAT_JobNeeds.ContainsKey(jobID)) this.MAT_PreScheduleInfoForJobSystems_NewJob(jobID);
        DateTimeOffset bestTime = originalTime;
        
        // loop through needs to get timestamp
        foreach(var need in MAT_JobNeeds[jobID])
        {
          //need.Key; // materialID
          //need.Value; // quantity needed

          if(!MAT_Inventory.ContainsKey(need.Key)) { return r.AsSuccess(true); }
          var inventoryList = MAT_Inventory[need.Key];
          double remaining = need.Value;
          bool enoughMaterial = false;
          
          foreach(DbItem item in inventoryList)
          {
            remaining -= item.Quantity;
            if(remaining <= 0) 
            {
              // if there's enough, check the datetime to return
              bestTime = this.Util_ReturnLaterDateTimeOffset(item.ValidFromDateTime, bestTime).Return;
              enoughMaterial = true;
              break;
            }
          }
          
          // if foreach completes, there's not enough
          if(!enoughMaterial) 
          {
            _unscheduleables.Add(jobID);
            MAT_JobStart.RemoveByKeyIfExists(jobID);
            break;
          }
        }
        
        // add bestTime to MAT_JobStart
        MAT_JobStart[jobID] = bestTime;
        
        // return
        return r.AsSuccess(true);
      }
      catch(Exception ex)
      {
        _api.Util.LogCustom.WriteException(ex);
        _api.Util.LogCustom.WriteInformation(ex.StackTrace, "", "");
        return r.AsError(ex.Message);
      }
    }
    private Result<bool> MAT_PreScheduleInfoForJobSystems_NewJob(int jobID)
    {
      var r = new Result<bool>();
      try
      {
        // for job ID, create dictionary of <materialID, qty needed>
        var needsDictionary = new Dictionary<int, double>();
        
        var job = _api.Data.DbJob.Load.ByID(jobID);
        var product = _api.Data.DbProduct.Load.ByID(job.ProductID);
        var productMaterialList = _api.Data.DbProductMaterial.GetList.ForProductID(product.ID);

        var multiplier = 1 / product.StandardSize;
        if(job.Type == JobType.Discrete)
        {
          var jobDiscrete = _api.Data.DbJobDiscrete.Load.ByJobID(job.ID);
          multiplier = multiplier * jobDiscrete.PlannedCalculationCount;
        }
        else if(job.Type == JobType.Batch)
        {
          var jobBatch = _api.Data.DbJobBatch.Load.ByJobID(job.ID);
          multiplier = multiplier * jobBatch.PlannedBatchSize * jobBatch.PlannedNumberOfBatches;
        }
        
        foreach(DbProductMaterial pm in productMaterialList)
        {
          needsDictionary[pm.MaterialID] = pm.Quantity * multiplier;
        }
        
        // add to full dictionary
        MAT_JobNeeds[jobID] = needsDictionary;
        
        // load product, load product material, load job size
        return r.AsSuccess(true);
      }
      catch(Exception ex)
      {
        _api.Util.LogCustom.WriteException(ex);
        _api.Util.LogCustom.WriteInformation(ex.StackTrace, "", "");
        return r.AsError(ex.Message);
      }
    }
    
    /// ******************************************************************
    /// Returns the appropriate date time
    /// ******************************************************************
    public Result<DateTimeOffset> MAT_CalculateMinStartDateTimeForJobOnSystem(int jobID)
    {
      var r = new Result<DateTimeOffset>();
      if(!MAT_Enabled) { return r.AsSuccess(ETS.Core.Constants.NullDateTimeOffset); }
      if(!MAT_JobStart.ContainsKey(jobID))
      {
        _api.Util.LogCustom.WriteInformation("FAILED HERE WITH JOBID {0}".FormatWith(jobID), "", "");
      }
      
      return r.AsSuccess(MAT_JobStart[jobID]);
    }
    /// ******************************************************************
    /// Remove Job from Job Needs
    /// Remove Items from Inventory
    /// ******************************************************************
    public Result<bool> MAT_PostScheduleInfoForJobSystems(int jobID)
    {
      var r = new Result<bool>();
      if(!MAT_Enabled) { return r.AsSuccess(true); }
      
      try
      {
        // load what would be consumed
        var needsDictionary = MAT_JobNeeds[jobID];
        
        // loop through _inventory and remove/update items
        foreach(var need in needsDictionary)
        {
          // get item list
          var itemList = MAT_Inventory[need.Key];
          
          // prepare a list for removal
          var removeList = new List<DbItem>();
          
          // placeholder for calculating quantity
          double remaining = need.Value;
          foreach(var item in itemList)
          {
            if(item.Quantity > (remaining)) // if item covers quantity, adjust item and move to next
            {
              item.Quantity -= remaining;
              break;
            }
            else // if item is not enough, reduce remaining and queue item for removal
            {
              remaining -= item.Quantity;
              removeList.Add(item);
              continue;
            }
          }
          
          // update item list
          foreach(var item in removeList)
          {
            itemList.Remove(item);
          }
          
          // re-add to _inventory
          MAT_Inventory[need.Key] = itemList;
          
          // remove job from JobNeeds
          MAT_JobNeeds.RemoveByKeyIfExists(jobID);
        }
        return r.AsSuccess(true);
      }
      catch(Exception ex)
      {
        _api.Util.LogCustom.WriteException(ex);
        _api.Util.LogCustom.WriteInformation(ex.StackTrace, "", "");
        return r.AsError(ex.Message);
      }
    }
    #endregion   
    
    #region GeneralUtilities
    private int _psStageID;
    private List<DbPsStageMessage> _psStageMessages;
    
    /// ***********************************************************
    public Result<bool> Util_PostCreatePsStage(int psStageID)
    {
      var r = new Result<bool>();
      
      _psStageID = psStageID;
      _psStageMessages = new List<DbPsStageMessage>();
      
      return r.AsSuccess(true);
    }
    
    /// ***********************************************************
    public Result<bool> Util_CreatePsStageMessage(string stageMessage)
    {
      var r = new Result<bool>();
      
      var message = new DbPsStageMessage();
      message.PsStageID = _psStageID;
      message.Message = stageMessage;
      _psStageMessages.Add(message);
      
      return r.AsSuccess(true);
    }
    
    /// ***********************************************************
    public Result<bool> Util_PostSavePsStageJobs(IUnitOfWork uow)
    {
      var r = new Result<bool>();
      
      foreach(var message in _psStageMessages)
      {
        _api.Data.DbPsStageMessage.Save.InsertAsNew(message, uow).ThrowIfFailed("");
      }
      
      return r.AsSuccess(true);
    }
    
    /// ***********************************************************
    public Result<DateTimeOffset> Util_ReturnLaterDateTimeOffset(DateTimeOffset date1, DateTimeOffset date2)
    {
      var r = new Result<DateTimeOffset>();
      return r.AsSuccess((date1 > date2) ? date1 : date2);
    }
    
    /// ***********************************************************
    public Result<DateTimeOffset> Util_ReturnLaterDateTimeOffset(List<DateTimeOffset> dtoList)
    {
      var r = new Result<DateTimeOffset>();
      if(dtoList.Count == 0)
      {
        return r.AsError("ReturnEarlierDateTime: List.Count was 0.");
      }
      
      var bestDate = dtoList[0];
      for(int i = 1; i < dtoList.Count; i++)
      {
        if(dtoList[i].IsNull()) continue;
        bestDate = this.Util_ReturnLaterDateTimeOffset(bestDate, dtoList[i]).ThrowIfFailed();
      }
      
      return r.AsSuccess(bestDate);
    }
    
    /// ***********************************************************
    public Result<DateTimeOffset> Util_ReturnEarlierDateTimeOffset(DateTimeOffset date1, DateTimeOffset date2)
    {
      var r = new Result<DateTimeOffset>();
      return r.AsSuccess((date1 < date2) ? date1 : date2);
    }
    
    /// ***********************************************************
    public Result<DateTimeOffset> Util_ReturnEarlierDateTimeOffset(List<DateTimeOffset> dtoList)
    {
      var r = new Result<DateTimeOffset>();
      if(dtoList.Count == 0)
      {
        return r.AsError("ReturnEarlierDateTime: List.Count was 0.");
      }
      
      var bestDate = dtoList[0];
      for(int i = 1; i < dtoList.Count; i++)
      {
        if(dtoList[i].IsNull()) continue;
        bestDate = this.Util_ReturnEarlierDateTimeOffset(bestDate, dtoList[i]).ThrowIfFailed();
      }
      
      return r.AsSuccess(bestDate);
    }
    
    /// ***********************************************************
    /// Checks for Job in _unscheduleables
    /// ***********************************************************
    public Result<bool> Util_ShouldScheduleJob(int jobID)
    {
      var r = new Result<bool>();
      
      if(_unscheduleables.Contains(jobID)) { return r.AsSuccess(false); }
      
      return r.AsSuccess(true);
    }
    
    /// ***********************************************************
    /// Adds Job to _unscheduleables
    /// ***********************************************************
    public Result<bool> Util_PostScheduleJobs(List<int> jobIDs)
    {
      var r = new Result<bool>();
      
      _unscheduleables.AddRange(jobIDs);
      
      return r.AsSuccess(true);
    }

    #endregion
  }
}