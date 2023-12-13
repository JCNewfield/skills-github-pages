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
using ETS.Core.Services.Resource;
using ETS.Ts.Core.ContentParts;
using ETS.Ts.Core.Enums;
using ETS.Ts.Core.Scripting;
using System.Threading.Tasks;
using ETS.Core.Scripting;
using System.Threading;

namespace ETS.Ts.Content
{
  /// ***********************************************************
  public partial class Step_overview : ContentPageBase
  {
    /// Automatically map the decorated property to incoming Ets.Values data (typically QueryString variables).
    /// The name of the property is used as the Key in accessing the Ets.Values collection during mapping.
    [ValuesProperty()]
    public int SystemID { get; set; } = 1;

    [ValuesProperty()]
    public int tab { get; set; } = 1;

    [ValuesProperty()]
    public int vis { get; set; } = 0;

    private DbJob _job;
    private DbBatch _batch;
    // private DbSystem _system;
    // private GlobalBatch _manager;

    // ***********************************************************
    protected override bool ContentPage_Init()
    {
      // _system = Ets.Api.Data.DbSystem.Load.ByID(SystemID);
      // _manager = new GlobalBatch(_system, _job, _batch);
      SomeMethod();
      _job = this.Ets.Api.Data.DbJob.Load.CurrentBySystemID(this.SystemID);
      _batch = this.Ets.Api.Data.DbBatch.Load.CurrentBySystemID(this.SystemID);
      if (null == _job || null == _batch) return true;
      this.Ets.Values["JobID"] = _job.ID;
      this.Ets.Values["BatchID"] = _batch.ID;

      var sql = $@"SELECT TOP 1 ID 
FROM tBatchStep bs
WHERE bs.BatchID = {_batch.ID.ToSql()}
AND StartDateTime IS NULL
AND EndDateTime IS NULL
ORDER BY StartSequence, EndSequence";
      this.Ets.Values["NextStepID"] = this.Ets.Api.Util.Db.ExecuteScalar<int>(sql).ThrowIfFailed();
      return true;
    }

    // ***********************************************************
    protected override bool ContentPage_PartPreInit05()
    {
      if (vis == 1)
      {
        var sql = $@"SELECT s.ID [GroupValue]
        , s.Name [GroupLabel]
        , s.ID [GroupDisplayOrder]
        
        , 0 [ValueDifference]
        , null [ValueID]
        , bs.ID [ValueGroupID]
        , s.name + ' - ' + fd.Name [ValueGroupName]
        , CASE 
        	WHEN bs.StartDateTime IS NULL THEN 'gray' --grey / not started
        	WHEN bs.EndDateTime IS NULL THEN 'green' --green / in-progress
        	ELSE 'blue' --blue / completed
        END [ValueGroupColor]
        , bs.PlannedDurationSeconds
        , bs.StartDateTime
        , bs.EndDateTime
        FROM tBatchStep bs
        JOIN tFunctionDefinition fd
        ON bs.FunctionDefinitionID = fd.ID
        JOIN tSystem s
        ON fd.SubSystemID = s.ID
        WHERE bs.BatchID = {this._batch.ID.ToSql()}
        ORDER BY bs.StartSequence, bs.EndSequence";
        DataTable dt = this.Ets.Api.Util.Db.GetDataTable(sql).ThrowIfFailed($"Could not load Batch Steps for Batch ID {this._batch.ID}");
        dt.Columns.Add("ValueStart", typeof(DateTimeOffset));
        dt.Columns.Add("ValueEnd", typeof(DateTimeOffset));

        DateTimeOffset timestamp = _batch.StartDateTime;
        this.Ets.Values["StepsSDT"] = timestamp;
        var selectedRowID = this.Ets.Values.GetAsInt("StepID", -1);
        var selectedRows = new List<DataRow>();

        foreach (DataRow dr in dt.Rows)
        {
          if (dr.GetInteger("ValueGroupID") == selectedRowID) { selectedRows.Add(dr); }

          if (dr.GetDateTimeOffset("StartDateTime").IsNull())
          {
            timestamp = (timestamp > this.Ets.SiteNow) ? timestamp : this.Ets.SiteNow;
            dr["ValueStart"] = timestamp;

            dr["ValueDifference"] = dr.GetInteger("PlannedDurationSeconds");
            timestamp = timestamp.AddSeconds(dr.GetInteger("PlannedDurationSeconds"));
            dr["ValueEnd"] = timestamp;
          }
          else if (dr.GetDateTimeOffset("EndDateTime").IsNull())
          {
            timestamp = dr.GetDateTimeOffset("StartDateTime");
            dr["ValueStart"] = timestamp;
            dr["ValueDifference"] = (this.Ets.SiteNow.Subtract(timestamp).TotalSeconds > dr.GetInteger("PlannedDurationSeconds"))
              ? Math.Floor(this.Ets.SiteNow.Subtract(timestamp).TotalSeconds) : dr.GetInteger("PlannedDurationSeconds");
            timestamp = timestamp.AddSeconds(dr.GetInteger("ValueDifference"));
            dr["ValueEnd"] = timestamp;
          }
          else
          {
            dr["ValueStart"] = dr.GetDateTimeOffset("StartDateTime");
            dr["ValueEnd"] = dr.GetDateTimeOffset("EndDateTime");
            dr["ValueDifference"] = dr.GetDateTimeOffset("EndDateTime").Subtract(dr.GetDateTimeOffset("StartDateTime")).TotalSeconds;
            timestamp = dr.GetDateTimeOffset("EndDateTime");
          }
        }
        this.Ets.Values["StepsEDT"] = timestamp;
        this.Ets.Values["Data.BatchSteps"] = dt;
        this.Ets.Values.AddDataTableInformationWithSelected(dt, "Data.BatchSteps", true, "ValueGroupID", selectedRowID.ToString(), selectedRows);
      }
      return true;
    }
    // ***********************************************************
    protected override bool ContentPage_PartPreInit10()
    {


      if (!this.Ets.Values.GetAsBool("Data.BatchSteps.HasData", false))
      {
        this.Ets.Values["Tab"] = 0;
        this.Ets.Values["Vis"] = 0;
      }
      return true;
    }

    // ***********************************************************
    private void Click_StartStep(object sender, RowItemEventArgs e)
    {

      try
      {
        var uow = this.Ets.Api.CreateUnitOfWork();

        var sql = $@"SELECT TOP 1 fd.TriggerTagID
          FROM tBatchStep bs 
          JOIN tFunctionDefinition fd
          ON bs.FunctionDefinitionID = fd.ID
          WHERE bs.BatchID = {this._batch.ID.ToSql()} 
          AND bs.StartDateTime IS NOT NULL 
          AND bs.EndDateTime IS NULL";
        var triggerTagID = this.Ets.Api.Util.Db.ExecuteScalar<int>(sql, -1).ThrowIfFailed();
        if (triggerTagID != -1) { this.Ets.Api.Tags.UpdateVirtualTagByID(triggerTagID, false, uow).ThrowIfFailed(); }

        var nextStepID = this.Ets.Values.GetAsInt("NextStepID", -1);
        var nextStep = this.Ets.Api.Data.DbBatchStep.Load.ByID(nextStepID).ThrowIfLoadFailed("NextStepID", nextStepID);
        sql = $@"SELECT TOP 1 fd.TriggerTagID
            FROM tBatchStep bs 
            JOIN tFunctionDefinition fd
            ON bs.FunctionDefinitionID = fd.ID
            WHERE bs.ID = {nextStep.ID.ToSql()}";
        triggerTagID = this.Ets.Api.Util.Db.ExecuteScalar<int>(sql).ThrowIfFailed();
        this.Ets.Api.Tags.UpdateVirtualTagByID(triggerTagID, 1, uow).ThrowIfFailed();
        // _manager.TriggerNextStep(triggerTagID);


        if (triggerTagID == 145)
        {

          this.Ets.Api.Tags.UpdateVirtualTagByID(29, 0, uow).ThrowIfFailed();
          this.Ets.Api.Tags.UpdateVirtualTagByID(26, 0, uow).ThrowIfFailed();
          this.Ets.Api.Tags.UpdateVirtualTagByID(27, 0, uow).ThrowIfFailed();
          this.Ets.Api.Tags.UpdateVirtualTagByID(139, 0, uow).ThrowIfFailed();
          this.Ets.Api.Tags.UpdateVirtualTagByID(145, 0, uow).ThrowIfFailed();

        }


        var result = uow.ExecuteReturnsResultObject().ThrowIfFailed();
        // var url = this.Ets.Pages.PageUrl;
        // this.Ets.Pages.RedirectToUrl(url);
        Thread.Sleep(1000);
        var url = this.Ets.ProcessExpressionUrl("?WaitForBatchStart&BatchStepID={nextStepID}");
        this.Ets.Pages.RedirectToUrl(url);
      }
      catch (Exception ex)
      {
        this.Ets.Debug.FailFromException(ex);
      }
    }
    public void SomeMethod()
    {

      string strBatchId = this.Ets.Api.Tags.Load.ByName("F_BATCH_OVEN_LOAD").Value;
      if (this.Ets.Api.Tags.Load.ByName("F_BATCH_OVEN_LOAD").Value == "0")
      {

        this.Ets.Api.Tags.UpdateVirtualTagByID(139, 0);


        //this.Ets.Api.Tags.UpdateVirtualTagByID(141, 1);


      }
    }



  }
}