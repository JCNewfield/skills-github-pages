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
  public class Debug_Batch_Hooks : PsBatchJobPluginBase
  {
    private readonly ApiService _api;
    private IPsStatusUpdater _updater;

    public PsScheduleSettings Settings { get; set; }

    /// ******************************************************************
    public Debug_Batch_Hooks(ApiService api)
    {
      _api = api;
      this.Settings = null;
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
    /// <summary>
    /// This method determines if the context.JobInfo and the
    /// context.SysInfo should be scheduled. If Result.Return is false
    /// then the current scoring loop will not consider the combination
    /// as allowed.
    /// </summary>
    /// ***********************************************************
    public override Result<bool> ShouldScheduleJobOnSystem(IPsBatchJobSystemScheduleContext context)
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
    public override Result<bool> PostScheduleJobs(IPsBatchJobGeneralScheduleContext context)
    {
      var r = new Result<bool>();
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