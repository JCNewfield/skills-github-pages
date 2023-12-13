using System;
using System.Collections.Generic;
using System.Data;
using ETS.Core;
using ETS.Core.Api;
using ETS.Core.Api.Models.Data;
using ETS.Core.Enums;
using ETS.Core.Extensions;
using System.Linq;
using ETS.Core.Scripting;



namespace ETS.Core.Scripting
{
  /// ******************************************************************
  /// <summary>Class Description</summary>
  /// ******************************************************************
  public class MaterialManager
  {
    /// ******************************************************************
    /// <summary>// Declare a list object to store items
    public int MaterialID { get; set; }
    public int LocationID { get; set; }
    public DbItem item;
    private readonly DbSystem _system;
    /// Method Description
  
    /// </summary>
    /// ******************************************************************
    public void CheckMaterialStock()
    {
      var api = ETS.Core.Api.ApiService.GetInstance();
      List<Api.Models.Data.DbItem> items = new List<Api.Models.Data.DbItem>();

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

    }
  }
}