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
  public partial class SalesOrder : ContentPageBase
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

      // Creating a new job object using a composite factory method
      var job = this.Ets.Api.Data.DbJobDiscreteComposite.Create.FromParentNone();

      // Fetching the next job number from a sequence table and incrementing it
      var jobNumber = this.Ets.Api.Util.Db.ExecuteScalar<int>(@"SELECT Value FROM tSequence WHERE Name = 'JobID'").Return + 1;

      // Generating a job name using the job number
      var jobName = @"S-{0}".FormatWith(jobNumber);
      job.Name = jobName;

      // Updating job's ProductID using a utility function that extracts data from a form
      job.ProductID = this.Ets.Form.UpdateModelValueByFormKey("Model.ProductID", job.ProductID, "Product");

      // Updating job's CustomerID using a similar utility function
      job.Capture01 = this.Ets.Form.UpdateModelValueByFormKey("Model.CustomerID", job.Capture01, "Customer");

      // Setting planned job parameters
      job.PlannedDurationSeconds = 120;
      job.JobTypeID = 2;
      job.PlannedProductionCount = 1;
      job.PlannedProductionCountUnits = "Pallet";
      job.PlannedCalculationCount = 1;
      job.PlannedCalculationCountUnits = "Pallet";

      // Setting Priority and other related parameters
      job.PsSetID = 2;
      job.PsRequestedPriority = 1;

      // Based on different ProductIDs, assigning specific values to job properties
      if (job.ProductID == 8)
      {
        job.PsRequestedProductCode = "P.SALMON";
        job.Capture09 = "1";
      }
      if (job.ProductID == 9)
      {
        job.PsRequestedProductCode = "P.TROUT";
        job.Capture09 = "3";
      }
      if (job.ProductID == 10)
      {
        job.PsRequestedProductCode = "P.COD";
        job.Capture09 = "2";
      }

      // Getting the current date and time
      DateTime datetime = DateTime.Now;

      // Converting datetime to string format (required for Capture04 property)
      job.Capture04 = datetime.ToString("dd-MM-yyyy HH:mm:ss");

      // Saving the job object while ignoring certain validation checks for insertion
      this.Ets.Api.Data.DbJobDiscreteComposite.Save.ValidateIgnoredForInsertAsNew(job);


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