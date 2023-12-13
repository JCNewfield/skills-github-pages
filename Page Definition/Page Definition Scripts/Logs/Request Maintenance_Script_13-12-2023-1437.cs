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
  public partial class requestMaintenance : ContentPageBase
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
      return true;
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
    protected override bool ContentPage_Final()
    {
      return true;
    }
    
    // creates a random string
    static string GenerateRandomString(int length)
    {
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
      Random random = new Random();
      string randomString = new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());

      return randomString;
    }

    // enum list for datatypes
    public enum DataType
    {
      Integer = 1,
      String = 2,
      // Other enum values...
    }

    private void Add(object sender, EventArgs e)
    {
      //get an instance of the api
      ApiService api = this.Ets.Api;

      DateTime defaultDateTime = DateTime.Now.AddDays(2);
     
      // Populate values from the form
      string JobName = this.Ets.Form.GetValueAsStringByKey("Model.Name");
      string Description = this.Ets.Form.GetValueAsStringByKey("Model.Notes");
      int PriorityNumber = this.Ets.Form.GetValueAsIntByKey("Model.Priority");
      int SystemID = this.Ets.Form.GetValueAsIntByKey("Model.Area");
      int UserID = this.Ets.Form.GetValueAsIntByKey("Model.User");
      string CompletedByDateTime = this.Ets.Form.GetValueAsDateTimeByKey("Model.Deadline", defaultDateTime).ToUniversalDateTimeString();
      int UserState = 0;
      string colorCode;
      string Priority;
      DateTime Submitted = DateTime.Now;
      string formattedDate = Submitted.ToString("yyyy-MM-dd");


      int _TaskFormItemGroupID = -1;

      // sets a colorcode and priority text based on the choosen priority in the form
      switch (PriorityNumber)
      {
        case 1:
          colorCode = "#808080";
          Priority = "No Priority";
          break;
        case 2:
          colorCode = "#808080";
          Priority = "Low Priority";
          break;
        case 3:
          colorCode = "#808080";
          Priority = "Medium Priority";
          break;
        case 4:
          colorCode = "#ffff00";
          Priority = "High Priority";
          break;
        case 5:
          colorCode = "#FFA500";
          Priority = "Urgent";
          break;
        case 6:
          colorCode = "#FF0000";
          Priority = "Emergency";
          break;
        default:
          colorCode = "#808080";
          Priority = "UNKNOWN";
          break;
      }

      // links the systems with a taskformitemgroup
      switch (SystemID)
      {
        //oven
        case 2:
          _TaskFormItemGroupID = 12;
          break;
        //rotator
        case 3:
          _TaskFormItemGroupID = 13;
          break;
        //slicer
        case 4:
          _TaskFormItemGroupID = 14;
          break;
        //Packaging line
        case 6:
          _TaskFormItemGroupID = 18;
          break;
        // delivery and pickup station
        case 11:
          _TaskFormItemGroupID = 15;
          break;
        // hbw
        case 12:
          _TaskFormItemGroupID = 16;
          break;
        //fpw
        case 13:
          _TaskFormItemGroupID = 17;
          break;
      }


      // Validate the data
      if (string.IsNullOrWhiteSpace(JobName))
      {
        this.Ets.Form.AddValidationError("Invalid Job Name", "Model.Name");
        return;
      }


      string randomString = GenerateRandomString(4);

      ETS.Core.Api.Models.Data.DbTaskFormItem taskItem = new ETS.Core.Api.Models.Data.DbTaskFormItem();

      // popualte a taskformitem object
      taskItem.Name = this.Ets.Form.UpdateModelValueByFormKey("Model.name", taskItem.Name, "Name");
      taskItem.TaskFormItemGroupID = _TaskFormItemGroupID;
      taskItem.Key = "temp" + randomString;
      taskItem.DataType = ETS.Core.Enums.DataType.Integer;
      taskItem.DefaultValue = "0";
      taskItem.ValidateMinimum = 0;
      taskItem.ValidateMaximum = 0;
      taskItem.ValidateChange = false;
      taskItem.Enabled = true;
      taskItem.WidthBs = 12;
      taskItem.IsReadOnly = false;
      taskItem.Description = this.Ets.Form.UpdateModelValueByFormKey("Model.nOTES", taskItem.Description, "Notes");

      ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbTaskFormItem> result;

      // Here we insert the entity in the database (The ID will be created when inserted to the database)
      result = api.Data.DbTaskFormItem.Save.InsertAsNew(taskItem);

      // Custom table _customer does not have a related API, and a SQL query is used instead
      string addTaskSql = @"
            INSERT INTO t_maintTasks (JobName, Description, PriorityNumber, UserState, SystemID, CompletedByDateTime, ColorCode, Priority, UserID, Submitted, TaskFormItemGroupID)
            VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}' , '{9}', {10} );
        ".FormatWith(JobName, Description, PriorityNumber, UserState, SystemID, CompletedByDateTime, colorCode, Priority, UserID, formattedDate, _TaskFormItemGroupID);

      // Save the customer group to the database

      if (api.Util.Db.ExecuteSql(addTaskSql).ThrowIfFailed())
      {
        // If the save is successful, redirect to the success URL
        var url = this.Ets.Pages.PageUrl;
        this.Ets.Pages.RedirectToUrl(url);
      }


    }


  }
}
