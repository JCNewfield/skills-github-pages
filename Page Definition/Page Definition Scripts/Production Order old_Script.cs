using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using System.Web.UI.DataVisualization.Charting;
using ETS.Core.Api;
using ETS.Core.Api.Models;
using ETS.Core.Api.Models.Data;
using ETS.Core.Enums;
using ETS.Core.Extensions;
using ETS.Core.Scripting;
using ETS.Core.Services.Resource;
using ETS.Ts.Core.ContentParts;
using ETS.Ts.Core.Enums;
using ETS.Ts.Core.Scripting;

namespace ETS.Ts.Content
{
  /// ***********************************************************
  /// <templateauthor>Parsec</templateauthor>
  /// <templateversion>Created from template 10.0.0</templateversion>
  /// <templateinstructions>
  /// The term "Entity" is used in the following code to represent
  /// a type of TrakSYS entity (Product, Material, Event, etc...).
  /// 
  /// Replace the text "Entity" with an appropraite TrakSYS entity
  /// name (i.e. replace "Entity" with "Product").
  /// 
  /// Uncomment and alter code as needed.
  /// </templateinstructions>
  /// ***********************************************************
  public partial class Order : ContentPageBase
  {
    /// Automatically map the decorated properties to incoming Ets.Values data (typically QueryString variables).
    /// The name of the property is used as the Key in accessing the Ets.Values collection during mapping.

    // actual entity id (used for editing)
    public int systemID { get; set; } = -1;
    public int subsystemID { get; set; } = -1;
    private DbSystem system;
    private DbSystem subsystem;

    // private DbProduct model;


    /// ***********************************************************
    protected override bool ContentPage_Init()
    {

      systemID = 1;
      system = this.Ets.Api.Data.DbSystem.Load.ByID(systemID);
      subsystem = this.Ets.Api.Data.DbSystem.Load.WithSql(@"SELECT TOP 1 * FROM tSystem WHERE ParentSystemID =  {0}".FormatWith(systemID)).ThrowIfLoadFailed("System ID", systemID);
      return true;
    }
    /// ***********************************************************
    private void Save_Click(object sender, EventArgs e)
    {

      var job = this.Ets.Api.Data.DbJobBatchComposite.Create.FromParentNone();
      var jobNumber = this.Ets.Api.Util.Db.ExecuteScalar<int>(@"SELECT Value FROM tSequence WHERE Name = 'JobID'").Return + 1;
      var jobName = @"P-{0}".FormatWith(jobNumber);
      job.Name = jobName;
      job.ProductID = this.Ets.Form.UpdateModelValueByFormKey("Model.ProductID", job.ProductID, "Product");
      // job.Capture01 = this.Ets.Form.UpdateModelValueByFormKey("Model.Address", job.Capture01, "Address");
      // job.Capture02 = this.Ets.Form.UpdateModelValueByFormKey("Model.Customer", job.Capture02, "Customer");
      //job.PlannedStartDateTime = DateTime.Now.Date;
      job.PlannedDurationSeconds = 100;
      job.JobTypeID = 1;

      job.PlannedNumberOfBatches = 1;
      job.PlannedBatchSize = 1;
      job.PlannedBatchSizeUnits = "kg";
      job.PsSetID = 1;
      job.PsRequestedPriority = 1;
      if (job.ProductID == 1)
      {
        job.PsRequestedProductCode = "B.SALMON";
        job.RecipeID = 2;
      }
      if (job.ProductID == 2)
      {
        job.PsRequestedProductCode = "B.COD";
        job.RecipeID = 1;
      }
      if (job.ProductID == 3)
      {
        job.PsRequestedProductCode = "B.TROUT";
        job.RecipeID = 3;
      }
      // Getting date locally from computer
      DateTime datetime = DateTime.Now;
      // Converting datetime to string (Otherwise Capture04 won't read it and there will be an error)
      job.Capture04 = datetime.ToString("dd-MM-yyyy HH:mm:ss");


      this.Ets.Api.Data.DbJobBatchComposite.Save.ValidateIgnoredForInsertAsNew(job);
      

      // validate the entity
      if (!this.ValidateModelData()) return;

      // save the entity
      if (!this.SaveModelData()) return;

      // run create job batch script after job has been created


      // has successfully saved, redirect to success
      var url = this.Ets.Pages.PageUrl;
      this.Ets.Pages.RedirectToUrl(url);

    }
    /// ***********************************************************
    private bool ValidateModelData()
    {
      // return validation results
      return this.Ets.Form.IsValid;
    }

    /// ***********************************************************
    private bool SaveModelData()
    {
      var uow = this.Ets.Api.CreateUnitOfWork();
      // save model (validation can be ignored as it has already been done)


      // perform any related save operations (sinks, tag updates, etc...)
      var result = uow.ExecuteReturnsResultObject();

      if (!result.Success)
      {
        this.Ets.Debug.FailFromResultMessages(result.Messages);
      }

      // execute final save
      return uow.ExecuteReturnsResultObject().ThrowIfFailed();
    }
  }
}