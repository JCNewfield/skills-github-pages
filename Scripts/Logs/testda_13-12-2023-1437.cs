using System;
using System.Collections.Generic;
using ETS.Core;
using ETS.Core.Api;
using ETS.Core.Api.Models.Data;
using ETS.Core.Enums;
using ETS.Core.Extensions;

namespace ETS.Core.Scripting
{
  /// ******************************************************************
  /// <summary>
  /// This class contains code that can be executed by a System
  /// instance that is attached using the Script Class Name setting.
  /// </summary>
  /// ******************************************************************
  public class testda : ETS.Core.Scripting.SystemScriptClassBase
  {
    /* ---------------------------------------------------------------- */
    #region Properties
    public int ID { get; set; }
    public string Name { get; set; }
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
      // this.CustomProp = sys.CustomProperties["FULLKEY"].Value;

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
    public override void PostScanJobEnd(IPostScanJobEndContext context) { }

    /// ******************************************************************
    /// The following methods are triggered if a Batch (for this System) is
    /// started or ended in the preceding scan.  If triggered, these methods
    /// are called at the end of the Logic Service scan.
    /// ******************************************************************
    public override void PostScanBatchStart(IPostScanBatchStartContext context) { }
    public override void PostScanBatchEnd(IPostScanBatchEndContext context) { }

    /// ******************************************************************
    /// The following methods are triggered if a Batch Step (for this System)
    /// is started, ended or an Overage is created in the preceding scan.  If
    /// triggered, these methods are called at the end of the Logic Service scan.
    /// ******************************************************************
    public override void PostScanBatchStepStart(IPostScanBatchStepStartContext context) { }
    public override void PostScanBatchStepEnd(IPostScanBatchStepEndContext context) { }
    public override void PostScanBatchStepOverageStart(IPostScanBatchStepOverageStartContext context) { }
  }
}