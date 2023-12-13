using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using System.Web.UI.DataVisualization.Charting;
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
  public partial class Jobs : ContentPageBase
  {
    /// ***********************************************************
    protected override bool ContentPage_Init()
    {
      // const string fDefault = "NOLOT";
      
      // if (!this.Ets.Values.ContainsKey("f"))
      // { 
      //   int itemID = this.Ets.Values.GetAsInt("ItemID", this.Ets.Session.GetAsInt("ItemID", -1));
      //   if(itemID.IsNotNull())
      //   {
      //     DbItem item = this.Ets.Api.Data.DbItem.Load.ByID(itemID);
      //     if(item != null)
      //     {
      //       this.Ets.Values["f"] = item.Lot;
      //       DbJob job = this.Ets.Api.Data.DbJob.Load.ByID(item.JobID);
            
      //       if(job != null)
      //       {
      //         this.Ets.Values["JobName"] = job.Name;
      //       }
            
      //     }
      //   }
      //   else
      //   {
      //     this.Ets.Values["f"] = fDefault; 
      //   }
      // }
      
      // var fVal = this.Ets.Values.GetAsString("f", fDefault);
      // this.Ets.Values["IsFocusSet"] = (fVal != fDefault);
      
      return true;
    }

    /// ***********************************************************
    protected override bool ContentPage_PartPreInit02()
    {
      var fVal = this.Ets.Values.GetAsString("f");
      if (fVal != "NOLOT") this.Ets.Values["JobName"] = fVal;
      return true;
    }

    /// ***********************************************************
    protected override bool ContentPage_PartPreInit10()
    {
      this.Ets.Values["LatestJob.Name"] = this.Ets.Values.GetAsString("Data.LatestJob.Selected.JobName");
      // this.Ets.Values["LatestJob.Name"] = "Job231.1";
      return true;
    }

    /// ***********************************************************
    protected override bool ContentPage_Final()
    {
      this.Ets.Values["Data.Job.Card"] = (this.Ets.Values.GetAsBool("Data.Job.HasData", false) ? 1 : 0);
      this.Ets.Values["Data.Trace.Focus.Card"] = (this.Ets.Values.GetAsBool("Data.Trace.Focus.HasData", false) ? 1 : 0);
      this.Ets.Values["Data.Trace.Select.Card"] = (this.Ets.Values.GetAsBool("Data.Trace.Select.HasData", false) ? 1 : 0);
      
      return true;
    }
  }
}