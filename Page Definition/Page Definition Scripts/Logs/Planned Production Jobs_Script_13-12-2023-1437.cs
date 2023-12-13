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
  public partial class PlannedProdJobs : ContentPageBase
  {
    public int SystemID { get; set; } = 1;

    [ValuesProperty()]
    public int JobID { get; set; } = -1;
    public DbSystem _sys;
    DbMaterial _material;

    // ***********************************************************
    protected override bool ContentPage_Init()
    {
      // Setting the "JobID" value from the selected batch job
      this.Ets.Values["JobID"] = this.Ets.Values.GetAsInt("data.BatchJobs.Selected.JobID");

      // Loading system information based on the provided SystemID
      _sys = this.Ets.Api.Data.DbSystem.Load.ByID(this.SystemID).ThrowIfLoadFailed("ID", this.SystemID);

      // Storing the system name in ETS values for reference
      this.Ets.Values["System.Name"] = _sys.Name;

      // Retrieving the "JobID" value again (redundant operation, might be unintentional)
      this.Ets.Values["JobID"] = this.Ets.Values.GetAsInt("data.BatchJobs.Selected.JobID");

      // Acquiring an instance of the ETS API service
      var api = ETS.Core.Api.ApiService.GetInstance();

      // Initializing a list to hold items from the database
      List<ETS.Core.Api.Models.Data.DbItem> items = new List<ETS.Core.Api.Models.Data.DbItem>();

      // Loading a list of items from the database using the API
      items = api.Data.ListOf.DbItems.GetList.WithSql("SELECT * FROM tItem");

      // Defining MaterialIDs to check and lists of LocationIDs for each material
      int[] materialIDsToCheck = { 1, 2, 3 };
      List<int>[] locationIDsToCheck = new List<int>[]
      {
        new List<int> { 7, 8, 9, 10, 11, 12, 13, 14, 15 },
        new List<int> { 7, 8, 9, 10, 11, 12, 13, 14, 15 },
        new List<int> { 7, 8, 9, 10, 11, 12, 13, 14, 15 }
      };

      // Finding items that match MaterialID and LocationIDs for each material
      var matchingItemsMaterial1 = items.Where(i => i.MaterialID == materialIDsToCheck[0] && locationIDsToCheck[0].Contains(i.LocationID));
      var matchingItemsMaterial2 = items.Where(i => i.MaterialID == materialIDsToCheck[1] && locationIDsToCheck[1].Contains(i.LocationID));
      var matchingItemsMaterial3 = items.Where(i => i.MaterialID == materialIDsToCheck[2] && locationIDsToCheck[2].Contains(i.LocationID));

      // Checking for matching items for MaterialID 1
      if (matchingItemsMaterial1.Any())
      {
        // Items with MaterialID 1 and specific LocationIDs exist.
        // Updating a virtual tag accordingly.
        api.Tags.UpdateVirtualTagByID(595, 0); // Tag ID 595 represents Material 1 presence
      }
      else
      {
        // No matching items for MaterialID 1.
        api.Tags.UpdateVirtualTagByID(595, 1); // Tag ID 595 represents Material 1 absence
      }

      // Checking for matching items for MaterialID 2
      if (matchingItemsMaterial2.Any())
      {
        // Matching items for MaterialID 2 exist.
        // Updating a virtual tag accordingly.
        api.Tags.UpdateVirtualTagByID(593, 0); // Tag ID 593 represents Material 2 presence
      }
      else
      {
        // No matching items for MaterialID 2.
        // Triggering a function for MaterialID 2 or updating a virtual tag.
        api.Tags.UpdateVirtualTagByID(593, 1); // Tag ID 593 represents Material 2 absence
      }

      // Checking for matching items for MaterialID 3
      if (matchingItemsMaterial3.Any())
      {
        // Matching items for MaterialID 3 exist.
        // Updating a virtual tag accordingly.
        api.Tags.UpdateVirtualTagByID(594, 0); // Tag ID 594 represents Material 3 presence
      }
      else
      {
        // No matching items for MaterialID 3.
        // Triggering a function for MaterialID 3 or updating a virtual tag.
        api.Tags.UpdateVirtualTagByID(594, 1); // Tag ID 594 represents Material 3 absence
      }

      // Indicating the completion of processing
      return true;

    }

    // *********************************************************************
    protected override bool ContentPage_PartPreInit02()
    {

      return true;
    }
    protected override bool ContentPage_PartPreInit10()
    {

      return true;
    }
    // ***********************************************************

    // starts a job when clicked
    private void StartJob(object sender, RowItemEventArgs e)
    {
      var api = ETS.Core.Api.ApiService.GetInstance();

      // Extract the selected job's ID
      int jobID = e.GetInteger("JobID");
      JobID = jobID;
      // Load the job information from the database
      DbJob _job = this.Ets.Api.Data.DbJob.Load.ByID(JobID).ThrowIfLoadFailed("JobID", this.JobID);

      // Set up the unit of work for database operations
      var uow = this.Ets.Api.CreateUnitOfWork();
      // Load the associated product information
      var product = this.Ets.Api.Data.DbProduct.Load.ByID(_job.ProductID).ThrowIfLoadFailed("Job.ProductID", _job.ProductID);

      // Initialize a list to hold retrieved items
      List<ETS.Core.Api.Models.Data.DbItem> items = new List<ETS.Core.Api.Models.Data.DbItem>();

      // Fetch the list of items from the database
      items = api.Data.ListOf.DbItems.GetList.WithSql("SELECT * FROM tItem");
      // Capture the current date and time
      DateTime currentDateTime = DateTime.Now;

      // Define the reference point for Unix timestamp (January 1, 1970)
      DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

      // Calculate the time difference in seconds to get Unix timestamp
      TimeSpan timeDifference = currentDateTime.ToUniversalTime() - unixEpoch;
      double unixTimestampSeconds = timeDifference.TotalSeconds;

      // Check if the job's associated product is of type "salmon"
      if (_job.ProductID == 1)
      {
        // Load material information for "salmon"
        _material = this.Ets.Api.Data.DbMaterial.Load.ByID(3).ThrowIfLoadFailed("ID", _job.ProductID);

        // Define a list of location IDs to consider
        List<int> locationIDsToCheck = new List<int> { 7, 8, 9, 10, 11, 12, 13, 14, 15 };

        // Filter items that match the material and location criteria
        var matchingItems = items.Where(i => i.MaterialID == _material.ID && locationIDsToCheck.Contains(i.LocationID));

        // Find the item with the lowest ID among the matching items
        var itemWithLowestID = matchingItems.OrderBy(i => i.ID).FirstOrDefault();

        // Update a virtual tag to indicate a change in status
        this.Ets.Api.Tags.UpdateVirtualTagByID(562, "RED", uow).ThrowIfFailed();

        if (itemWithLowestID != null)
        {
          // Update the location of the matched item to the production area
          itemWithLowestID.LocationID = 1;

          // Save the updated item information to the database
          api.Data.DbItem.Save.UpdateExisting(itemWithLowestID);

          // Create a log entry for the item's movement to production
          ETS.Core.Api.Models.Data.DbItemLog itemLog = new ETS.Core.Api.Models.Data.DbItemLog();

          // Populate the log entry details
          itemLog.User = this.Ets.User.Login;
          itemLog.ItemLogDefinitionID = 2;
          itemLog.LogDateTime = this.Ets.SiteNow;
          itemLog.Lot = itemWithLowestID.Lot;
          itemLog.ItemID = itemWithLowestID.ID;
          itemLog.MaterialID = 3;
          itemLog.Quantity = 1;
          itemLog.LocationID = 1;
          itemLog.JobID = JobID;
          //itemLog.SubLot = itemWithLowestID.Lot;
          itemLog.Notes = "Moved to production";

          ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbItemLog> result;

          // Insert the log entry into the database
          result = api.Data.DbItemLog.Save.InsertAsNew(itemLog);
        }
      }

      // Check if the job's associated product is of type "cod"
      if (_job.ProductID == 2)
      {
        // Load the material information for the specific product type
        _material = this.Ets.Api.Data.DbMaterial.Load.ByID(2).ThrowIfLoadFailed("ID", _job.ProductID);

        // Define a list of location IDs to consider
        List<int> locationIDsToCheck = new List<int> { 7, 8, 9, 10, 11, 12, 13, 14, 15 };

        // Find the items that match the MaterialID and LocationIDs in the items list
        var matchingItems = items.Where(i => i.MaterialID == _material.ID && locationIDsToCheck.Contains(i.LocationID));

        // Find the item with the lowest ID among the matching items
        var itemWithLowestID = matchingItems.OrderBy(i => i.ID).FirstOrDefault();

        // Update a virtual tag to indicate a change in status (WHITE color)
        this.Ets.Api.Tags.UpdateVirtualTagByID(562, "WHITE", uow).ThrowIfFailed();


        if (itemWithLowestID != null)
        {
          // Update the location of the matched item to the production area
          itemWithLowestID.LocationID = 1;

          // Save the updated item information to the database
          api.Data.DbItem.Save.UpdateExisting(itemWithLowestID);

          // Create a log entry for the item's movement to production
          ETS.Core.Api.Models.Data.DbItemLog itemLog = new ETS.Core.Api.Models.Data.DbItemLog();

          // Populate the log entry details
          itemLog.User = this.Ets.User.Login;
          itemLog.ItemLogDefinitionID = 2;
          itemLog.LogDateTime = this.Ets.SiteNow;
          itemLog.Lot = itemWithLowestID.Lot;
          itemLog.ItemID = itemWithLowestID.ID;
          itemLog.MaterialID = 2;
          itemLog.Quantity = 1;
          itemLog.LocationID = 1;
          itemLog.JobID = JobID;

          itemLog.Notes = "Moved to production";

          ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbItemLog> result;

          // Insert the log entry into the database
          result = api.Data.DbItemLog.Save.InsertAsNew(itemLog);

        }
      }

      // Check if the job's associated product is of type "trout"
      if (_job.ProductID == 3)
      {
        // Load the material information for the specific product type
        _material = this.Ets.Api.Data.DbMaterial.Load.ByID(1).ThrowIfLoadFailed("ID", _job.ProductID);

        // Define a list of location IDs to consider
        List<int> locationIDsToCheck = new List<int> { 7, 8, 9, 10, 11, 12, 13, 14, 15 };

        // Find the items that match the MaterialID and LocationIDs in the items list
        var matchingItems = items.Where(i => i.MaterialID == _material.ID && locationIDsToCheck.Contains(i.LocationID));

        // Find the item with the lowest ID among the matching items
        var itemWithLowestID = matchingItems.OrderBy(i => i.ID).FirstOrDefault();

        // Update a virtual tag to indicate a change in status (BLUE color)
        this.Ets.Api.Tags.UpdateVirtualTagByID(562, "BLUE", uow).ThrowIfFailed();


        if (itemWithLowestID != null)
        {
          // Update the location of the matched item to the production area
          itemWithLowestID.LocationID = 1;

          // Save the updated item information to the database
          api.Data.DbItem.Save.UpdateExisting(itemWithLowestID);

          // Create a log entry for the item's movement to production
          ETS.Core.Api.Models.Data.DbItemLog itemLog = new ETS.Core.Api.Models.Data.DbItemLog();

          // Populate the log entry details
          itemLog.User = this.Ets.User.Login;
          itemLog.ItemLogDefinitionID = 2;
          itemLog.LogDateTime = this.Ets.SiteNow;
          itemLog.Lot = itemWithLowestID.Lot;
          itemLog.ItemID = itemWithLowestID.ID;
          itemLog.MaterialID = 1;
          itemLog.Quantity = 1;
          itemLog.LocationID = 1;
          itemLog.JobID = JobID;
          itemLog.Notes = "Moved to production";

          ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbItemLog> result;

          // Insert the log entry into the database
          result = api.Data.DbItemLog.Save.InsertAsNew(itemLog);

        }
      }

      // update all the needed tags so that a job wil start
      this.Ets.Api.Tags.UpdateVirtualTagByID(28, 1, uow).ThrowIfFailed();
      this.Ets.Api.Tags.UpdateVirtualTagByID(29, product.Name, uow).ThrowIfFailed();
      this.Ets.Api.Tags.UpdateVirtualTagByID(26, _job.Name + "-1", uow).ThrowIfFailed();
      this.Ets.Api.Tags.UpdateVirtualTagByID(27, _job.Name, uow).ThrowIfFailed();
      this.Ets.Api.Tags.UpdateVirtualTagByID(139, 1, uow).ThrowIfFailed();
      Trace.Write("Tags have been changed.");

      uow.ExecuteReturnsResultObject().ThrowIfFailed();

      var url = "http://10.19.10.4/TS/pages/home/batching/plannedprodjobs/jobstart";
      this.Ets.Pages.RedirectToUrl(url);
    }

  }

  // ***********************************************************

  // ***********************************************************

}