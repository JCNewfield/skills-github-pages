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
using ETS.Core.Services.Resource;
using ETS.Ts.Core.ContentParts;
using ETS.Ts.Core.Enums;
using ETS.Ts.Core.Scripting;

namespace ETS.Ts.Content
{
  /// ***********************************************************
  /// <templateauthor>Parsec</templateauthor>
  /// <templateversion>Created from template 10.0.0</templateversion>
  /// ***********************************************************
  public partial class Salesstart : ContentPageBase
  {
    /// <remarks>
    /// Hardcode the various settings, convert them to ValuesProperty
    /// if you want them driven from the QueryString/ValuesDictionary
    /// 
    /// _initialDelayMs:
    ///   Waits this many milliseconds on the first request to allow for
    ///   the posibility to go to directly to the SuccessUrl
    /// _timeoutSeconds:
    ///   After this many seconds, the Timeout message will be displayed.
    /// _refreshIntervalSeconds:
    ///   How often do we poll the server to see if the Wait condition is done.
    /// </remarks>
    private int _initialDelayMs = 1000;
    private int _timeoutSeconds = 10;
    private int _refreshIntervalSeconds = 2;

    // TODO add data driven parameters that can be used in ContentPage_Init and/or IsWaitConditionSatisfied
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
      if (!this.IsPostBack)
      {
        this.Ets.Pages.ContentPageRefreshSeconds = _refreshIntervalSeconds;
        var expiredAtString = this.Ets.SiteNow.AddSeconds(_timeoutSeconds).ToUniversalDateTimeString();
        this.InputExpireValue.InputValue = expiredAtString;

        if (_initialDelayMs > 0) { System.Threading.Thread.Sleep(_initialDelayMs); }
      }

      return true;
    }

    /// ***********************************************************
    protected bool IsWaitConditionSatisfied()
    {

      // TODO write a sql statement or some other logic to determine
      // if the Wait condition is satisfied.

      string sql = "SELECT Value from tTag WHERE ID = 592";

      if (this.Ets.Api.Util.Db.ExecuteScalar<bool>(sql).ThrowIfFailed() == true) return true;

      return false;

      // return this.Ets.Api.Util.Db.ExecuteScalar<bool>(sql).ThrowIfFailed();
    }

    /// ***********************************************************
    protected override bool ContentPage_PartPreInit10()
    {
      // TODO customize messages based on if we are waiting or timedout

      // check for timeout
      var expiredDateTime = this.Ets.Form.GetValueAsDateTimeOffsetByKey(this.InputExpireValue.FormKey, this.Ets.SiteNow);
      if (this.Ets.SiteNow > expiredDateTime)
      {
        this.Ets.Values["HeaderTimeout.Visible"] = true;
        this.Ets.Values["HeaderMessage.Visible"] = false;

        this.Ets.Values["HeaderTimeout"] = "Timeout";
        // examples
        // this.Ets.Values["Message.MarkDown"] = "The thing **{0}** did not something.".FormatWith(this.EntityName);
        this.Ets.Values["Message.MarkDown"] = "Loading Overview";
      }
      else
      {
        this.Ets.Values["HeaderTimeout.Visible"] = false;
        this.Ets.Values["HeaderMessage.Visible"] = true;

        this.Ets.Values["HeaderText"] = "Waiting";
        // examples
        // this.Ets.Values["Message.MarkDown"] = "Waiting for **{0}** to something...".FormatWith(this.EntityName);
        this.Ets.Values["Message.MarkDown"] = "Waiting Markdown...";

        var url = "http://10.19.10.4/TS/pages/home/batching/batchoverview/";

        // check condition
        if (this.IsWaitConditionSatisfied()) return this.Ets.Pages.RedirectToUrl(url);
      }

      return true;
    }
  }
}