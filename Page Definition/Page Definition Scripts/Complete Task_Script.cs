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
using ETS.Core.Api.Models.Tasks;

namespace ETS.Ts.Content
{
  /// ***********************************************************
  public partial class CompleteTask : ContentPageBase
  {
    public int taskFormItemID { get; set; }
    /// Automatically map the decorated property to incoming Ets.Values data (typically QueryString variables).
    /// The name of the property is used as the Key in accessing the Ets.Values collection during mapping.
    //[ValuesProperty()]
    //public int SystemID { get; set; } = -1;

    public int taskFormItemGroupID { get; set; }

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
      string task_String = this.Ets.Values["TaskValue"].ToString();
      if(task_String == "1"){
        this.Ets.Values["Data.TaskFormItems.DoNotShow"] = false;
        this.Ets.Values["Data.TaskFormItems.DoShow"] = true;
      }
      else{
        this.Ets.Values["Data.TaskFormItems.DoNotShow"] = true;
        this.Ets.Values["Data.TaskFormItems.DoShow"] = false;
      }

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

    private int Convert(int value)
    {
      switch (value)
      {
        case 3:
          value = 1;
          return value;
        case 8:
          value = 2;
          return value;
        case 4:
          value = 3;
          return value;
        case 5:
          value = 4;
          return value;
        case 6:
          value = 5;
          return value;
        case 7:
          value = 6;
          return value;
        default:
          return value;
      }
    }

    protected override bool ContentPage_PartPreInit10()
    {

      //Get instance of api service
      var api = ETS.Core.Api.ApiService.GetInstance();

      string taskFormItemGroupID_String = this.Ets.Values["Data.TaskFormItems.Selected.TaskFormItemGroupID"].ToString();
      string taskFormItemID_String = this.Ets.Values["Data.TaskFormItems.Selected.ID"].ToString();

      taskFormItemGroupID = Int32.Parse(taskFormItemGroupID_String);
      taskFormItemID = Int32.Parse(taskFormItemID_String);

      int taskDefinitionID = Convert(taskFormItemGroupID);

      string vgr = "~/library/root/MaintenanceItems/PmVGRThumb.png";
      string vgr_Url = "~/library/root/MaintenanceItems/PmVGR.pdf";
      string cutter = "~/library/root/MaintenanceItems/PmCutterThumb.png";
      string cutter_Url = "~/library/root/MaintenanceItems/PmCutter.pdf";
      string oven = "~/library/root/MaintenanceItems/PmOvenThumb.png";

      switch (taskDefinitionID)
      {
        case 1:
          this.Ets.Values["Sop.Thumb"] = vgr;
          this.Ets.Values["Sop.Tip"] = "VGR";
          this.Ets.Values["Sop.Url"] = vgr_Url;
          break;
        case 3:
          this.Ets.Values["Sop.Thumb"] = cutter;
          this.Ets.Values["Sop.Tip"] = "Cutter";
          this.Ets.Values["Sop.Url"] = cutter_Url;
          break;
        case 4:
          this.Ets.Values["Sop.Thumb"] = oven;
          this.Ets.Values["Sop.Tip"] = "Oven";
          break;
        default:
          this.Ets.Values["Sop.Thumb"] = "";
          this.Ets.Values["Sop.Tip"] = "No Instructions";
          break;
      }

      return true;
    }
    public void UpdateTaskUserState(List<ETS.Core.Api.Models.Data.DbTask> tasks, int taskDefinitionID)
    {
      var api = ETS.Core.Api.ApiService.GetInstance();
      foreach (var task in tasks)
      {
        if (task.TaskDefinitionID == taskDefinitionID)
        {
          task.UserState = 1;
          ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbTask> result;

          result = api.Data.DbTask.Save.UpdateExisting(task);

        }
      }
    }

    private void Add(object sender, EventArgs e)
    {

      var uow = this.Ets.Api.CreateUnitOfWork();
      //Get instance of api service
      var api = ETS.Core.Api.ApiService.GetInstance();

      // Create a model object to populate
      ETS.Core.Api.Models.Data.DbJournal journal = new ETS.Core.Api.Models.Data.DbJournal();

      // Receives the last id from the table that is chosen + 1 or any other values that is preferred.
      var maxIDFromTjournal = this.Ets.Api.Util.Db.ExecuteScalar<int>(@"SELECT MAX(ID) FROM tJournal;").Return + 1;

      // Populate the model object as needed
      journal.ID = maxIDFromTjournal;
      journal.JournalCategoryID = this.Ets.Form.UpdateModelValueByFormKey("Model.JournalCategory", journal.JournalCategoryID, "Journal Category");
      journal.Case = this.Ets.Form.UpdateModelValueByFormKey("Model.Case", journal.Case, "Journal Case");
      // journal.Measure = this.Ets.Form.UpdateModelValueByFormKey("Model.Measure", journal.Measure, "Journal Measure");
      journal.Notes = this.Ets.Form.UpdateModelValueByFormKey("Model.Notes", journal.Notes, "Journal Notes");
      journal.User = this.Ets.User.Login;
      journal.SystemID = this.Ets.Form.UpdateModelValueByFormKey("TaskValue", journal.SystemID, "Journal System");
      journal.AreaID = 4;
      journal.JournalDateTime = DateTime.Now;
      journal.Capture01 = "PM";
      // create a result object to determine the success of the operation
      ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbJournal> result;

      // Here we insert the entity in the database (The ID will be created when inserted to the database)
      result = api.Data.DbJournal.Save.InsertAsNew(journal);

      // new test script
      List<ETS.Core.Api.Models.Data.DbTask> tasks = new List<ETS.Core.Api.Models.Data.DbTask>();
      tasks = api.Data.ListOf.DbTasks.GetList.WithSql("SELECT * FROM tTask");
      var url = this.Ets.Pages.PageUrl;

      switch (taskFormItemID)
      {
        case 1:
          this.Ets.Api.Tags.UpdateVirtualTagByID(588, 0, uow);
          uow.ExecuteReturnsResultObject().ThrowIfFailed();
          UpdateTaskUserState(tasks, 1);
          var uVGR = api.Util.Db.ExecuteScalar<int>($"UPDATE t_Fixing SET Grade = 100 WHERE ID = 1");
          this.Ets.Pages.RedirectToUrl(url);
          break;
        case 4:
          this.Ets.Api.Tags.UpdateVirtualTagByID(588, 0, uow);
          uow.ExecuteReturnsResultObject().ThrowIfFailed();
          UpdateTaskUserState(tasks, 3);
          var uCutter = api.Util.Db.ExecuteScalar<int>($"UPDATE t_Fixing SET Grade = 100 WHERE ID = 3");
          this.Ets.Pages.RedirectToUrl(url);
          break;
        case 6:
          UpdateTaskUserState(tasks, 4);
          var uOven = api.Util.Db.ExecuteScalar<int>($"UPDATE t_Fixing SET Grade = 100 WHERE ID = 2");
          this.Ets.Pages.RedirectToUrl(url);
          break;
        case 7:
          UpdateTaskUserState(tasks, 5);
          this.Ets.Pages.RedirectToUrl(url);
          break;
        case 9:
          UpdateTaskUserState(tasks, 6);
          this.Ets.Pages.RedirectToUrl(url);
          break;
        case 16:
          UpdateTaskUserState(tasks, 10);
          this.Ets.Pages.RedirectToUrl(url);
          break;
        case 19:
          UpdateTaskUserState(tasks, 9);
          this.Ets.Pages.RedirectToUrl(url);
          break;
        case 20:
          UpdateTaskUserState(tasks, 8);
          this.Ets.Pages.RedirectToUrl(url);
          break;
        case 15:
          this.Ets.Api.Tags.UpdateVirtualTagByID(432, 0, uow);
          this.Ets.Api.Tags.UpdateVirtualTagByID(433, 0, uow);
          this.Ets.Api.Tags.UpdateVirtualTagByID(592, 0, uow);
          this.Ets.Api.Tags.UpdateVirtualTagByID(417, 0, uow);
          this.Ets.Api.Tags.UpdateVirtualTagByID(416, 0, uow);
          this.Ets.Api.Tags.UpdateVirtualTagByID(413, 0, uow);
          uow.ExecuteReturnsResultObject().ThrowIfFailed();

          List<ETS.Core.Api.Models.Data.DbItem> items = new List<ETS.Core.Api.Models.Data.DbItem>();

          // Load the list of items from the database
          items = api.Data.ListOf.DbItems.GetList.WithSql("SELECT * FROM tItem");

          // Find the items that match the MaterialID and LocationIDs in items list
          int locationIDsToCheck = 20;
          var matchingItems = items.Where(i => locationIDsToCheck == i.LocationID);

          // Find the item with the lowest ID among the matching items
          var itemWithLowestID = matchingItems.OrderBy(i => i.ID).FirstOrDefault();
          if (itemWithLowestID != null)
          {

            itemWithLowestID.LocationID = 21;

            api.Data.DbItem.Save.UpdateExisting(itemWithLowestID);
            ETS.Core.Api.Models.Data.DbItemLog itemLog = new ETS.Core.Api.Models.Data.DbItemLog();

            itemLog.User = this.Ets.User.Login;
            itemLog.ItemLogDefinitionID = 5;
            itemLog.LogDateTime = this.Ets.SiteNow;
            itemLog.Lot = itemWithLowestID.Lot;
            itemLog.ItemID = itemWithLowestID.ID;
            itemLog.Quantity = 1;
            itemLog.LocationID = 21;
            itemLog.JobID = itemWithLowestID.JobID;
            itemLog.SubLot = itemWithLowestID.Lot;
            itemLog.Notes = "Picked up by customer";
            itemLog.Capture10 = itemWithLowestID.Attribute10;
            if (itemWithLowestID.MaterialID == 1)
            {
              itemLog.ProductID = 9;
            }
            if (itemWithLowestID.MaterialID == 2)
            {
              itemLog.ProductID = 10;
            }
            if (itemWithLowestID.MaterialID == 3)
            {
              itemLog.ProductID = 8;
            }

            ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbItemLog> result2;

            result2 = api.Data.DbItemLog.Save.InsertAsNew(itemLog);

            this.Ets.Pages.RedirectToUrl(url);
          }
          UpdateTaskUserState(tasks, 2);
          break;
          this.Ets.Pages.RedirectToUrl(url);

        // Add more cases if needed for other values of taskFormItemID
        // case otherValue:
        //     // Do something for otherValue
        //     break;
        default:
          // Code to execute if taskFormItemID doesn't match any of the cases
          break;
          if (result.Success)
          {
            int newID = result.Return.ID;
          }
          else
          {
            return;
          }

          var url3 = this.Ets.Pages.PageUrl;
          this.Ets.Pages.RedirectToUrl(url3);

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
    protected override bool ContentPage_Final()
    {
      return true;
    }
  }
}
