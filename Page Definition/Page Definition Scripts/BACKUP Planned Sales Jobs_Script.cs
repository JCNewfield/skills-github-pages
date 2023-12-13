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
  public partial class DiscreteJobs : ContentPageBase
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

      // Get instance of api service
      var api = ETS.Core.Api.ApiService.GetInstance();

      // Get the selected job's ID
      int jobID = e.GetInteger("JobID");
      JobID = jobID;

      DbJobDiscreteComposite _job = this.Ets.Api.Data.DbJobDiscreteComposite.Load.ByID(JobID).ThrowIfLoadFailed("JobID", this.JobID);
      DbJob _job2 = this.Ets.Api.Data.DbJob.Load.ByID(JobID).ThrowIfLoadFailed("JobID", this.JobID);
      this.Ets.Values["Job.Name"] = _job.Name;

      Trace.Write("The integer jobID's Value is: " + jobID);

      var uow = this.Ets.Api.CreateUnitOfWork();
      var product = this.Ets.Api.Data.DbProduct.Load.ByID(_job.ProductID).ThrowIfLoadFailed("Job.ProductID", _job.ProductID);

      Trace.Write("Tags have not been changed.");
      // Load the material based on RecipeID
      // Declare a list object to store items
      List<ETS.Core.Api.Models.Data.DbItem> items = new List<ETS.Core.Api.Models.Data.DbItem>();

      // Load the list of items from the database
      items = api.Data.ListOf.DbItems.GetList.WithSql("SELECT * FROM tItem");

      // Define a list of LocationIDs that you want to check (7, 8, and 9 in this case)

      if (_job2.ProductID == 8)
      {
        _material = this.Ets.Api.Data.DbMaterial.Load.ByID(3).ThrowIfLoadFailed("ID", _job2.ProductID);
        // Find the items that match the MaterialID and LocationIDs in items list
        int locationIDsToCheck = 19;
        var matchingItems = items.Where(i => i.MaterialID == _material.ID && locationIDsToCheck == i.LocationID);
        this.Ets.Api.Tags.UpdateVirtualTagByID(416, 1, uow);

        // Find the item with the lowest ID among the matching items
        var itemWithLowestID = matchingItems.OrderBy(i => i.ID).FirstOrDefault();
        if (itemWithLowestID != null)
        {

          itemWithLowestID.LocationID = 20;
          itemWithLowestID.Attribute10 = _job2.Capture01;

          api.Data.DbItem.Save.UpdateExisting(itemWithLowestID);
          ETS.Core.Api.Models.Data.DbItemLog itemLog = new ETS.Core.Api.Models.Data.DbItemLog();


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
          //itemLog.MaterialID = 3;
          itemLog.Capture10 = itemWithLowestID.Attribute10;

          ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbItemLog> result2;

          result2 = api.Data.DbItemLog.Save.InsertAsNew(itemLog);
        }

      }
      if (_job2.ProductID == 9)
      {
        _material = this.Ets.Api.Data.DbMaterial.Load.ByID(1).ThrowIfLoadFailed("ID", _job2.ProductID);
        int locationIDsToCheck = 18;
        // Find the items that match the MaterialID and LocationIDs in items list
        var matchingItems = items.Where(i => i.MaterialID == _material.ID && locationIDsToCheck == i.LocationID);
        this.Ets.Api.Tags.UpdateVirtualTagByID(413, 1, uow);
        // Find the item with the lowest ID among the matching items
        var itemWithLowestID = matchingItems.OrderBy(i => i.ID).FirstOrDefault();
        if (itemWithLowestID != null)
        {

          itemWithLowestID.LocationID = 20;
          itemWithLowestID.Attribute10 = _job2.Capture01;

          api.Data.DbItem.Save.UpdateExisting(itemWithLowestID);
          ETS.Core.Api.Models.Data.DbItemLog itemLog = new ETS.Core.Api.Models.Data.DbItemLog();


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
          //itemLog.MaterialID = 1;
          itemLog.Capture10 = itemWithLowestID.Attribute10;

          ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbItemLog> result3;

          result3 = api.Data.DbItemLog.Save.InsertAsNew(itemLog);
        }

      }
      if (_job2.ProductID == 10)
      {
        _material = this.Ets.Api.Data.DbMaterial.Load.ByID(2).ThrowIfLoadFailed("ID", _job2.ProductID);
        // Find the items that match the MaterialID and LocationIDs in items list
        int locationIDsToCheck = 17;
        var matchingItems = items.Where(i => i.MaterialID == _material.ID && locationIDsToCheck == i.LocationID);
        this.Ets.Api.Tags.UpdateVirtualTagByID(417, 1, uow);
        // Find the item with the lowest ID among the matching items
        var itemWithLowestID = matchingItems.OrderBy(i => i.ID).FirstOrDefault();
        if (itemWithLowestID != null)
        {

          itemWithLowestID.LocationID = 20;
          itemWithLowestID.Attribute10 = _job2.Capture01;

          api.Data.DbItem.Save.UpdateExisting(itemWithLowestID);
          ETS.Core.Api.Models.Data.DbItemLog itemLog = new ETS.Core.Api.Models.Data.DbItemLog();


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
          //itemLog.MaterialID = 2;
          itemLog.Capture10 = itemWithLowestID.Attribute10;

          ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbItemLog> result;

          result = api.Data.DbItemLog.Save.InsertAsNew(itemLog);
        }

      }

      this.Ets.Api.Tags.UpdateVirtualTagByID(432, _job.Name, uow);
      this.Ets.Api.Tags.UpdateVirtualTagByID(433, product.Name, uow);
      this.Ets.Api.Tags.UpdateVirtualTagByID(592, 1, uow);

      Trace.Write("Tags have been changed.");

      uow.ExecuteReturnsResultObject().ThrowIfFailed();

      // var args = new Dictionary<string, string>();
      // args.Add("c", "ETS.Application.Wait.WaitForJobStart");
      // args.Add("SystemID", _sys.ID.AsString());
      // args.Add("JobName", _job.Name);
      // var url = this.Ets.BuildUrl("", args);

      var url = this.Ets.Pages.PageUrl;
      this.Ets.Pages.RedirectToUrl(url);

    }

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
        var url = this.Ets.Pages.PageUrl;
        this.Ets.Pages.RedirectToUrl(url);
      }
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
        job.RecipeID = 2;
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
        var url = this.Ets.Pages.PageUrl;
        this.Ets.Pages.RedirectToUrl(url);
      }
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
        job.RecipeID = 2;
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
        var url = this.Ets.Pages.PageUrl;
        this.Ets.Pages.RedirectToUrl(url);
      }
    }


  }
}