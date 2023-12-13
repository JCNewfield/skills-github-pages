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
  public partial class ManageProductStorage : ContentPageBase
  {
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

      var _test = this.Ets.Values["Data.Location.Selected.ID"];

      string _testString = _test.ToString();
      Trace.Write("The Location ID value is as following: " + _testString);

      return true;
    }

    private void Delete(object sender, RowItemEventArgs e)
    {

      // create a reference to the api service
      var api = ETS.Core.Api.ApiService.GetInstance();

      // Getting the ID of the clicked on item
      int itemID = e.GetInteger("ID");

      // create a result object to determine the success of the operation
      ETS.Core.Api.Models.Result<bool> result;

      // remove the entity with ID itemID from the database
      result = api.Data.DbProduct.Delete.ByID(itemID);

      // examine the results of the operation
      if (result.Success)
      {
        // success code
      }
      else
      {
        // failure code
      }

      var url = this.Ets.Pages.PageUrl;
      this.Ets.Pages.RedirectToUrl(url);

    }

    private void Add(object sender, EventArgs e)
    {

      // Get instance of api service
      var api = ETS.Core.Api.ApiService.GetInstance();

      // Create a model object to populate
      ETS.Core.Api.Models.Data.DbProduct product = new ETS.Core.Api.Models.Data.DbProduct();

      // Receives the last id from the table that is chosen + 1 or any other values that is preferred.
      var maxIDFromTproduct = this.Ets.Api.Util.Db.ExecuteScalar<int>(@"SELECT MAX(ID) FROM tProduct;").Return + 1;

      // Populate the model object as needed
      product.ID = maxIDFromTproduct;
      product.Name = this.Ets.Form.UpdateModelValueByFormKey("Model.Name", product.Name, "Name of the Product");
      product.ProductCode = this.Ets.Form.UpdateModelValueByFormKey("Model.ProductCode", product.ProductCode, "Code of the product");
      product.StandardSize = this.Ets.Form.UpdateModelValueByFormKey("Model.StandardSize", product.StandardSize, "Standard size of the product");
      product.ProductTypeID = this.Ets.Form.UpdateModelValueByFormKey("Model.ProductTypeID", product.ProductTypeID, "Product Type ID");
      product.ProductSetID = this.Ets.Form.UpdateModelValueByFormKey("Model.ProductSetID", product.ProductSetID, "Product Set ID");
      product.StandardUnits = this.Ets.Form.UpdateModelValueByFormKey("Model.StandardUnits", product.StandardUnits, "Standard units of the product");
     // product.ProducedMaterialID = this.Ets.Form.UpdateModelValueByFormKey("Model.ProductMaterial", product.ProducedMaterialID, "Material of the product");
      product.TheoreticalRateCalculationUnitsPerMinute = 1;
      product.TargetRateCalculationUnitsPerMinute = 2;

      // create a result object to determine the success of the operation
      ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbProduct> result;

      // Here we update the entity in the database.
      result = api.Data.DbProduct.Save.UpdateExisting(product);

      if (result.Success)
      {
        int newID = result.Return.ID;
      }
      else
      {
        return;
      }

      var url = this.Ets.Pages.PageUrl;
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