using System;
using System.Data;
using System.Collections.Generic;
using System.Globalization;
using ETS.Core;
using ETS.Core.Api;
using ETS.Core.Api.Models.Data;
using ETS.Core.Enums;
using ETS.Core.Extensions;
using System;
using System.Linq;
using System.Web;
using ETS.Core.Api.Models;
using ETS.Core.Services.Resource;

// Added a single comment for Batch.cs
// A change from TrakSYS to the Local Instance

// Changes have been mate at 23/11/2023 - Pavel Ibrahim - New First Change

namespace ETS.Core.Scripting
{
  /// ******************************************************************
  /// <summary>Class Description</summary>
  /// ******************************************************************
  public class GlobalBatch
  {
    private readonly ApiService api = ETS.Core.Api.ApiService.GetInstance();

    private DbSystem _system;
    private DbJob _job;
    private DbBatch _batch;


    public GlobalBatch(DbSystem system, DbJob job, DbBatch batch){
      _system = system;
      _job = job;
      _batch = batch;
    }

    public void StartJob()
    {}

    public void TriggerNextStep(){
      var uow = api.CreateUnitOfWork();
      var startBatchTag = api.Data.DbTag.Load.ByID(140);
      if (startBatchTag.Value == "1")
      {
        api.Tags.UpdateVirtualTagByID(139, 0, uow).ThrowIfFailed();
      }
    }
   


  }
}