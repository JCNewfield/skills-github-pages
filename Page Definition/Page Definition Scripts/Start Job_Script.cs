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
  public partial class StartBatch : ContentPageBase
  {
    [ValuesProperty()]
    public int JobID { get; set; } = -1;

    [ValuesProperty()]
    public int SystemID { get; set; } = -1;

    DbJobBatchComposite _job;
    ETS.Core.Api.Models.Tags.Tag _jobTag;
    ETS.Core.Api.Models.Tags.Tag _batchTag;
    DbProduct _product;
    DbRecipe _recipe;
    string _batchName;
    DbSystem _system;
    DbMaterial _material;
    public DbItem _item;

    public DbItemLogDefinition _itemLogDef;

    // ***********************************************************
    protected override bool ContentPage_Init()
    {

      this.Page.Trace.IsEnabled = true;

      // Gets current Datetime and then converts to UnixTimeSeconds
      DateTime currentTime = DateTime.Now;
      //long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
      // float unixFloat = (float)unixTime;

      var api = ETS.Core.Api.ApiService.GetInstance();

      var dateTimeTag = api.Data.DbTag.Load.ByID(546);
      string dateTimeTagValue = dateTimeTag.Value;


      Trace.Write("Datetime.Now Value is: " + currentTime);
      Trace.Write("Datetime.Tag Value is: " + dateTimeTagValue);


      var systems = this.Ets.Api.Data.DbSystem.GetList.ForParentSystemID(this.SystemID).ThrowIfLoadFailed("SystemID", this.SystemID);
      _system = systems[0];
      _jobTag = this.Ets.Api.Tags.Load.ByID(27).ThrowIfLoadFailed("IDtagjob", _system.JobTagID);
      _batchTag = this.Ets.Api.Tags.Load.ByID(26).ThrowIfLoadFailed("IDbatchtag", _system.BatchTagID);

      _job = this.Ets.Api.Data.DbJobBatchComposite.Load.ByID(JobID).ThrowIfLoadFailed("JobID", JobID);
      _product = this.Ets.Api.Data.DbProduct.Load.ByID(_job.ProductID).ThrowIfLoadFailed("ProductID", _job.ProductID);

      _recipe = this.Ets.Api.Data.DbRecipe.Load.ByID(_job.RecipeID).ThrowIfLoadFailed("ID", _job.RecipeID);

      _itemLogDef = this.Ets.Api.Data.DbItemLogDefinition.Load.ByID(2).ThrowIfLoadFailed("Code", 2);

      this.Ets.Values["jobName"] = _job.Name;
      return true;
    }

    // *********************************************************************
    protected override bool ContentPage_PartPreInit02()
    {

      return true;
    }

    // ***********************************************************
    protected void StartJob_Click(object sender, EventArgs e)
    {
      try
      {
        if (!StartJob(false)) return;
        var qsparams = new
        {
          batch = _batchName
        };
        var url = this.Ets.ProcessExpressionUrl("WaitForBatchStart?jobname={batch}", qsparams);
        this.Ets.Pages.RedirectToUrl(url);
      }
      catch (Exception ex)
      {
        this.Ets.Debug.FailFromException(ex);
      }

    }

    // ***********************************************************

    // ***********************************************************
    protected bool StartJob(bool RunDemo)
    {
      var uow = this.Ets.Api.CreateUnitOfWork();

      if (RunDemo)
      {
        _job.RecipeID = _recipe.ID;
        this.Ets.Api.Data.DbJobBatchComposite.Save.UpdateExisting(_job, uow);
      }


      var Batch = this.Ets.Values.GetAsString("inputBatchName");
      DbSystem parentSystem = this.Ets.Api.Data.DbSystem.Load.ByID(1).ThrowIfLoadFailed("SystemID", 1);
      var jobName = this.Ets.Values.GetAsString("JobName");
      _batchName = jobName + "-" + this.Ets.Form.GetValueAsStringByKey("inputBatchName");
      var nameCount = this.Ets.Api.Util.Db.ExecuteScalar<int>(@"SELECT COUNT(*) FROM tBatch WHERE Name = {1}".FormatWith(_job.ID.ToSql(), _batchName.ToSql())).ThrowIfFailed();


      if (Batch.IsNullOrWhiteSpace()) { this.Ets.Form.AddValidationError("Batch must be named."); }
      if (nameCount != 0) { this.Ets.Form.AddValidationError("Batch {0} already exists. Please use a new name.".FormatWith(_batchName)); }
      if (!this.Ets.Form.IsValid) return false;

      // Gets a list of items from the database and checks if the materialid and locationid is correct with the recipe and HBW. Then it selects the lowest out of the ids and updates it with locationid 1 which is the MPO
      // Create a reference to the api service
      var api = ETS.Core.Api.ApiService.GetInstance();

      // Declare a list object to store items
      List<ETS.Core.Api.Models.Data.DbItem> items = new List<ETS.Core.Api.Models.Data.DbItem>();

      // Load the list of items from the database
      items = api.Data.ListOf.DbItems.GetList.WithSql("SELECT * FROM tItem");

      // Check if _job.RecipeID is cod
      if (_job?.RecipeID == 1)
      {
        // Load the material based on RecipeID
        _material = this.Ets.Api.Data.DbMaterial.Load.ByID(2).ThrowIfLoadFailed("ID", _job.RecipeID);

        // Define a list of LocationIDs that you want to check (7, 8, and 9 in this case)
        List<int> locationIDsToCheck = new List<int> { 7, 8, 9, 10, 11, 12, 13, 14, 15 };

        // Find the items that match the MaterialID and LocationIDs in the items list
        var matchingItems = items.Where(i => i.MaterialID == _material.ID && locationIDsToCheck.Contains(i.LocationID));

        // Find the item with the lowest ID among the matching items
        var itemWithLowestID = matchingItems.OrderBy(i => i.ID).FirstOrDefault();
        _item = itemWithLowestID;
        this.Ets.Api.Tags.UpdateVirtualTagByID(562, "WHITE", uow).ThrowIfFailed();
        //this.Ets.Api.Tags.UpdateVirtualTagByID(546, DateTime.Now, uow).ThrowIfFailed();
        this.Ets.Api.Tags.Load.ByID(546);

        // Now you have the item you need, which is 'itemWithLowestID'.
        // You can use it as required.

        if (itemWithLowestID != null)
        {
          // Update the 'LocationID' of the matched item to 1
          itemWithLowestID.LocationID = 1;


          api.Data.DbItem.Save.UpdateExisting(itemWithLowestID);
          ETS.Core.Api.Models.Data.DbItemLog itemLog = new ETS.Core.Api.Models.Data.DbItemLog();

          itemLog.User = this.Ets.User.Login;
          itemLog.ItemLogDefinitionID = 2;
          itemLog.LogDateTime = this.Ets.SiteNow;
          itemLog.Lot = itemWithLowestID.Lot;
          itemLog.ItemID = itemWithLowestID.ID;
          itemLog.MaterialID = 2;
          itemLog.Quantity = 1;
          itemLog.LocationID = 1;
          itemLog.JobID = JobID;
          //itemLog.SubLot = itemWithLowestID.Lot;
          itemLog.Notes = "Moved to production";

          ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbItemLog> result;

          // Here we insert the entity in the database (The ID will be created when inserted to the database)
          result = api.Data.DbItemLog.Save.InsertAsNew(itemLog);

        }

      }

      // Check if _job.RecipeID is trout
      if (_job?.RecipeID == 2)
      {
        // Load the material based on RecipeID
        _material = this.Ets.Api.Data.DbMaterial.Load.ByID(3).ThrowIfLoadFailed("ID", _job.RecipeID);

        // Define a list of LocationIDs that you want to check (7, 8, and 9 in this case)
        List<int> locationIDsToCheck = new List<int> { 7, 8, 9, 10, 11, 12, 13, 14, 15 };

        // Find the items that match the MaterialID and LocationIDs in items list
        var matchingItems = items.Where(i => i.MaterialID == _material.ID && locationIDsToCheck.Contains(i.LocationID));

        // Gets current Datetime and then converts to UnixTimeSeconds
        DateTime currentTime = DateTime.UtcNow;
        float unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

        Trace.Write("UnixTime Value is: " + unixTime);

        // Find the item with the lowest ID among the matching items
        var itemWithLowestID = matchingItems.OrderBy(i => i.ID).FirstOrDefault();
        this.Ets.Api.Tags.UpdateVirtualTagByID(562, "RED", uow).ThrowIfFailed();
       // this.Ets.Api.Tags.UpdateVirtualTagByID(546, unixTime, uow).ThrowIfFailed();

        // Now you have the item you need, which is 'itemWithLowestID'.
        // You can use it as required.
        // ...
        if (itemWithLowestID != null)
        {
          // Update the 'LocationID' of the matched item to 1
          itemWithLowestID.LocationID = 1;

          api.Data.DbItem.Save.UpdateExisting(itemWithLowestID);
          ETS.Core.Api.Models.Data.DbItemLog itemLog = new ETS.Core.Api.Models.Data.DbItemLog();


          itemLog.User = this.Ets.User.Login;
          itemLog.ItemLogDefinitionID = 2;
          itemLog.LogDateTime = this.Ets.SiteNow;
          itemLog.Lot = itemWithLowestID.Lot;
          itemLog.ItemID = itemWithLowestID.ID;
          itemLog.MaterialID = 3;
          itemLog.Quantity = 1;
          itemLog.LocationID = 1;
          itemLog.JobID = JobID;
          itemLog.SubLot = itemWithLowestID.Lot;
          itemLog.Notes = "Moved to production";

          ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbItemLog> result;

          result = api.Data.DbItemLog.Save.InsertAsNew(itemLog);


        }
      }
      // Check if _job.RecipeID is salmon
      if (_job.RecipeID == 3)
      {
        // Load the material based on RecipeID
        _material = this.Ets.Api.Data.DbMaterial.Load.ByID(1).ThrowIfLoadFailed("ID", _job.RecipeID);

        // Define a list of LocationIDs that you want to check (7, 8, and 9 in this case)
        List<int> locationIDsToCheck = new List<int> { 7, 8, 9, 10, 11, 12, 13, 14, 15 };

        // Find the items that match the MaterialID and LocationIDs in items list
        var matchingItems = items.Where(i => i.MaterialID == _material.ID && locationIDsToCheck.Contains(i.LocationID));

        // Gets current Datetime and then converts to UnixTimeSeconds
        DateTime currentTime = DateTime.Now;
        //long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
        // float unixFloat = (float)unixTime;

        Trace.Write("Datetime.Now Value is: " + currentTime);

        this.Ets.Api.Tags.UpdateVirtualTagByID(562, "BLUE", uow).ThrowIfFailed();
        //this.Ets.Api.Tags.UpdateVirtualTagByID(546, currentTime, uow).ThrowIfFailed();

        // Find the item with the lowest ID among the matching items
        var itemWithLowestID = matchingItems.OrderBy(i => i.ID).FirstOrDefault();

        if (itemWithLowestID != null)
        {
          itemWithLowestID.LocationID = 1;

          api.Data.DbItem.Save.UpdateExisting(itemWithLowestID);
          ETS.Core.Api.Models.Data.DbItemLog itemLog = new ETS.Core.Api.Models.Data.DbItemLog();

          itemLog.User = this.Ets.User.Login;
          itemLog.ItemLogDefinitionID = 2;
          itemLog.ItemID = itemWithLowestID.ID;
          itemLog.LogDateTime = this.Ets.SiteNow;
          itemLog.Lot = itemWithLowestID.Lot;
          itemLog.MaterialID = 1;
          itemLog.Quantity = 1;
          itemLog.LocationID = 1;
          itemLog.JobID = JobID;
          itemLog.SubLot = itemWithLowestID.Lot;
          itemLog.Notes = "Moved to production";

          ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbItemLog> result;

          result = api.Data.DbItemLog.Save.InsertAsNew(itemLog);

        }
      }


      this.Ets.Api.Tags.UpdateVirtualTagByID(parentSystem.PlannedSizeTagID, _job.PlannedBatchSize, uow).ThrowIfFailed();
      this.Ets.Api.Tags.UpdateVirtualTagByID(29, _product.Name, uow).ThrowIfFailed();
      this.Ets.Api.Tags.UpdateVirtualTagByID(26, _batchName, uow).ThrowIfFailed();
      this.Ets.Api.Tags.UpdateVirtualTagByID(27, jobName, uow).ThrowIfFailed();
      this.Ets.Api.Tags.UpdateVirtualTagByID(139, 1, uow).ThrowIfFailed();

      // this.Ets.Api.Tags.UpdateVirtualTagByName("Discrete.Overall.Job.Name", "{0}.KPI_OVERALL".FormatWith(_job.Name), uow).ThrowIfFailed();

      
      var url = this.Ets.Pages.PageUrl;
      this.Ets.Pages.RedirectToUrl(url);

return uow.ExecuteReturnsResultObject().ThrowIfFailed();
    }
  }
}