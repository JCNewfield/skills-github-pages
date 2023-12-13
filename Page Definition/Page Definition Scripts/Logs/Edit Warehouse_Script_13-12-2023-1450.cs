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
using ETS.Core.Scripting;
using ETS.Core.Services.Resource;
using ETS.Ts.Core.ContentParts;
using ETS.Ts.Core.Enums;
using ETS.Ts.Core.Scripting;

namespace ETS.Ts.Content
{
  /// ***********************************************************
  public partial class EditWarehouse : ContentPageBase
  {

    private DbItem _thisItem;
    /// Automatically map the decorated property to incoming Ets.Values data (typically QueryString variables).
    /// The name of the property is used as the Key in accessing the Ets.Values collection during mapping.
    //[ValuesProperty()]
    //public int SystemID { get; set; } = -1;

    /// ***********************************************************
    /// <remarks>
    /// All Page level ContentProperties have been set from default
    /// values or Ets.Values. Content Parts are not yet loaded/initialized.
    ///
    /// Do things like:
    ///   Check Page Permissions
    ///   Set Resource Strings
    /// </remarks>
    /// ***********************************************************
    protected override bool ContentPage_Init()
    {
      var _ItemID = this.Ets.Values["ItemID"];
      string itemString2 = _ItemID.ToString();
      int itemInteger2 = Int32.Parse(itemString2);
      this._thisItem = Ets.Api.Data.DbItem.Load.ByID(itemInteger2);
      this.Ets.Values["Model.Packaging"] = this._thisItem.ItemDefinitionID;
      this.Ets.Values["Model.Material"] = this._thisItem.MaterialID;
      this.Ets.Values["Model.Location"] = this._thisItem.LocationID;
      this.Ets.Values["Model.RFID"] = this._thisItem.Attribute01;
      this.Ets.Values["Model.ExpDate"] = this._thisItem.Attribute02;
      return true;
    }

    /// ***********************************************************
    /// <remarks>
    /// Content Parts with InitOrder 1 have been loaded/initialized.
    /// Called just before Content Parts with InitOrder = 2
    /// are loaded/initialized (typically Filter parts).
    ///
    /// Do things like:
    ///   Read from Ets.Values
    ///   Update Ets.Values (with data for Parts about to be loaded/initialized)
    ///
    /// Do not:
    ///   Directly manipulate Content Part Properties
    /// </remarks>
    /// ***********************************************************
    protected override bool ContentPage_PartPreInit02()
    {
      return true;
    }

    /// ***********************************************************
    /// <remarks>
    /// Content Parts with InitOrder 1-4 have been loaded/initialized.
    /// Called just before Content Parts with InitOrder = 5
    /// are loaded/initialized (typically Data Table parts).
    ///
    /// Do things like:
    ///   Read from Ets.Values
    ///   Update Ets.Values (with data for Parts about to be loaded/initialized)
    ///
    /// Do not:
    ///   Directly manipulate Content Part Properties
    /// </remarks>
    /// ***********************************************************
    protected override bool ContentPage_PartPreInit05()
    {
      return true;
    }

    /// ***********************************************************
    /// <remarks>
    /// Content Parts with InitOrder 1-9 have been loaded/initialized.
    /// Called just before Content Parts with InitOrder = 10
    /// are loaded/initialized (typically all other Content Parts).
    ///
    /// Do things like:
    ///   Read from Ets.Values
    ///   Update Ets.Values (with data for Parts about to be loaded/initialized)
    ///
    /// Do not:
    ///   Directly manipulate Content Part Properties
    /// </remarks>
    /// ***********************************************************
    protected override bool ContentPage_PartPreInit10()
    {
      return true;
    }

    // private void Edit(object sender, RowItemEventArgs e)
    // {
    //   // Getting the ID of the clicked on item      
    //   int itemID = e.GetInteger("ID");

    //   // Load the item which is clicked on such that it can be used further
    //   var item = Ets.Api.Data.DbItem.Load.ByID(itemID).ThrowIfLoadFailed("Item.ID", itemID);




    // }

    private void Edit(object sender, EventArgs e)
    {

      // Get instance of api service
      var api = ETS.Core.Api.ApiService.GetInstance();

      // We need to get the idea in the session from the spokes pages, -- Setting Ets.Values['Data.Products.Selected.ID'] to "ID Value"	

      var _item = this.Ets.Values["Data.Products.Selected.ID"];

      string itemString = _item.ToString();
      int itemInteger = Int32.Parse(itemString);
      // Create a model object to populate
      ETS.Core.Api.Models.Data.DbItem item = api.Data.DbItem.Load.ByID(itemInteger);

      // Populate the model object as needed
      item.ItemDefinitionID = this.Ets.Form.UpdateModelValueByFormKey("Model.Packaging", item.ItemDefinitionID, "ItemDefiniton");
      item.MaterialID = this.Ets.Form.UpdateModelValueByFormKey("Model.Material", item.MaterialID, "Material");
      item.LocationID = this.Ets.Form.UpdateModelValueByFormKey("Model.Location", item.LocationID, "Location");
      item.UniqueID = this.Ets.Form.UpdateModelValueByFormKey("Model.Unique", item.UniqueID, "UniqueID");
      item.Attribute01 = this.Ets.Form.UpdateModelValueByFormKey("Model.RFID", item.Attribute01, "RFID");
      item.Attribute02 = this.Ets.Form.UpdateModelValueByFormKey("Model.ExpDate", item.Attribute02, "ExpDate");
      item.Quantity = 1;
      item.ValidFromDateTime = DateTime.Now;
      item.ValidToDateTime = DateTime.Now.AddDays(10);

      // create a result object to determine the success of the operation
      ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbItem> result;

      // Here we insert the entity in the database (The ID will be created when inserted to the database)
      result = api.Data.DbItem.Save.UpdateExisting(item);

      if (result.Success)
      {
        int newID = result.Return.ID;
      }
      else
      {
        return;
      }

      var url = "http://10.19.10.4/TS/pages/home/adminconfig/warehouse/";
      this.Ets.Pages.RedirectToUrl(url);
    }

    protected override bool ContentPage_Final()
    {
      return true;
    }

  }

  /// ***********************************************************
  /// <remarks>
  /// All Content Parts have been loaded/initialized.
  ///
  /// At this point, adding or changing Ets.Values data will no
  /// longer serve a purpose as all Content Parts have accessed
  /// what they require.
  /// </remarks>
  /// ***********************************************************

}
