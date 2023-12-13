using System;
using System.Collections.Generic;
using ETS.Core;
using ETS.Core.Api;
using ETS.Core.Api.Models.Data;
using ETS.Core.Enums;
using ETS.Core.Extensions;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;

using System.Web;



using ETS.Core.Services.Resource;



namespace ETS.Core.Scripting
{
  /// ******************************************************************
  /// <summary>
  /// This class contains code that can be executed by a System
  /// instance that is attached using the Script Class Name setting.
  /// </summary>
  /// ******************************************************************
  public class BatchScript : ETS.Core.Scripting.SystemScriptClassBase
  {
    /* ---------------------------------------------------------------- */
    #region Properties
    public int ID { get; set; }
    public string Name { get; set; }
    public DbItem item;
    public DbItemLog itemLog;
    public DbItemLogDefinition _itemLogDef;
    #endregion 
    /* ---------------------------------------------------------------- */

    /// ******************************************************************
    /// <summary>
    /// This method is called when the configuration item that references
    /// this class is being loaded.  Other configuration items may not be
    /// loaded into memory at this point.
    /// </summary>
    /// <param name="id">This is the ID for the instantiating entity.</param>
    /// ******************************************************************
    public override bool LoadAndInitialize(ILoadAndInitializeContext context, int id)
    {
      // load system information
      var sys = context.Api.Data.DbSystem.Load.ByID(id);
      if (sys == null) return context.Api.Util.Log.WriteWarning("Unable to load System object for ID {0}.".FormatWith(id), "SystemScript");
      this.ID = sys.ID;
      this.Name = sys.Name;
      

      return true;
    }

    /// ******************************************************************
    /// <summary>
    /// This method is called after the configuration is loaded but before
    /// the first Logic Service scan.
    /// </summary>
    /// ******************************************************************
    public override void Startup(IStartupContext context)
    {
    }

    /// ******************************************************************
    /// <summary>This method is called at the start of each Logic Service scan.</summary>
    /// ******************************************************************
    public override void PreScan(IPreScanContext context)
    {
    }

    /// ******************************************************************
    /// <summary>
    /// This method is called at the end of each Logic Service scan AFTER all
    /// the previous PostScan methods are called.
    /// </summary>
    /// ******************************************************************
    public override void PostScan(IPostScanContext context)
    {
    }

    /// ******************************************************************
    /// The following method is triggered if a subscribed tag's value was 
    /// changed in the preceding scan.  If triggered, this method is called
    /// at the end of the Logic Service scan.  To trigger this method, subscribe
    /// to tags in LoadAndInitialize.
    /// ******************************************************************
    public override void PostScanTagChanged(IPostScanTagChangedContext context) { }

    /// ******************************************************************
    /// The following methods are triggered if a new Shift was started or
    /// ended in the preceding scan.  If triggered, these methods are called
    /// at the end of the Logic Service scan.
    /// ******************************************************************
    public override void PostScanShiftHistoryStart(IPostScanShiftHistoryStartContext context) { }
    public override void PostScanShiftHistoryEnd(IPostScanShiftHistoryEndContext context) { }

    /// ******************************************************************
    /// The following methods are triggered if an OEE Interval (for a
    /// calculation that is assigned to this System) is started, ended
    /// or updated in the preceding scan.  If triggered, these methods
    /// are called at the end of the Logic Service scan.
    /// ******************************************************************
    public override void PostScanOeeIntervalStart(IPostScanOeeIntervalStartContext context) { }
    public override void PostScanOeeIntervalUpdate(IPostScanOeeIntervalUpdateContext context) { }
    public override void PostScanOeeIntervalEnd(IPostScanOeeIntervalEndContext context) { }

    /// ******************************************************************
    /// The following methods are triggered if an Event (for an Event Definition
    /// that is assigned to this System) is started, ended or OEE Type updated in
    /// the preceding scan.  If triggered, these methods are called at the end
    /// of the Logic Service scan.
    /// ******************************************************************
    public override void PostScanEventStart(IPostScanEventStartContext context) { }
    public override void PostScanEventEnd(IPostScanEventEndContext context) { }
    public override void PostScanEventOeeTypeChanged(IPostScanEventOeeTypeChangedContext context) { }

    /// ******************************************************************
    /// The following method is triggered for Task (for a Task Definition
    /// that is assigned to this System) activities in the preceding scan.
    /// If triggered, these methods are called at the end of the Logic Service scan.
    /// ******************************************************************
    public override void PostScanTaskCreated(IPostScanTaskCreatedContext context) { }
    public override void PostScanTaskLate(IPostScanTaskLateContext context) { }
    public override void PostScanTaskCompleted(IPostScanTaskCompletedContext context) { }
    public override void PostScanTaskUserStateChanged(IPostScanTaskUserStateChangedContext context) { }
    public override void PostScanTaskPassed(IPostScanTaskPassedContext context) { }
    public override void PostScanTaskFailed(IPostScanTaskFailedContext context) { }

    /// ******************************************************************
    /// The following methods are triggered if an Transfer (for an Transfer Definition
    /// that is assigned to this System) is started or ended. If triggered, these method
    /// are called at the end of the Logic Service scan.
    /// ******************************************************************
    public override void PostScanTransferStart(IPostScanTransferStartContext context) { }
    public override void PostScanTransferEnd(IPostScanTransferEndContext context) { }

    /// ******************************************************************
    /// The following method is triggered for Sample Sub Groups (for a Sample Definition
    /// that is assigned to this System) activities in the preceding scan.
    /// If triggered, these methods are called at the end of the Logic Service scan.
    /// ******************************************************************
    public override void PostScanSampleSubGroupCreated(IPostScanSampleSubGroupCreatedContext context) { }
    public override void PostScanSampleSubGroupRule(IPostScanSampleSubGroupRuleContext context) { }

    /// ******************************************************************
    /// The following methods are triggered if a Job (for this System) is
    /// started or ended in the preceding scan.  If triggered, these methods
    /// are called at the end of the Logic Service scan.
    /// ******************************************************************
    public override void PostScanJobStart(IPostScanJobStartContext context) { }
    public override void PostScanJobEnd(IPostScanJobEndContext context)
    {

    }

    /// ******************************************************************
    /// The following methods are triggered if a Batch (for this System) is
    /// started or ended in the preceding scan.  If triggered, these methods
    /// are called at the end of the Logic Service scan.
    /// ******************************************************************
    public override void PostScanBatchStart(IPostScanBatchStartContext context)
    {


    }
    public override void PostScanBatchEnd(IPostScanBatchEndContext context)
    {


    }
    private static Timer timer = null;
    /// ******************************************************************
    /// The following methods are triggered if a Batch Step (for this System)
    /// is started, ended or an Overage is created in the preceding scan.  If
    /// triggered, these methods are called at the end of the Logic Service scan.
    /// ******************************************************************
    public override void PostScanBatchStepStart(IPostScanBatchStepStartContext context)
    {
      // checks if MPO.X_active is triggered and if it is then sets oven load to true
      if (Api.Tags.Load.ByID(521).Value == "True")
      {
        Api.Tags.UpdateVirtualTagByID(140, 1);
      }
      string strBatchId = Api.Tags.Load.ByName("F_BATCH_OVEN_LOAD").Value;
      if (Api.Tags.Load.ByName("F_BATCH_OVEN_LOAD").Value == "1" && timer == null)
      {
        Api.Util.Log.WriteInformation("CREATING TIMER", "Mer Major Error");
        Api.Tags.UpdateVirtualTagByID(139, 0);

        // Create a new timer with an interval of 4 seconds (4000 milliseconds)
        timer = new Timer(4000);
        // Hook up the Elapsed event for the timer
        timer.Elapsed += OnTimerElapsed;
        // Set the timer to fire only once (false by default, but just to be explicit)
        timer.AutoReset = false;
        // Start the timer
        timer.Start();

      }
      //only to make sure that the job ends when it has reached batch_end step
      string endBatchId = Api.Tags.Load.ByName("F_BATCH_END").Value;
      if (Api.Tags.Load.ByName("F_BATCH_END").Value == "1")
      {
        //Api.Tags.UpdateVirtualTagByID(300, 1);
        Api.Tags.UpdateVirtualTagByID(29, 0);

        Api.Tags.UpdateVirtualTagByID(26, 0);

        Api.Tags.UpdateVirtualTagByID(27, 0);

        Api.Tags.UpdateVirtualTagByID(139, 0);
        Api.Tags.UpdateVirtualTagByID(144, 0);

        Api.Tags.UpdateVirtualTagByID(145, 0);

      }
    }
    // oven heating step
    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
      timer.Stop();
      // This method will be called when the timer elapses (after 4 seconds)
      Api.Util.Log.WriteInformation("1", "Function triggerd");
      // Update virtual tag with ID 141 to 1
      Api.Tags.UpdateVirtualTagByID(140, 0);
      Api.Tags.UpdateVirtualTagByID(141, 1);
      // timer = new Timer(7000);
      timer.Interval = 7000;
      // Hook up the Elapsed event for the timer
      timer.Elapsed -= OnTimerElapsed;
      timer.Elapsed += OnTimerElapsed2;
      // Set the timer to fire only once (false by default, but just to be explicit)
      // timer.AutoReset = false;
      // Start the timer
      timer.Start();
    }

    //load rotator/transport to rotator
    private void OnTimerElapsed2(object sender, ElapsedEventArgs e)
    {
      // This method will be called when the timer elapses (after 7 seconds)
      Api.Util.Log.WriteInformation("2", "Function triggerd");
      // Update virtual tag with ID 141 to 1
      Api.Tags.UpdateVirtualTagByID(141, 0);
      Api.Tags.UpdateVirtualTagByID(142, 1);
      timer = new Timer(15000);
      // Hook up the Elapsed event for the timer
      timer.Elapsed -= OnTimerElapsed2;
      timer.Elapsed += OnTimerElapsed3;
      // Set the timer to fire only once (false by default, but just to be explicit)
      timer.AutoReset = false;
      // Start the timer
      timer.Start();

    }

    //rotate step
    private void OnTimerElapsed3(object sender, ElapsedEventArgs e)
    {
      // This method will be called when the timer elapses (after 15 seconds)
      Api.Util.Log.WriteInformation("3", "Function triggerd");
      // Update virtual tag with ID 141 to 1
      Api.Tags.UpdateVirtualTagByID(142, 0);
      Api.Tags.UpdateVirtualTagByID(143, 1);
      timer = new Timer(4000);
      // Hook up the Elapsed event for the timer
      timer.Elapsed -= OnTimerElapsed3;
      timer.Elapsed += OnTimerElapsed4;
      // Set the timer to fire only once (false by default, but just to be explicit)
      timer.AutoReset = false;
      // Start the timer
      timer.Start();

    }

    // slicer step
    private void OnTimerElapsed4(object sender, ElapsedEventArgs e)
    {
      // This method will be called when the timer elapses (after 15 seconds)
      Api.Util.Log.WriteInformation("4", "Function triggerd");
      // Update virtual tag with ID 141 to 1
      Api.Tags.UpdateVirtualTagByID(143, 0);
      Api.Tags.UpdateVirtualTagByID(144, 1);
      timer = new Timer(8000);
      // Hook up the Elapsed event for the timer
      timer.Elapsed -= OnTimerElapsed4;
      timer.Elapsed += OnTimerElapsed5;
      // Set the timer to fire only once (false by default, but just to be explicit)
      timer.AutoReset = false;
      // Start the timer
      timer.Start();


    }

    // batch end step that sets everything to 0 so the job end
    private void OnTimerElapsed5(object sender, ElapsedEventArgs e)
    {
      Api.Util.Log.WriteInformation("5", "Function triggerd");

      // updates the states of three condtiions on maint dashboard
      var uVGR = Api.Util.Db.ExecuteScalar<int>($"UPDATE t_Fixing SET Grade = Grade - 5 WHERE ID = 1");
      var uOven = Api.Util.Db.ExecuteScalar<int>($"UPDATE t_Fixing SET Grade = Grade - 10 WHERE ID = 2");
      var uCutter = Api.Util.Db.ExecuteScalar<int>($"UPDATE t_Fixing SET Grade = Grade - 15 WHERE ID = 3");

      timer.Elapsed -= OnTimerElapsed5;
      timer = null;

      // sets all tags to zero when the last timer has been completed/job is completed
      Api.Tags.UpdateVirtualTagByID(29, 0);

      Api.Tags.UpdateVirtualTagByID(26, 0);

      Api.Tags.UpdateVirtualTagByID(27, 0);

      Api.Tags.UpdateVirtualTagByID(139, 0);
      Api.Tags.UpdateVirtualTagByID(140, 0);
      Api.Tags.UpdateVirtualTagByID(141, 0);
      Api.Tags.UpdateVirtualTagByID(142, 0);
      Api.Tags.UpdateVirtualTagByID(143, 0);
      Api.Tags.UpdateVirtualTagByID(144, 0);

      Api.Tags.UpdateVirtualTagByID(145, 0);
      Api.Tags.UpdateVirtualTagByID(588, 1);
      // Api.Tags.UpdateVirtualTagByID(562, "NO");


      // create a itemlog for the item
      
      // Declare a list object to store items
      List<Api.Models.Data.DbItem> items = new List<Api.Models.Data.DbItem>();

      // Load the list of items from the database
      items = Api.Data.ListOf.DbItems.GetList.WithSql("SELECT * FROM tItem");

      // Define a list of LocationIDs that you want to check (1 in this case)
      List<int> locationIDsToCheck = new List<int> { 1 };

      // Find the items that match the MaterialID and LocationIDs in the items list
      var matchingItems = items.Where(i => locationIDsToCheck.Contains(i.LocationID));

      // Find the item with the lowest ID among the matching items
      var itemWithLowestID = matchingItems.OrderBy(i => i.ID).FirstOrDefault();


      if (itemWithLowestID != null)
      {
        Api.Models.Data.DbItemLog itemLog = new Api.Models.Data.DbItemLog();

        //raw trout
        //checks if the item has the material of trout and updated item and itemlog with the correcrt locations and productid for that material
        if (itemWithLowestID.MaterialID == 1)
        {
          itemWithLowestID.LocationID = 18;
          itemWithLowestID.ProductID = 3;
          itemLog.LocationID = 18;
          itemLog.ProductID = 3;
        }

        //raw cod
        //checks if the item has the material of cod and updated item and itemlog with the correcrt locations and productid for that material
        if (itemWithLowestID.MaterialID == 2)
        {
          itemWithLowestID.LocationID = 17;
          itemWithLowestID.ProductID = 2;
          itemLog.LocationID = 17;
          itemLog.ProductID = 2;
        }

        //raw salmon
        //checks if the item has the material of salmon and updated item and itemlog with the correcrt locations and productid for that material
        if (itemWithLowestID.MaterialID == 3)
        {
          itemWithLowestID.LocationID = 19;
          itemWithLowestID.ProductID = 1;
          itemLog.LocationID = 19;
          itemLog.ProductID = 1;
        }
        Api.Data.DbItem.Save.UpdateExisting(itemWithLowestID);

        itemLog.ItemLogDefinitionID = 3;
        itemLog.LogDateTime = Api.Site.GetCurrentDateTime();
        itemLog.Lot = itemWithLowestID.Lot;
        itemLog.ItemID = itemWithLowestID.ID;
        itemLog.Quantity = 1;
        itemLog.JobID = itemWithLowestID.JobID;
        itemLog.Notes = "Moved to finished product warehouse";

        Api.Models.Result<Api.Models.Data.DbItemLog> result;

        // Insert the entity in the database (The ID will be created when inserted to the database)
        result = Api.Data.DbItemLog.Save.InsertAsNew(itemLog);

      }

    }

    public override void PostScanBatchStepEnd(IPostScanBatchStepEndContext context)
    {
      // if the batch step is named F_BATCH_END then finish the job and reset every tag
      if (Api.Tags.Load.ByName("F_BATCH_END").Value == "1")
      {
        //Api.Tags.UpdateVirtualTagByID(300, 1);
        Api.Tags.UpdateVirtualTagByID(29, 0);

        Api.Tags.UpdateVirtualTagByID(26, 0);

        Api.Tags.UpdateVirtualTagByID(27, 0);

        Api.Tags.UpdateVirtualTagByID(139, 0);
        Api.Tags.UpdateVirtualTagByID(140, 0);
        Api.Tags.UpdateVirtualTagByID(141, 0);
        Api.Tags.UpdateVirtualTagByID(142, 0);
        Api.Tags.UpdateVirtualTagByID(143, 0);
        Api.Tags.UpdateVirtualTagByID(144, 0);
        Api.Tags.UpdateVirtualTagByID(145, 0);

        //Trigger a notification for preventative maintenance
        Api.Tags.UpdateVirtualTagByName("MF.PROD.VGR.LUBRICATE", 1);

      }

    }
    public override void PostScanBatchStepOverageStart(IPostScanBatchStepOverageStartContext context) { }
  }
}