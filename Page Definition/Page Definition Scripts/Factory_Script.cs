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
  public partial class Home : ContentPageBase
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
    private void btnGenerate_Click(object sender, EventArgs e){

    }
    protected override bool ContentPage_Init()
    {
       var menuItems = new List<MenuItem>();
      var hubs = this.Ets.Api.Data.DbPageDefinition.GetList.ForSitePageDefinitionID(this.Ets.Pages.Site.ID);
      foreach (var hub in hubs)
      {
        if(!hub.IsVisibleInNavigation) continue;
        if(hub.PageLevelType != PageDefinitionLevel.Section) continue;
        if(hub.NavigationTitle == "ets.icon") continue;
        
        string route = "{0}{1}/".FormatWith(this.Ets.Pages.BuildUrlPathForLevel(PageDefinitionLevel.Site), hub.Key);
        bool hasPerm = this.Ets.HasPagePermission(route);
        
        this.Ets.Debug.Trace(route + " " + hasPerm.AsString());
        
        if(!hasPerm) continue;
        
        menuItems.Add(new MenuItem
        {
          MainText = (hub.PageTitle.IsNullOrWhiteSpace()) ? hub.Name : hub.PageTitle,
          SubText = (hub.Description.IsNullOrWhiteSpace()) ? "" : hub.Description,
          Icon = hub.NavigationIconCss,
          Url = hub.Key
        });
      }
      this.Ets.Values["Data.Menu"] = menuItems;
      return true;
    }

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
    //protected override bool ContentPage_PartPreInit02()
    //{
    //  return true;
    //}

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
    //protected override bool ContentPage_PartPreInit05()
    //{
    //  return true;
   // }

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
    //protected override bool ContentPage_PartPreInit10()
    //{
    //  return true;
    //}

    /// ***********************************************************
    /// <remarks>
    /// All Content Parts have been loaded/initialized.  This is
    /// the ideal location to directly access/modify Content Part
    /// properties, as well as show/hide/manipulate page elements.
    ///
    /// At this point, adding or changing Ets.Values data will no
    /// longer serve a purpose as all Content Parts have accessed
    /// what they require.
    /// </remarks>
    /// ***********************************************************
    //protected override bool ContentPage_Final()
    //{
    //  return true;
    //}
  //}

   ///***********************************************************
  public class MenuItem
  {
    public string MainText { get; set; } = "";
    public string SubText { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Url { get; set; } = "";
  }
}
