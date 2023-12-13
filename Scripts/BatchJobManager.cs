using System;
using System.Collections.Generic;
using System.Linq;
using ETS.Core;
using ETS.Core.Api;
using ETS.Core.Api.Models.Data;
using ETS.Core.Enums;
using ETS.Core.Extensions;

namespace ETS.Core.Scripting
{
  /// ******************************************************************
  /// <summary>Class Description</summary>
  /// ******************************************************************
  public class BatchJobManager
  {
    private readonly ApiService api = ETS.Core.Api.ApiService.GetInstance();

    private DbSystem _system;
    private DbJob _job;
    private DbBatch _batch;

    public BatchJobManager(DbSystem system, DbJob job, DbBatch batch)
    {
      _system = system;
      _job = job;
      _batch = batch;
    }

    public int GetNextSequence()
    {
      string sql = @"
        with AllSequences as 
        ( 
        select StartSequence as [Sequence]
        from tBatchStep
        where StartDateTime is null

        union

        select EndSequence as [Sequence] 
        from tBatchStep
        where EndDateTime is null
        )
        select Min([Sequence])
        from AllSequences";

      int nextSequence = api.Util.Db.ExecuteScalar<int>(sql, 0).Return;

      return nextSequence;
    }

    public bool StartNextSequence()
    {
      try
      {
        int sequence = GetNextSequence();
        StartSequence(sequence);
      }
      catch
      {
        return false;
      }

      return true;
    }

    public void StartSequence(int sequence)
    {
      var batchSteps = api.Data.DbBatchStep.GetList.ForBatchID(_batch.ID);
      var stepsToStart = batchSteps.Where(batchStep => batchStep.StartSequence == sequence);
      var stepsToEnd = batchSteps.Where(batchStep => batchStep.EndSequence == sequence);

      var uow = api.CreateUnitOfWork();

      foreach (DbBatchStep step in stepsToEnd)
      {
        EndStep(step.ID);
      }
      
      foreach (DbBatchStep step in stepsToStart)
      {
        StartStep(step.ID);
      }

      var result = uow.ExecuteReturnsResultObject();
      if(!result.Success) api.Util.Log.WriteErrorsFromResultObject(result, "Debug_StartStep");
    }

    public void StartStep(int batchStepID, IUnitOfWork uow = null)
    {
      var batchStep = api.Data.DbBatchStep.Load.ByID(batchStepID).ThrowIfLoadFailed("BatchStep.ID", batchStepID);
      var fd = api.Data.DbFunctionDefinition.Load.ByID(batchStep.FunctionDefinitionID);
      var subSystem = api.Data.DbSystem.Load.ByID(fd.SubSystemID);

      if(fd.Key == "OVEN.LOAD")
      {
        api.Util.MsSql.Execute("UPDATE tBatchStep SET StartDateTime='{0}' WHERE ID={1}".FormatWith(DateTimeOffset.Now, batchStepID));
        return;
      }

      api.Tags.UpdateVirtualTagByID(subSystem.JobTagID, _job.Name);
      api.Tags.UpdateVirtualTagByID(subSystem.BatchTagID, _batch.Name);
      api.Tags.UpdateVirtualTagByID(fd.TriggerTagID, 1);
      // api.Util.MsSql.Execute("UPDATE tBatchStep SET StartDateTime='{0}' WHERE ID={1}".FormatWith(DateTimeOffset.Now, batchStepID));
    }

    public void EndStep(int batchStepID, IUnitOfWork uow = null)
    {
      var batchStep = api.Data.DbBatchStep.Load.ByID(batchStepID).ThrowIfLoadFailed("BatchStep.ID", batchStepID);
      var fd = api.Data.DbFunctionDefinition.Load.ByID(batchStep.FunctionDefinitionID);
      var subSystem = api.Data.DbSystem.Load.ByID(fd.SubSystemID);

      if(fd.Key == "OVEN.LOAD")
      {
        api.Util.MsSql.Execute("UPDATE tBatchStep SET EndDateTime='{0}' WHERE ID={1}".FormatWith(DateTimeOffset.Now, batchStepID));
        return;
      }

      // api.Util.MsSql.Execute("UPDATE tBatchStep SET EndDateTime='{0}' WHERE ID={1}".FormatWith(DateTimeOffset.Now, batchStepID));
      api.Tags.UpdateVirtualTagByID(fd.TriggerTagID, 0);
      api.Tags.UpdateVirtualTagByID(subSystem.JobTagID, "");
      api.Tags.UpdateVirtualTagByID(subSystem.BatchTagID, "");
    }
  }
}