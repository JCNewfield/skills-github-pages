using System;
using System.Collections.Generic;
using System.Data;
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
  public class DemoBatchPlugin : PsBatchJobPluginBase
  {
    private readonly ApiService _api;
    private IPsStatusUpdater _updater;

    public PsScheduleSettings Settings { get; set; }
    public SimSchedUtil _simSched;

    public bool EnableSequentialScheduling = true;
    public bool EnableMaterialConstraints = true;
    
    /// ******************************************************************
    public DemoBatchPlugin(ApiService api, SimSchedUtil simSched)
    {
      _api = api;
      this.Settings = null;
      _simSched = simSched;
    }

    public void SimSchedUtil_Set(SimSchedUtil simSched)
    {
      _simSched = simSched;
    }
    
    public SimSchedUtil SimSchedUtil_Get()
    {
      return _simSched;
    }
    
    /// ******************************************************************
    /// <summary>
    /// This method is the first plugin method to be called.
    /// </summary>
    /// ******************************************************************
    public override Result<bool> Init(IPsGeneralInitContext context)
    {
      var r = new Result<bool>();
      this.Settings = context.Settings;
      
      if(EnableMaterialConstraints) _simSched.MAT_Init();
      
      return r.AsSuccess(true);
    }

    /// ***********************************************************
    public override Result<DateTimeRange> CalculateScheduleRange(PsScheduleSettings settings, DateTimeRange defaultRange)
    {
      var r = new Result<DateTimeRange>();
      return r.AsSuccess(defaultRange);
    }

    /// ******************************************************************
    /// <summary>
    /// This method is called once the DbPsStage has been created and 
    /// therefore allows status messaging to be sent.
    /// </summary>
    /// ***********************************************************
    public override Result<bool> PostCreatePsStage(DbPsStage psStage, IPsStatusUpdater updater)
    { 
      var r = new Result<bool>();
      _updater = updater;
      return r.AsSuccess(true);
    }

    /// ***********************************************************
    /// <summary>
    /// This method determines if the context.Job.ID should be scheduled.
    /// If the Result.Return is false then the Job will not be considered
    /// at all for being scheduled.
    /// </summary>
    /// ***********************************************************
    public override Result<bool> ShouldScheduleJob(IPsBatchJobScheduleContext context)
    {
      var r = new Result<bool>();
            
      if(!_simSched.Util_ShouldScheduleJob(context.Job.ID).Return) { return r.AsSuccess(false); }
      if(EnableSequentialScheduling) { if(!_simSched.SS_ShouldScheduleJob(context.Job.ParentJobID).Return) { return r.AsSuccess(false); } }
      
      return r.AsSuccess(true);
    }

    /// ***********************************************************
    public override Result<List<DbJobBatchComposite>> ResortJobList(List<DbJobBatchComposite> list)
    {
      var r = new Result<List<DbJobBatchComposite>>();
      return r.AsSuccess(list);
    }

    /// ***********************************************************
    /// <summary>
    /// This method determines if the context.System.ID should be scheduled.
    /// If Result.Return is false then the System will not be considered at
    /// all for being scheduled.
    /// </summary>
    /// ***********************************************************
    public override Result<bool> ShouldScheduleForSystem(IPsSystemBatchScheduleContext context)
    {
      var r = new Result<bool>();
      return r.AsSuccess(true);
    }

    /// ***********************************************************
    public override Result<DateTimeOffset> CalculateMinStartDateTimeForSystem(IPsSystemBatchScheduleContext context)
    {
      var r = new Result<DateTimeOffset>();
      return r.AsSuccess(context.MinStart);
    }

    /// ***********************************************************
    public override Result<bool> PreScheduleJobs(IPsBatchJobGeneralScheduleContext context)
    {
      var r = new Result<bool>();
      return r.AsSuccess(true);
    }

    /// ***********************************************************
    public override Result<bool> PreScheduleInfoForJobSystems(IPsBatchJobGeneralScheduleInfoForJobSystemsContext context)
    {
      var r = new Result<bool>();
      
      if(EnableMaterialConstraints)
      {
        foreach (var job in context.JobsToSchedule)
        {
          _simSched.MAT_PreScheduleInfoForJobSystems(job.JobID, job.Source.PlannedStartDateTime);
        }
      }
      
      return r.AsSuccess(true);
    }
    
    /// ***********************************************************
    public override Result<DateTimeOffset>CalculateMinStartDateTimeForJobOnSystem(IPsBatchJobSystemScheduleContext context)
    {
      var r = new Result<DateTimeOffset>();
      var dtoList = new List<DateTimeOffset>();
      dtoList.Add(context.JobInfo.Source.PlannedStartDateTime);
      dtoList.Add(context.SysInfo.MinStartDateTime);
      
      if(EnableSequentialScheduling)
      {
        var ssResult = _simSched.SS_CalculateMinStartDateTimeForJobOnSystem(context.JobInfo.JobID, context.JobInfo.Source.ParentJobID, context.JobInfo.Source.PlannedStartDateTime);
        if(!ssResult.Success) { return r.AsErrorWithMessages(ssResult.Messages); }
        dtoList.Add(ssResult.Return);
      }
      
      if(EnableMaterialConstraints)
      {
        var matResult = _simSched.MAT_CalculateMinStartDateTimeForJobOnSystem(context.JobInfo.JobID);
        if(!matResult.Success) { return r.AsErrorWithMessages(matResult.Messages); }
        dtoList.Add(matResult.Return);
      }
        
      var utilResult = _simSched.Util_ReturnLaterDateTimeOffset(dtoList);
      if(!utilResult.Success) { return r.AsError(utilResult.Messages[0].AsString()); }
      
      return r.AsSuccess(utilResult.Return);
    }
    
    /// ***********************************************************
    /// <summary>
    /// This method returns the recipeID that the job and system combination
    /// should be run. If -1 is returned, no recipe will be chosen, and
    /// therefore th combination will not be allowed for the current sorting loop.
    /// </summary>
    /// ***********************************************************
    public override Result<int> CalculateRecipeToUse(IPsBatchJobRecipeContext context)
    {
      var r = new Result<int>();
      if (context.Recipe == null) return r.AsSuccess(-1);
      return r.AsSuccess(context.Recipe.ID);
    }

    /// ***********************************************************
    public override Result<bool> UpdateJobDuration(IPsBatchJobDurationContext context)
    {
      var r = new Result<bool>();
      return r.AsSuccess(true);
    }

    /// ***********************************************************
    public override Result<bool> InitScores(IPsBatchJobScoreInitContext context)
    {
      var r = new Result<bool>();
      return r.AsSuccess(true);
    }
  
    /// ***********************************************************
    public override Result<bool> UpdateScores(IPsBatchJobScoreCalculatedContext context)
    {
      var r = new Result<bool>();
      return r.AsSuccess(true);
    }

    /// ***********************************************************
    public override Result<double> CalculateLineAffinityScore(IPsBatchJobScoreContext context)
    {
      var r = new Result<double>();
      return r.AsSuccess(0);
    }

    /// ***********************************************************
    public override Result<double> CalculateCustomScore(IPsBatchJobScoreCustomContext context)
    {
      var r = new Result<double>();
      return r.AsSuccess(0);
    }

    /// ***********************************************************
    public override Result<bool> PostScheduleInfoForJobSystems(IPsBatchJobGeneralScheduleInfoForJobSystemsContext context)
    {
      var r = new Result<bool>();
      
      if(EnableSequentialScheduling)
      {
        var rSS = _simSched.SS_PostScheduleInfoForJobSystems(context.BestJob.JobInfo.Source.ParentJobID, context.BestJob.PlannedDuration, context.BestJob.PlannedStartDateTime);
        if(!rSS.Success) { return r.AsError(rSS.Messages[0].AsString()); }
      }
      
      if(EnableMaterialConstraints)
      {
        var rMat = _simSched.MAT_PostScheduleInfoForJobSystems(context.BestJob.JobInfo.JobID);
      }
      return r.AsSuccess(true);
    }
    
    /// ***********************************************************
    public override Result<bool> PostScheduleJobs(IPsBatchJobGeneralScheduleContext context)
    {
      var r = new Result<bool>();

      List<int> idList = context.JobsUnscheduled.Select(c => c.JobID).Distinct().ToList();
      _simSched.Util_PostScheduleJobs(idList);
      
      if(EnableSequentialScheduling)
      {
        List<int> parentIdList = context.JobsUnscheduled.Select(c => c.Source.ParentJobID).Distinct().ToList();
        _simSched.SS_PostScheduleJobs(parentIdList);
      }
      
      return r.AsSuccess(true);
    }

    /// ***********************************************************
    public override Result<bool> PreSavePsStageJobs(IPsBatchJobGeneralSaveContext context)
    {
      var r = new Result<bool>();
      return r.AsSuccess(true);
    }

    /// ***********************************************************
    public override Result<bool> PostSavePsStageJobs(IPsBatchJobGeneralSaveContext context)
    {
      var r = new Result<bool>();
      
      return r.AsSuccess(true);
    }

    /// ***********************************************************
    public override Result<bool> PostUowExecute()
    {
      var r = new Result<bool>();
      return r.AsSuccess(true);
    }
  }
}