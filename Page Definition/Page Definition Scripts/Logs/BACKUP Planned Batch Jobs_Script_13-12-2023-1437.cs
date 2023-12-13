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
  public partial class SelectBatchJob : ContentPageBase
  {
    /// Automatically map the decorated property to incoming Ets.Values data (typically QueryString variables).
    /// The name of the property is used as the Key in accessing the Ets.Values collection during mapping.
    [ValuesProperty()]
    public int SystemID { get; set; } = 1;

    [ValuesProperty()]
    public int JobID { get; set; } = -1;

    [ValuesProperty()]
    public int cmbStatus { get; set; } = -1;
    

    // ***********************************************************
    protected override bool ContentPage_Init()
    {

      return true;
    }

    // ***********************************************************
    protected override bool ContentPage_PartPreInit10()
    {
      this.Ets.Values["JobID"] = this.Ets.Values.GetAsInt("data.BatchJobs.Selected.JobID");
      var api = ETS.Core.Api.ApiService.GetInstance();
     List<ETS.Core.Api.Models.Data.DbItem> items = new List<ETS.Core.Api.Models.Data.DbItem>();

      // Load the list of items from the database
      items = api.Data.ListOf.DbItems.GetList.WithSql("SELECT * FROM tItem");

      // Define the MaterialIDs and lists of LocationIDs for each material
      int[] materialIDsToCheck = { 1, 2, 3 };
      List<int>[] locationIDsToCheck = new List<int>[]
      {
        new List<int> { 7, 8, 9, 10, 11, 12, 13, 14, 15 },
        new List<int> { 7, 8, 9, 10, 11, 12, 13, 14, 15 },
        new List<int> { 7, 8, 9, 10, 11, 12, 13, 14, 15 }
      };

      // Find the items that match the MaterialID and LocationIDs for each material in the items list
      var matchingItemsMaterial1 = items.Where(i => i.MaterialID == materialIDsToCheck[0] && locationIDsToCheck[0].Contains(i.LocationID));
      var matchingItemsMaterial2 = items.Where(i => i.MaterialID == materialIDsToCheck[1] && locationIDsToCheck[1].Contains(i.LocationID));
      var matchingItemsMaterial3 = items.Where(i => i.MaterialID == materialIDsToCheck[2] && locationIDsToCheck[2].Contains(i.LocationID));

      // Check if there are any matching items for each material
      if (matchingItemsMaterial1.Any())
      {
        // Items with MaterialID = 1 and LocationID between 7 and 15 exist in the list.
        // Do something for MaterialID 1 if needed.
        api.Tags.UpdateVirtualTagByID(595, 0);
      }
      else
      {
        api.Tags.UpdateVirtualTagByID(595, 1);
      }

      if (matchingItemsMaterial2.Any())
      {
        // Do something for MaterialID 2 if needed.
        api.Tags.UpdateVirtualTagByID(593, 0);
      }
      else
      {
        // Trigger your desired function for MaterialID 2 here.
        //YourFunctionForMaterial2();
        api.Tags.UpdateVirtualTagByID(593, 1);
      }

      if (matchingItemsMaterial3.Any())
      {
        // Do something for MaterialID 3 if needed.
        api.Tags.UpdateVirtualTagByID(594, 0);
      }
      else
      {
        // Trigger your desired function for MaterialID 3 here.
        //YourFunctionForMaterial3();
        api.Tags.UpdateVirtualTagByID(594, 1);
      }

    
      
      return true;
    }


  }
}
