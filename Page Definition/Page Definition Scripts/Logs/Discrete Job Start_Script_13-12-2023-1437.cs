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
  /// **********************************************************************************
  /// THIS PAGE WAS DEVELOPED IN 10.2.4 AND MAY NOT FUNCTION PROPERLY IN NEWER VERSIONS.
  /// To request an update to the page, please contact the Applications Team.
  /// applications@parsec-corp.com
  /// **********************************************************************************
  public partial class StartDiscreteJob : ContentPageBase
  {
    [ValuesProperty()]
    public int SystemID { get; set; } = 6;
    
    [ValuesProperty()]
    public int JobID { get; set; } = 155;
    
    public DbJobDiscreteComposite _job;
    public DbSystem _sys;
    /// ***********************************************************
    protected override bool ContentPage_Init()
    {
      _sys = this.Ets.Api.Data.DbSystem.Load.ByID(this.SystemID).ThrowIfLoadFailed("ID", this.SystemID);
      _job = this.Ets.Api.Data.DbJobDiscreteComposite.Load.ByID(this.JobID).ThrowIfLoadFailed("ID", this.JobID);
      
      this.Ets.Values["System.Name"] = _sys.Name;
      this.Ets.Values["Job.Name"] = _job.Name;
      
      return true;
    }

    /// ***********************************************************
    protected override bool ContentPage_PartPreInit02()
    {
      return true;
    }

    /// ***********************************************************
    protected override bool ContentPage_PartPreInit05()
    {
      return true;
    }

    /// ***********************************************************
    protected override bool ContentPage_PartPreInit10()
    {
      return true;
    }

    /// ***********************************************************
    protected override bool ContentPage_Final()
    {
      return true;
    }
    
    /// ***********************************************************
    private void Start_Click(object sender, EventArgs e)
    {
      var uow = this.Ets.Api.CreateUnitOfWork();
      var product = this.Ets.Api.Data.DbProduct.Load.ByID(_job.ProductID).ThrowIfLoadFailed("Job.ProductID", _job.ProductID);
      
      this.Ets.Api.Tags.UpdateVirtualTagByID(_sys.JobTagID, _job.Name, uow);
      this.Ets.Api.Tags.UpdateVirtualTagByID(_sys.ProductTagID, product.ProductCode, uow);
      
      uow.ExecuteReturnsResultObject().ThrowIfFailed();
      
      var args = new Dictionary<string, string>();
      args.Add("c", "ETS.Application.Wait.WaitForJobStart");
      args.Add("SystemID", _sys.ID.AsString());
      args.Add("JobName", _job.Name);
      var url = this.Ets.BuildUrl("", args);
      
      this.Ets.Pages.RedirectToUrl(url);
    }
  }
}