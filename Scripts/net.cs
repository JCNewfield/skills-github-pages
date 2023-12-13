using System;
using System.Collections.Generic;
using ETS.Core;
using ETS.Core.Api;
using ETS.Core.Api.Models.Data;
using ETS.Core.Enums;
using ETS.Core.Extensions;

// Changes have been mate at 23/11/2023 - Pavel Ibrahim - New First Change

namespace ETS.Core.Scripting
{
  /// ******************************************************************
  /// <summary>
  /// This class contains code that can be executed by a Logic Service
  /// instance that is attached using the Script Class Name setting.
  /// </summary>
  /// ******************************************************************
  public class net : ETS.Core.Scripting.LogicManagerScriptClassBase
  {
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
    /// <summary>This method is called at the end of each Logic Service scan.</summary>
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
  }
}