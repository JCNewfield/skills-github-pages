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
  public partial class EditCustomer : ContentPageBase
  {
   
    /// Automatically map the decorated property to incoming Ets.Values data (typically QueryString variables).
    /// The name of the property is used as the Key in accessing the Ets.Values collection during mapping.
    //[ValuesProperty()]
    //public int SystemID { get; set; } = -1;
 public int CustomerGroupID { get; set; } = -1;
    /// ***********************************************************
    /// <remarks>
    /// All Page level ContentProperties have been set from default
    /// values or Ets.Values. Content Parts are not yet loaded/initialized.
    ///
    private string CustomerName;
    private string CustomerAddress;
    private string CustomerEmail;
    private string CustomerNumber;
    private string CustomerGroupName;
    private string CustomerGroupAltName;
    private bool CustomerGroupActive;

    /// Do things like:
    ///   Check Page Permissions
    ///   Set Resource Strings
    /// </remarks>
    /// ***********************************************************
    protected override bool ContentPage_Init()
    {
       var customerValue = this.Ets.Values["CustomerID"];

    string customerString = customerValue.ToString();
    CustomerGroupID = Int32.Parse(customerString);
      ETS.Core.Scripting.CustomerAddress CustomerAddress = new ETS.Core.Scripting.CustomerAddress();
     
        DataTable currentData = CustomerAddress.GetCustomerGroup(this.Ets.Api, CustomerGroupID);

        //this.Ets.Api.Util.Db.ExecuteSql();
        this.CustomerGroupName = currentData.Rows[0].Field<string>("name");
        this.CustomerGroupAltName = currentData.Rows[0].Field<string>("address");
        this.CustomerEmail = currentData.Rows[0].Field<string>("email");
        this.CustomerNumber = currentData.Rows[0].Field<string>("phonenumber");
        this.Ets.Values["Model.Name"] = this.CustomerGroupName;
        this.Ets.Values["Model.Address"] = this.CustomerGroupAltName;
         this.Ets.Values["Model.Email"] = this.CustomerEmail;
         this.Ets.Values["Model.Number"] = this.CustomerNumber;
        
       
      
      
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
    this.CustomerGroupName = this.Ets.Form.GetValueAsStringByKey("Model.Name");
    this.CustomerGroupAltName = this.Ets.Form.GetValueAsStringByKey("Model.Address");
     this.CustomerEmail = this.Ets.Form.GetValueAsStringByKey("Model.Email");
     this.CustomerNumber= this.Ets.Form.GetValueAsStringByKey("Model.Number");
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

    // var _customer = this.Ets.Values["CustomerID"];

    // string customerString2 = _customer.ToString();
    // int customerID = Int32.Parse(customerString2);
    // Create a model object to populate
    //thisCustomer = this.Ets.Api.Data.DbSystem.Load.WithSql(@"SELECT * FROM _customer WHERE ID = {0}".FormatWith(customerID));
    string customerName = this.Ets.Form.GetValueAsStringByKey("Model.Name");
    string customerAddress = this.Ets.Form.GetValueAsStringByKey("Model.Address");
    string customerEmail = this.Ets.Form.GetValueAsStringByKey("Model.Email");
    string customerPhone = this.Ets.Form.GetValueAsStringByKey("Model.Number");

    string updateCustomerSql = @"
      UPDATE _customer
      SET name = '{1}'
      , address = '{2}'
      , email = '{3}'
      , phonenumber = '{4}'
      WHERE ID = {0};
       ".FormatWith(CustomerGroupID, customerName, customerAddress, customerEmail, customerPhone);
    api.Util.Db.ExecuteSql(updateCustomerSql).ThrowIfFailed();
    var url = "http://10.19.10.4/TS/pages/home/adminconfig/managecustomer/";
    //FullUrlPath("http://10.19.10.4/TS/pages/home/adminconfig/managecustomer/");
    this.Ets.Pages.RedirectToUrl(url);

  }






  protected override bool ContentPage_Final()
  {
  this.CustomerGroupName = this.Ets.Form.GetValueAsStringByKey("Model.Name");
      this.CustomerGroupAltName = this.Ets.Form.GetValueAsStringByKey("CustomerGroup.Address");
          this.CustomerEmail = this.Ets.Form.GetValueAsStringByKey("Model.Email");
     this.CustomerNumber= this.Ets.Form.GetValueAsStringByKey("Model.Number");
    return true;
  }
 private bool PopulateValuesFromForm()
    {
      this.CustomerGroupName = this.Ets.Form.GetValueAsStringByKey("Model.Name");
      this.CustomerGroupAltName = this.Ets.Form.GetValueAsStringByKey("CustomerGroup.Address");
          this.CustomerEmail = this.Ets.Form.GetValueAsStringByKey("Model.Email");
     this.CustomerNumber= this.Ets.Form.GetValueAsStringByKey("Model.Number");
      return true;
    }

}
}


