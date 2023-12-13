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


// Testing changes to TrackItem : ContentPageBase
// Testing changes to TrackItem : ContentPageBase
// Testing changes to TrackItem : ContentPageBase
// Testing changes to TrackItem : ContentPageBase
// Testing changes to TrackItem : ContentPageBase
// Testing changes to TrackItem : ContentPageBase
// Testing changes to TrackItem : ContentPageBase

namespace ETS.Ts.Content
{
  /// ***********************************************************
  public partial class TrackItem : ContentPageBase
  {
    /// Automatically map the decorated property to incoming Ets.Values data (typically QueryString variables).
    /// The name of the property is used as the Key in accessing the Ets.Values collection during mapping.
    [ValuesProperty()]
    public int ItemID { get; set; } = -1;

    public DbItem _item;

    /// ***********************************************************
    protected override bool ContentPage_Init()
    {
      if(this.ItemID.IsEdit()) this.Ets.Session["ItemID"] = this.ItemID;
      // find the item that will be impacted
      _item = this.Ets.Api.Data.DbItem.Load.ByID(this.ItemID);
      
      // if no id was provided or it was invalid, ask for an ID
      if(_item == null)
      {
        var sql = @"SELECT TOP 1 i.ID FROM tItem i JOIN tItemDefinition id ON i.ItemDefinitionID = id.ID 
                    JOIN tItemDefinitionGroup idg ON id.ItemDefinitionGroupID = idg.ID WHERE idg.SiteID = {0} 
                    AND i.LocationID IS NOT NULL AND i.LocationID <> 125 AND i.ID = 136 ORDER BY ID DESC".FormatWith(this.Ets.Api.Site.GetCurrentSiteID());
        this.ItemID = this.Ets.Api.Util.Db.ExecuteScalar<int>(sql).Return;
        _item = this.Ets.Api.Data.DbItem.Load.ByID(this.ItemID).ThrowIfLoadFailed("ID", this.ItemID);
        this.Ets.Values["ScanID"] = _item.UniqueID;
        this.Ets.Values["ItemFound"] = false;
        this.Ets.Values["ItemNotFound"] = true;
        return true;
      }
      
      this.Ets.Values["ItemFound"] = true;
      this.Ets.Values["ItemNotFound"] = false;
      this.Ets.Values.CopyFromModel(_item, "Item.");
      if(_item.LocationID != -1) this.Ets.Values["Location.Name"] = this.Ets.Api.Data.DbLocation.Load.ByID(_item.LocationID).Name;
      if(_item.JobID != -1) this.Ets.Values["Job.Name"] = this.Ets.Api.Data.DbJob.Load.ByID(_item.JobID).Name;
      if(_item.BatchID != -1) this.Ets.Values["Batch.Name"] = this.Ets.Api.Data.DbBatch.Load.ByID(_item.BatchID).Name;
      if(_item.ProductID != -1) 
      {
        this.Ets.Values["Product.Name"] = this.Ets.Api.Data.DbProduct.Load.ByID(_item.ProductID).Name;
        this.Ets.Values["Product.LookupEnabled"] = true;
      }
      if(_item.MaterialID != -1) 
      {
        this.Ets.Values["Material.Name"] = this.Ets.Api.Data.DbMaterial.Load.ByID(_item.MaterialID).Name;
        this.Ets.Values["Material.Units"] = this.Ets.Api.Data.DbMaterial.Load.ByID(_item.MaterialID).Units;
      }
      
      this.Ets.Values["ScanID"] = _item.UniqueID;
      
      return true;    } 
    //   // get userstate value and use with "Item_UserState" lookupset to retrieve value
    //   var itemStates = this.Ets.Api.Data.DbLookupSet.Load.ByKey("Item_UserState");
    //   if(itemStates != null) // if lookupset was found, enable change state tile
    //   {
    //     this.Ets.Values["TileChangeState.Visible"] = true;
    //     var stateList = this.Ets.Api.Data.DbLookupValue.GetList.ForLookupSetID(itemStates.ID);
    //     DbLookupValue selectedState = null;
    //     foreach(var state in stateList)
    //     {
    //       if(state.Value.AsInt(-1) == _item.UserState)
    //       {
    //         selectedState = state;
    //         this.Ets.Values.CopyFromModel(state, "ItemState.");
    //         break;
    //       }
    //     }
    //     if(selectedState == null) this.Ets.Values["ItemState.Name"] = "NA";
    //   }
    //   return true;
     

    /// ***********************************************************
    private void LoadItem_Click(object sender, EventArgs e)
    {
      var uID = this.Ets.Form.GetValueAsStringByKey("Item.UniqueID");
      var item = this.Ets.Api.Data.DbItem.Load.ByUniqueID(uID);
      if(item == null)
      {
        this.Ets.Form.AddValidationError("Item not found for Unique ID '{0}'".FormatWith(uID));
        return;
      }
      this.Ets.Session["ItemID"] = item.ID;
      var url = this.Ets.Pages.BuildSelfUrlIfNecessary("self:ItemID={0}".FormatWith(item.ID));
      this.Ets.Pages.RedirectToUrl(url);
    }
  }
}