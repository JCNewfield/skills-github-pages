using System;
using System.Collections.Generic;
using ETS.Core;
using ETS.Core.Api;
using ETS.Core.Api.Models;
using ETS.Core.Api.Models.Data;
using ETS.Core.Api.Models.DateAndTime;
using ETS.Core.Api.Models.ProductionScheduling;
using ETS.Core.Api.Models.ProductionScheduling.Plugins;
using ETS.Core.Enums;
using ETS.Core.Extensions;
using ETS.Core.Scripting;

namespace ETS.Core.Scripting.ProductionScheduling
{
  /// ******************************************************************
  /// <summary>
  /// This class contains code that is executed during the execution of
  /// the Production Scheduling algorithm and allows customization through
  /// script to alter the default behavior, enabling the ability to control
  /// if jobs can be assigned and what the overall placement score is for
  /// each attempt.
  /// </summary>
  /// ******************************************************************
  public class Debug_Discrete_Hooks : PsDiscreteJobPluginBase
  {
    private readonly ApiService _api;
    private IPsStatusUpdater _updater;

    public PsScheduleSettings Settings { get; set; }

    /// ******************************************************************
    public Debug_Discrete_Hooks(ApiService api)
    {
      _api = api;
      this.Settings = null;
    }

    /// ******************************************************************
    public override Result<bool> Init(IPsGeneralInitContext context)
    {
      var r = new Result<bool>();
      this.Settings = context.Settings;
      
      _api.Util.LogCustom.WriteInformation("Init", "All Discrete Hooks", @"");
      
      return r.AsSuccess(true);
      
    }

    /// ******************************************************************
    public override Result<DateTimeRange> CalculateScheduleRange(PsScheduleSettings settings, DateTimeRange defaultRange)
    {
      var r = new Result<DateTimeRange>();

      _api.Util.LogCustom.WriteInformation("CalculateScheduleRange", "All Discrete Hooks", @"");
      
      return r.AsSuccess(defaultRange);
    }

    /// ******************************************************************
    public override Result<bool> PostCreatePsStage(DbPsStage psStage, IPsStatusUpdater updater)
    {
      var r = new Result<bool>();
      _updater = updater;
      
      _api.Util.LogCustom.WriteInformation("PostCreatePsStage", "All Discrete Hooks", @"");
      
      return r.AsSuccess(true);
    }

    /// ***********************************************************
    /// Runs once per job
    /// ***********************************************************
    private int JobCount = 0; // hardcoding this to limit the number of jobs that get scheduled
    public override Result<bool> ShouldScheduleJob(IPsDiscreteJobScheduleContext context)
    {
      var r = new Result<bool>();
      
      if(JobCount >= 3) return r.AsSuccess(false);
      
      _api.Util.LogCustom.WriteInformation("ShouldScheduleJob", "All Discrete Hooks", "JobID = {0}".FormatWith(context.Job.ID));
      JobCount++;
      
      return r.AsSuccess(true);
    }

    /// ***********************************************************
    public override Result<List<DbJobDiscreteComposite>> ResortJobList(List<DbJobDiscreteComposite> list) 
    {
      var r = new Result<List<DbJobDiscreteComposite>>();
      
      _api.Util.LogCustom.WriteInformation("ResortJobList", "All Discrete Hooks", @"List.Count = {0}".FormatWith(list.Count));
      
      return r.AsSuccess(list);
    }

    #region runs once per system
    /// ***********************************************************
    public override Result<bool> ShouldScheduleForSystem(IPsSystemDiscreteScheduleContext context) 
    {
      var r = new Result<bool>();
      
      _api.Util.LogCustom.WriteInformation("ShouldScheduleForSystem", "All Discrete Hooks", @"System.ID = {0}
MinStart = {1}".FormatWith(context.System.ID, context.MinStart));
      
      return r.AsSuccess(true);
    }
    
    /// ***********************************************************
    public override Result<DateTimeOffset> CalculateMinStartDateTimeForSystem(IPsSystemDiscreteScheduleContext context) 
    {
      var r = new Result<DateTimeOffset>();
      
      _api.Util.LogCustom.WriteInformation("CalculateMinStartDateTimeForSystem", "All Discrete Hooks", @"System.ID = {0}
MinStart = {1}".FormatWith(context.System.ID, context.MinStart));
      
      return r.AsSuccess(context.MinStart);
    }
    #endregion

    /// ***********************************************************
    /// Runs once per system
    /// ***********************************************************
    public override Result<bool> UpdateUnavailableTimesForSystem(IPsSystemUnavailableContext context)
    {
      var r = new Result<bool>();
            
      _api.Util.LogCustom.WriteInformation("UpdateUnavailableTimesForSystem", "All Discrete Hooks", @"System.ID = {0}
System.MinStartDateTime = {1}
UnavailableTimes.Count = {2}".FormatWith(context.SystemID, context.MinStartDateTime, context.UnavailableTimes.Count));
      
      return r.AsSuccess(true);
    }

    /// ***********************************************************
    public override Result<bool> PreScheduleJobs(IPsDiscreteJobGeneralScheduleContext context)
    {
      var r = new Result<bool>();
      
      _api.Util.LogCustom.WriteInformation("PreScheduleJobs", "All Discrete Hooks", @"System.Count = {0}
JobUnscheduled.Count = {1}
JobToSchedule.Count = {2}
JobAssigned.Count = {3}".FormatWith(context.SystemList.Count, context.JobsUnscheduled.Count, context.JobsToSchedule.Count, context.JobsAssigned.Count));
      
      return r.AsSuccess(true);
    }
    
    /// ***********************************************************
    public override Result<bool> PreScheduleInfoForJobSystems(IPsDiscreteJobGeneralScheduleInfoForJobSystemsContext context)
    {
      var r = new Result<bool>();
      
      _api.Util.LogCustom.WriteInformation("PreScheduleInfoForJobSystems", "All Discrete Hooks", @"System.Count = {0}
JobUnscheduled.Count = {1}
JobToSchedule.Count = {2}
JobAssigned.Count = {3}".FormatWith(context.SystemList.Count, context.JobsUnscheduled.Count, context.JobsToSchedule.Count, context.JobsAssigned.Count));
      
      return r.AsSuccess(true);
    }
    
    /// ******************************************************************
    public override Result<DbProduct> LoadProduct(IPsDiscreteJobProductContext context)
    {
      var r = new Result<DbProduct>(); 

      _api.Util.LogCustom.WriteInformation("LoadProduct", "All Discrete Hooks", @"Job.ID = {0}
System.ID = {1}
Job.RequestedProductCode = {2}".FormatWith(context.JobInfo.JobID, context.SysInfo.SystemID, context.JobInfo.PsRequestedProductCode));

      return r.AsSuccess(context.Product);
    }
    
    /// ***********************************************************
    public override Result<bool> UpdateJobDuration(IPsDiscreteJobDurationContext context) 
    {
      var r = new Result<bool>();
      
      string data = string.Empty;
      if(context.PreviousJob != null)     data += "PreviousJob.ID = {0} ".FormatWith(context.PreviousJob.JobInfo.JobID);
      if(context.JobInfo != null)         data += "CurrentJob.ID = {0} ".FormatWith(context.JobInfo.JobID);
      if(context.Product != null)         data += "CurrentProduct.ID = {0} ".FormatWith(context.Product.ID);
      if(context.JobInfo != null)         data += "CurrentJob.Duration = {0} ".FormatWith(context.JobInfo.Duration.TotalSeconds);
      if(context.SysInfo != null)         data += "System.ID = {0} ".FormatWith(context.ChangeoverType.ID);
      if(context.ChangeoverType != null)  data += "ChangeoverType.ID = {0} ".FormatWith(context.SysInfo.SystemID);
      
      _api.Util.LogCustom.WriteInformation("UpdateJobDuration", "All Discrete Hooks", data);
      
      return r.AsSuccess(true);
    }

    /// ***********************************************************
    public override Result<bool> ShouldScheduleJobOnSystem(IPsDiscreteJobSystemScheduleContext context) 
    {
      var r = new Result<bool>();
      
      string data = string.Empty;
      if(context.PreviousJob != null)   data += "PreviousJob.ID = {0} ".FormatWith(context.PreviousJob.JobInfo.JobID);
      if(context.JobInfo != null)       data += "CurrentJob.ID = {0} ".FormatWith(context.JobInfo.JobID);
      if(context.JobsAssigned != null)  data += "JobsAssigned.Count = {0} ".FormatWith(context.JobsAssigned.Count);
      _api.Util.LogCustom.WriteInformation("ShouldScheduleJobOnSystem", "All Discrete Hooks", data);
      
      return r.AsSuccess(true);
    }

     /// ***********************************************************
     public override Result<DateTimeOffset> CalculateMinStartDateTimeForJobOnSystem(IPsDiscreteJobSystemScheduleContext context)
     {
       var r = new Result<DateTimeOffset>();
    
       string data = string.Empty;
       if(context.JobInfo != null)       data += "CurrentJob.ID = {0}".FormatWith(context.JobInfo.JobID);
       if(context.JobsAssigned != null)  data += "JobsAssigned.Count = {0}".FormatWith(context.JobsAssigned.Count);
       _api.Util.LogCustom.WriteInformation("CalculateMinStartDateTimeForJobOnSystem", "All Discrete Hooks", data);

       return r.AsSuccess(context.SysInfo.MinStartDateTime);
     }    
    
    /// ***********************************************************
    public override Result<DateTimeOffset>CalculateNextPlannedStartDateTime(IPsDiscreteJobSchedulePlannedContext context)
    {
      var r = new Result<DateTimeOffset>();
      
      string data = string.Empty;
      if(context.JobInfo != null)                data += "Job.ID = {0}".FormatWith(context.JobInfo.JobID);
      if(context.PlannedStartDateTime != null)   data += "Job.PlannedStart = {0}".FormatWith(context.PlannedStartDateTime);
      if(context.PlannedDuration != null)        data += "Job.PlannedDuration = {0}".FormatWith(context.PlannedDuration.TotalSeconds);
      if(context.SystemInfo != null)             data += "System.ID = {0}".FormatWith(context.SystemInfo.SystemID);
      _api.Util.LogCustom.WriteInformation("CalculateNextPlannedStartDateTime", "All Discrete Hooks", data);
            
      return r.AsSuccess(context.PlannedStartDateTime);
    }
      
   /// ***********************************************************
   public override Result<bool> InitScores(IPsDiscreteJobScoreInitContext context)
   {
     var r = new Result<bool>();
     
     _api.Util.LogCustom.WriteInformation("InitScores", "All Discrete Hooks", @"ScoreList.Count = {0}".FormatWith(context.ScoreList.Count));
      
     return r.AsSuccess(true);
   }

   /// ***********************************************************
   public override Result<bool> UpdateScores(IPsDiscreteJobScoreCalculatedContext context)
   {
     var r = new Result<bool>();
     
     _api.Util.LogCustom.WriteInformation("UpdateScores", "All Discrete Hooks", @"CurrentJob.ID = {0}
ScoreInfo.TotalScore = {1}
".FormatWith(context.ScoreInfo.JobInfo.JobID, context.ScoreInfo.TotalScore));
      
     return r.AsSuccess(true);
   }

   /// ***********************************************************
   public override Result<double> CalculateLineAffinityScore(IPsDiscreteJobScoreContext context)
   {
     var r = new Result<double>();
      
     _api.Util.LogCustom.WriteInformation("CalculateLineAffinityScore", "All Discrete Hooks", @"CurrentJob.ID = {0}
System.ID = {1}
".FormatWith(context.ScoreInfo.JobInfo.JobID, context.ScoreInfo.SystemInfo.SystemID));

     return r.AsSuccess(0);
   }

   /// ***********************************************************
   public override Result<double> CalculateCustomScore(IPsDiscreteJobScoreCustomContext context)
   {
     var r = new Result<double>();
      
     _api.Util.LogCustom.WriteInformation("CalculateCustomScore", "All Discrete Hooks", @"ScoreType = {0}
Job.ID = {1}
System.ID = {2}
".FormatWith(context.ScoreType, context.ScoreInfo.JobInfo.JobID, context.ScoreInfo.SystemInfo.SystemID));
      
     return r.AsSuccess(0);
   }

    /// ***********************************************************
    public override Result<bool> PostScheduleInfoForJobSystems(IPsDiscreteJobGeneralScheduleInfoForJobSystemsContext context)
    {
      var r = new Result<bool>();
      
      _api.Util.LogCustom.WriteInformation("PostScheduleInfoForJobSystems", "All Discrete Hooks", @"BestJob.JobID = {4}
BestJob.SystemID = {5}
System.Count = {0}
JobUnscheduled.Count = {1}
JobToSchedule.Count = {2}
JobAssigned.Count = {3}".FormatWith(context.SystemList.Count, context.JobsUnscheduled.Count, context.JobsToSchedule.Count, context.JobsAssigned.Count, context.BestJob.JobInfo.JobID, context.BestJob.SystemInfo.SystemID));
      
      return r.AsSuccess(true);
    }
    
    /// ***********************************************************
    public override Result<bool> PostScheduleJobs(IPsDiscreteJobGeneralScheduleContext context)
    {
      var r = new Result<bool>();
      
      _api.Util.LogCustom.WriteInformation("PostScheduleJobs", "All Discrete Hooks", @"System.Count = {0}
JobUnscheduled.Count = {1}
JobToSchedule.Count = {2}
JobAssigned.Count = {3}".FormatWith(context.SystemList.Count, context.JobsUnscheduled.Count, context.JobsToSchedule.Count, context.JobsAssigned.Count));
      
      return r.AsSuccess(true);
    }
    
    /// ******************************************************************
    public override Result<bool> PostScheduleInfoCompleteWithUnscheduledJobs(IPsDiscreteJobGeneralScheduleContext context)
    {
      var r = new Result<bool>();
      
      _api.Util.LogCustom.WriteInformation("PostScheduleInfoCompleteWithUnscheduledJobs", "All Discrete Hooks", @"System.Count = {0}
JobUnscheduled.Count = {1}
JobToSchedule.Count = {2}
JobAssigned.Count = {3}".FormatWith(context.SystemList.Count, context.JobsUnscheduled.Count, context.JobsToSchedule.Count, context.JobsAssigned.Count));
      
      return r.AsSuccess(true);
    }
    
    /// ***********************************************************
    public override Result<bool> PreSavePsStageJobs(IPsDiscreteJobGeneralSaveContext context)
    {
      var r = new Result<bool>();
      
      _api.Util.LogCustom.WriteInformation("PreSavePsStageJobs", "All Discrete Hooks", @"JobUnscheduled.Count = {0}
JobAssigned.Count = {1}".FormatWith(context.JobsUnscheduled.Count, context.JobsAssigned.Count));
      
      return r.AsSuccess(true);
    }

    /// ***********************************************************
    public override Result<bool> PostSavePsStageJobs(IPsDiscreteJobGeneralSaveContext context)
    {
      var r = new Result<bool>();
      
      _api.Util.LogCustom.WriteInformation("PostSavePsStageJobs", "All Discrete Hooks", @"JobUnscheduled.Count = {0}
JobAssigned.Count = {1}".FormatWith(context.JobsUnscheduled.Count, context.JobsAssigned.Count));
      
      return r.AsSuccess(true);
    }

    /// ***********************************************************
    public override Result<bool> PostUowExecute() 
    {
      var r = new Result<bool>();
      
      _api.Util.LogCustom.WriteInformation("PostUowExecute", "All Discrete Hooks", @"");
      
      return r.AsSuccess(true);
    }
  }
}