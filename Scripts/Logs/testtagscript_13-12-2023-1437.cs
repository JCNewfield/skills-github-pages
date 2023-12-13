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
  /// This class contains code that can be executed by a script tag.
  /// </summary>
  /// ******************************************************************
  public class testtagscript : ScriptClassTagBase
  {
    /// ******************************************************************
    /// <summary>
    /// This method is called when the script tag is initialized.
    /// </summary>
    /// <param name="id">This is the ID for the script tag.</param> 
    /// ******************************************************************
    public override bool Initialize(ITagScriptInitializeContext context, int id)
    {
      return true;
    }

    /// ******************************************************************
    /// <summary>
    /// This method is called to evaluate the value for the script tag.
    /// </summary>
    /// ******************************************************************
    public override object EvaluateValue()
    {
      return null;
    }
  }
}