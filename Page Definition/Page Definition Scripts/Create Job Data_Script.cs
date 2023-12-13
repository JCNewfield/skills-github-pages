using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using ETS.Core.Api;
using ETS.Core.Api.Models;
using ETS.Core.Api.Models.Data;
using ETS.Core.Enums;
using ETS.Core.Extensions;
using ETS.Core.Services.Resource;
using ETS.Ts.Core.ContentParts;
using ETS.Ts.Core.Enums;
using ETS.Ts.Core.Scripting;

namespace ETS.Ts.Content
{
  ///***********************************************************
  public partial class CreateJobData : ContentPageBase
  {
    [ValuesProperty()]
    public int SystemID { get; set; } = 1;
       
    ///***********************************************************
    protected override bool ContentPage_Init()
    {
      this.Ets.Values["Trace"] = 1;
      
      // load current batch
      var batch = this.Ets.Api.Data.DbBatch.Load.CurrentBySystemID(1).ThrowIfLoadFailed("SystemID", 1);
      
      // start first step
      var steps = this.Ets.Api.Data.DbBatchStep.GetList.ForBatchID(batch.ID).ThrowIfLoadFailed("batch.ID", batch.ID);
      var function = this.Ets.Api.Data.DbFunctionDefinition.Load.ByID(steps[0].FunctionDefinitionID).ThrowIfLoadFailed("steps[0].FunctionDefinitionID", steps[0].FunctionDefinitionID);
      this.Ets.Api.Tags.UpdateVirtualTagByID(function.TriggerTagID, true).ThrowIfFailed();
                 
      // redirect
      this.Ets.Pages.RedirectToUrl(this.Ets.Pages.FolderUrl); 
      return true;
    }
  }
}
