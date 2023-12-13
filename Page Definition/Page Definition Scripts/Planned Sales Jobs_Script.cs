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
using ETS.Core.Services.Resource;
using ETS.Ts.Core.ContentParts;
using ETS.Ts.Core.Enums;
using ETS.Ts.Core.Scripting;

namespace ETS.Ts.Content
{
  /// ***********************************************************
  public partial class salesJobs : ContentPageBase
  {
    /// Automatically map the decorated property to incoming Ets.Values data (typically QueryString variables).
    /// The name of the property is used as the Key in accessing the Ets.Values collection during mapping.
    [ValuesProperty()]
    public int SystemID { get; set; } = 6;

    [ValuesProperty()]
    public int JobID { get; set; } = -1;

    public DbSystem _sys;
    DbMaterial _material;

    // ***********************************************************
    protected override bool ContentPage_Init()
    {
      this.Page.Trace.IsEnabled = true;
      this.Ets.Values["JobID"] = this.Ets.Values.GetAsInt("data.BatchJobs.Selected.JobID");

      _sys = this.Ets.Api.Data.DbSystem.Load.ByID(this.SystemID).ThrowIfLoadFailed("ID", this.SystemID);

      this.Ets.Values["System.Name"] = _sys.Name;
      this.Ets.Values["Red.Button.HasNoData"] = false;

      return true;
    }

    // ***********************************************************
    protected override bool ContentPage_PartPreInit10()
    {
      // this.Ets.Values["JobID"] = this.Ets.Values.GetAsInt("data.BatchJobs.Selected.JobID");
      return true;
    }

    private void StartDiscreteJob(object sender, RowItemEventArgs e)
    {

      // Retrieve the API service instance
      var api = ETS.Core.Api.ApiService.GetInstance();

      // Get the ID of the selected job
      int jobID = e.GetInteger("JobID");
      JobID = jobID;

      // Load job information using the provided ID
      DbJobDiscreteComposite _job = this.Ets.Api.Data.DbJobDiscreteComposite.Load.ByID(JobID).ThrowIfLoadFailed("JobID", this.JobID);
      DbJob _job2 = this.Ets.Api.Data.DbJob.Load.ByID(JobID).ThrowIfLoadFailed("JobID", this.JobID);

      // Store the job name in the values dictionary
      this.Ets.Values["Job.Name"] = _job.Name;

      // Create a Unit of Work instance
      var uow = this.Ets.Api.CreateUnitOfWork();

      // Load the associated product using the job's ProductID
      var product = this.Ets.Api.Data.DbProduct.Load.ByID(_job.ProductID).ThrowIfLoadFailed("Job.ProductID", _job.ProductID);

      // Declare a list object to store items
      List<ETS.Core.Api.Models.Data.DbItem> items = new List<ETS.Core.Api.Models.Data.DbItem>();

      // Load the list of items from the database
      items = api.Data.ListOf.DbItems.GetList.WithSql("SELECT * FROM tItem");

      // Check if the product ID is 8 (salmon)
      if (_job2.ProductID == 8)
      {
        // Load the material associated with ID 3 (salmon)
        _material = this.Ets.Api.Data.DbMaterial.Load.ByID(3).ThrowIfLoadFailed("ID", _job2.ProductID);

        // Filter items based on material and location
        int locationIDsToCheck = 19;
        var matchingItems = items.Where(i => i.MaterialID == _material.ID && locationIDsToCheck == i.LocationID);

        // update the tag bpickupRed to 1/active 
        this.Ets.Api.Tags.UpdateVirtualTagByID(416, 1, uow);

        // Find the item with the lowest ID among the matching items
        var itemWithLowestID = matchingItems.OrderBy(i => i.ID).FirstOrDefault();
        if (itemWithLowestID != null)
        {
          // Update item's details for the new location
          itemWithLowestID.LocationID = 20;
          itemWithLowestID.Attribute10 = _job2.Capture01;

          // Save the updated item
          api.Data.DbItem.Save.UpdateExisting(itemWithLowestID);

          // Record item movement in the log
          ETS.Core.Api.Models.Data.DbItemLog itemLog = new ETS.Core.Api.Models.Data.DbItemLog();

          // Populate log details
          itemLog.User = this.Ets.User.Login;
          itemLog.ItemLogDefinitionID = 4;
          itemLog.LogDateTime = this.Ets.SiteNow;
          itemLog.Lot = itemWithLowestID.Lot;
          itemLog.ItemID = itemWithLowestID.ID;
          itemLog.ProductID = 8;
          itemLog.Quantity = 1;
          itemLog.LocationID = 20;
          itemLog.JobID = JobID;
          itemLog.SubLot = itemWithLowestID.Lot;
          itemLog.Notes = "Moved to pick up line";

          itemLog.Capture10 = itemWithLowestID.Attribute10;

          ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbItemLog> result2;

          // Save the item log entry
          result2 = api.Data.DbItemLog.Save.InsertAsNew(itemLog);


        }

      }
      // Check if the product ID is 9 (trout)
      if (_job2.ProductID == 9)
      {
        // Load the material associated with ID 1 (trout)
        _material = this.Ets.Api.Data.DbMaterial.Load.ByID(1).ThrowIfLoadFailed("ID", _job2.ProductID);

        // Filter items based on material and location
        int locationIDsToCheck = 18;
        var matchingItems = items.Where(i => i.MaterialID == _material.ID && locationIDsToCheck == i.LocationID);

        // update the tag bpickupBlue to 1/activ
        this.Ets.Api.Tags.UpdateVirtualTagByID(413, 1, uow);

        // Find the item with the lowest ID among the matching items
        var itemWithLowestID = matchingItems.OrderBy(i => i.ID).FirstOrDefault();
        if (itemWithLowestID != null)
        {
          // Update item's details for the new location
          itemWithLowestID.LocationID = 20;
          itemWithLowestID.Attribute10 = _job2.Capture01;

          // Save the updated item
          api.Data.DbItem.Save.UpdateExisting(itemWithLowestID);

          // Record item movement in the log
          ETS.Core.Api.Models.Data.DbItemLog itemLog = new ETS.Core.Api.Models.Data.DbItemLog();

          // Populate log details
          itemLog.User = this.Ets.User.Login;
          itemLog.ItemLogDefinitionID = 4;
          itemLog.LogDateTime = this.Ets.SiteNow;
          itemLog.Lot = itemWithLowestID.Lot;
          itemLog.ItemID = itemWithLowestID.ID;
          itemLog.ProductID = 9;
          itemLog.Quantity = 1;
          itemLog.LocationID = 20;
          itemLog.JobID = JobID;
          itemLog.SubLot = itemWithLowestID.Lot;
          itemLog.Notes = "Moved to pick up line";

          itemLog.Capture10 = itemWithLowestID.Attribute10;

          ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbItemLog> result3;

          // Save the item log entry
          result3 = api.Data.DbItemLog.Save.InsertAsNew(itemLog);
        }

      }
      // Check if the product ID is 10 (cod)
      if (_job2.ProductID == 10)
      {
        // Load the material associated with ID 2 (cod)
        _material = this.Ets.Api.Data.DbMaterial.Load.ByID(2).ThrowIfLoadFailed("ID", _job2.ProductID);

        // Filter items based on material and location
        int locationIDsToCheck = 17;
        var matchingItems = items.Where(i => i.MaterialID == _material.ID && locationIDsToCheck == i.LocationID);

        // update the tag bpickupWhite to 1/activ
        this.Ets.Api.Tags.UpdateVirtualTagByID(417, 1, uow);

        // Find the item with the lowest ID among the matching items
        var itemWithLowestID = matchingItems.OrderBy(i => i.ID).FirstOrDefault();
        if (itemWithLowestID != null)
        {
          // Update item's details for the new location
          itemWithLowestID.LocationID = 20;
          itemWithLowestID.Attribute10 = _job2.Capture01;

          // Save the updated item
          api.Data.DbItem.Save.UpdateExisting(itemWithLowestID);

          // Record item movement in the log
          ETS.Core.Api.Models.Data.DbItemLog itemLog = new ETS.Core.Api.Models.Data.DbItemLog();

          // Populate log details
          itemLog.User = this.Ets.User.Login;
          itemLog.ItemLogDefinitionID = 4;
          itemLog.LogDateTime = this.Ets.SiteNow;
          itemLog.Lot = itemWithLowestID.Lot;
          itemLog.ItemID = itemWithLowestID.ID;
          itemLog.ProductID = 10;
          itemLog.Quantity = 1;
          itemLog.LocationID = 20;
          itemLog.JobID = JobID;
          itemLog.SubLot = itemWithLowestID.Lot;
          itemLog.Notes = "Moved to pick up line";

          itemLog.Capture10 = itemWithLowestID.Attribute10;

          ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbItemLog> result;

          // Save the item log entry
          result = api.Data.DbItemLog.Save.InsertAsNew(itemLog);
        }

      }

      // sets the appropiate tags so that the job starts
      this.Ets.Api.Tags.UpdateVirtualTagByID(432, _job.Name, uow);
      this.Ets.Api.Tags.UpdateVirtualTagByID(433, product.Name, uow);
      this.Ets.Api.Tags.UpdateVirtualTagByID(592, 1, uow);

      Trace.Write("Tags have been changed.");

      uow.ExecuteReturnsResultObject().ThrowIfFailed();


      var url = "http://10.19.10.4/TS/pages/home/batching/salesjobs/salesstart";
      this.Ets.Pages.RedirectToUrl(url);

    }
    
    // makes an porduction order based o nthe clicked jobs required product
    private void OrderProduction(object sender, RowItemEventArgs e)
    {
      var api = ETS.Core.Api.ApiService.GetInstance();

      // Get the selected job's ID
      int jobID = e.GetInteger("JobID");
      JobID = jobID;

      DbJobDiscreteComposite _job = this.Ets.Api.Data.DbJobDiscreteComposite.Load.ByID(JobID).ThrowIfLoadFailed("JobID", this.JobID);
      DbJob _job2 = this.Ets.Api.Data.DbJob.Load.ByID(JobID).ThrowIfLoadFailed("JobID", this.JobID);

      Trace.Write("The integer jobID's Value is: " + jobID);

      var uow = this.Ets.Api.CreateUnitOfWork();
      var product = this.Ets.Api.Data.DbProduct.Load.ByID(_job.ProductID).ThrowIfLoadFailed("Job.ProductID", _job.ProductID);
      //salmon
      if (_job2.ProductID == 8)
      {
        _material = this.Ets.Api.Data.DbMaterial.Load.ByID(3).ThrowIfLoadFailed("ID", _job2.ProductID);

        ETS.Core.Api.Models.Data.DbJobDiscrete jobDiscrete = new ETS.Core.Api.Models.Data.DbJobDiscrete();

        var job = this.Ets.Api.Data.DbJobBatchComposite.Create.FromParentNone();

        var jobNumber = this.Ets.Api.Util.Db.ExecuteScalar<int>(@"SELECT Value FROM tSequence WHERE Name = 'JobID'").Return + 1;
        var jobName = @"P-{0}".FormatWith(jobNumber);
        job.Name = jobName;
        job.ProductID = 1;

        job.PlannedDurationSeconds = 100;
        job.JobTypeID = 1;

        job.PlannedNumberOfBatches = 1;
        job.PlannedBatchSize = 1;
        job.PlannedBatchSizeUnits = "kg";
        job.PsSetID = 1;
        job.PsRequestedPriority = 1;

        job.PsRequestedProductCode = "B.SALMON";
        job.RecipeID = 2;
        job.Capture07 = "3";

        // Getting date locally from computer
        DateTime datetime = DateTime.Now;
        // Converting datetime to string (Otherwise Capture04 won't read it and there will be an error)
        job.Capture04 = datetime.ToString("dd-MM-yyyy HH:mm:ss");

        jobDiscrete.PlannedProductionCount = 1;
        jobDiscrete.PlannedProductionCountUnits = "Pallet";
        jobDiscrete.PlannedCalculationCount = 1;
        jobDiscrete.PlannedCalculationCountUnits = "Pallet";
        jobDiscrete.JobID = jobNumber;


        this.Ets.Api.Data.DbJobBatchComposite.Save.ValidateIgnoredForInsertAsNew(job);
        ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbJobDiscrete> result;

        // Here we insert the entity in the database (The ID will be created when inserted to the database)
        result = api.Data.DbJobDiscrete.Save.InsertAsNew(jobDiscrete);



        // run create job batch script after job has been created


        // has successfully saved, redirect to success
        var url = "http://10.19.10.4/TS/pages/home/order/productionorder/";
        this.Ets.Pages.RedirectToUrl(url);
      }
      // trout
      if (_job2.ProductID == 9)
      {
        _material = this.Ets.Api.Data.DbMaterial.Load.ByID(1).ThrowIfLoadFailed("ID", _job2.ProductID);
        ETS.Core.Api.Models.Data.DbJobDiscrete jobDiscrete = new ETS.Core.Api.Models.Data.DbJobDiscrete();

        var job = this.Ets.Api.Data.DbJobBatchComposite.Create.FromParentNone();

        var jobNumber = this.Ets.Api.Util.Db.ExecuteScalar<int>(@"SELECT Value FROM tSequence WHERE Name = 'JobID'").Return + 1;
        var jobName = @"P-{0}".FormatWith(jobNumber);
        job.Name = jobName;
        job.ProductID = 3;

        job.PlannedDurationSeconds = 100;
        job.JobTypeID = 1;

        job.PlannedNumberOfBatches = 1;
        job.PlannedBatchSize = 1;
        job.PlannedBatchSizeUnits = "kg";
        job.PsSetID = 1;
        job.PsRequestedPriority = 1;

        job.PsRequestedProductCode = "B.TROUT";
        job.RecipeID = 3;
        job.Capture07 = "1";

        // Getting date locally from computer
        DateTime datetime = DateTime.Now;
        // Converting datetime to string (Otherwise Capture04 won't read it and there will be an error)
        job.Capture04 = datetime.ToString("dd-MM-yyyy HH:mm:ss");

        jobDiscrete.PlannedProductionCount = 1;
        jobDiscrete.PlannedProductionCountUnits = "Pallet";
        jobDiscrete.PlannedCalculationCount = 1;
        jobDiscrete.PlannedCalculationCountUnits = "Pallet";
        jobDiscrete.JobID = jobNumber;


        this.Ets.Api.Data.DbJobBatchComposite.Save.ValidateIgnoredForInsertAsNew(job);
        ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbJobDiscrete> result;

        // Here we insert the entity in the database (The ID will be created when inserted to the database)
        result = api.Data.DbJobDiscrete.Save.InsertAsNew(jobDiscrete);



        // run create job batch script after job has been created


        // has successfully saved, redirect to success
        var url = "http://10.19.10.4/TS/pages/home/order/productionorder/";
        this.Ets.Pages.RedirectToUrl(url);
      }

      //cod
      if (_job2.ProductID == 10)
      {
        _material = this.Ets.Api.Data.DbMaterial.Load.ByID(2).ThrowIfLoadFailed("ID", _job2.ProductID);
        ETS.Core.Api.Models.Data.DbJobDiscrete jobDiscrete = new ETS.Core.Api.Models.Data.DbJobDiscrete();

        var job = this.Ets.Api.Data.DbJobBatchComposite.Create.FromParentNone();

        var jobNumber = this.Ets.Api.Util.Db.ExecuteScalar<int>(@"SELECT Value FROM tSequence WHERE Name = 'JobID'").Return + 1;
        var jobName = @"P-{0}".FormatWith(jobNumber);
        job.Name = jobName;
        job.ProductID = 2;

        job.PlannedDurationSeconds = 100;
        job.JobTypeID = 1;

        job.PlannedNumberOfBatches = 1;
        job.PlannedBatchSize = 1;
        job.PlannedBatchSizeUnits = "kg";
        job.PsSetID = 1;
        job.PsRequestedPriority = 1;

        job.PsRequestedProductCode = "B.COD";
        job.RecipeID = 1;
        job.Capture07 = "2";

        // Getting date locally from computer
        DateTime datetime = DateTime.Now;
        // Converting datetime to string (Otherwise Capture04 won't read it and there will be an error)
        job.Capture04 = datetime.ToString("dd-MM-yyyy HH:mm:ss");

        jobDiscrete.PlannedProductionCount = 1;
        jobDiscrete.PlannedProductionCountUnits = "Pallet";
        jobDiscrete.PlannedCalculationCount = 1;
        jobDiscrete.PlannedCalculationCountUnits = "Pallet";
        jobDiscrete.JobID = jobNumber;


        this.Ets.Api.Data.DbJobBatchComposite.Save.ValidateIgnoredForInsertAsNew(job);
        ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbJobDiscrete> result;

        // Here we insert the entity in the database (The ID will be created when inserted to the database)
        result = api.Data.DbJobDiscrete.Save.InsertAsNew(jobDiscrete);



        // run create job batch script after job has been created


        // has successfully saved, redirect to success
        var url = "http://10.19.10.4/TS/pages/home/order/productionorder/";
        this.Ets.Pages.RedirectToUrl(url);
      }
    }


  }
}