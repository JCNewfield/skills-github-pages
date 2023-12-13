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
  public partial class ManageCustomer : ContentPageBase
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
    // private string CustomerName;
    // private string CustomerAddress;
    /// </remarks>
    /// ***********************************************************
    protected override bool ContentPage_Init()
    {
      this.Page.Trace.IsEnabled = true;
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
 
    /// ***********************************************************
    protected override bool ContentPage_Final()
    {
      return true;
    }

    private void Delete(object sender, RowItemEventArgs e)
    {

      // create a reference to the api service
      var api = ETS.Core.Api.ApiService.GetInstance();

      // Getting the ID of the clicked on item
      int customerID = e.GetInteger("ID");

      // create a result object to determine the success of the operation
      ETS.Core.Api.Models.Result<bool> result;

      // remove the entity with ID itemID from the database

      Trace.Write("The CustomerID is: " + customerID);

      // The table is a custom made in the TrakSYS db, therefore a SQL query is run to delete the chosen customer
      var customer = api.Util.Db.ExecuteScalar<int>("DELETE FROM _customer WHERE [ID] = " + customerID + ";");

      // examine the results of the operation
      // if (result.Success)
      // {
      //   // success code
      // }
      // else
      // {
      //   // failure code
      // }

      var url = this.Ets.Pages.PageUrl;
      this.Ets.Pages.RedirectToUrl(url);

    }

    private void Save_Click(object sender, EventArgs e)
    {
      try
      {
        var maxIDFromCostumer = this.Ets.Api.Util.Db.ExecuteScalar<int>(@"SELECT MAX(ID) FROM _customer;").Return + 1;
        // Populate values from the form
        string customerName = this.Ets.Form.GetValueAsStringByKey("Model.Name");
        string customerAddress = this.Ets.Form.GetValueAsStringByKey("Model.Address");
        string customerEmail = this.Ets.Form.GetValueAsStringByKey("Model.Email");
        string customerPhone = this.Ets.Form.GetValueAsStringByKey("Model.Number");
        customerPhone = "+47 " + customerPhone;

        // Validate the data
        if (string.IsNullOrWhiteSpace(customerName))
        {
          this.Ets.Form.AddValidationError("Invalid Customer Name", "Model.Name");
          return;
        }
        // Validate the data
        if (string.IsNullOrWhiteSpace(customerAddress))
        {
          this.Ets.Form.AddValidationError("Invalid Customer address", "Model.Address");
          return;
        }

        // Custom table _customer does not have a related API, and a SQL query is used instead
        string addCustomerSql = @"
            INSERT INTO _customer (ID, name, address, email, phonenumber)
            VALUES ('{0}', '{1}', '{2}', '{3}', '{4}');
        ".FormatWith(maxIDFromCostumer, customerName, customerAddress, customerEmail, customerPhone);

        // Save the customer group to the database
        ApiService api = this.Ets.Api;
        if (api.Util.Db.ExecuteSql(addCustomerSql).ThrowIfFailed())
        {
          // If the save is successful, redirect to the success URL
          var url = this.Ets.Pages.PageUrl;
          this.Ets.Pages.RedirectToUrl(url);

        }
      }
      catch (Exception ex)
      {
        this.Ets.Debug.FailFromException(ex);
      }
    }

  }
}
