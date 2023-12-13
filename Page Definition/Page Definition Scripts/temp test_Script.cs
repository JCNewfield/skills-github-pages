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

namespace ETS.Ts.Content
{
  /// ***********************************************************
  /// <templateauthor>Parsec</templateauthor>
  /// <templateversion>Created from template 10.0.0</templateversion>
  /// <templateinstructions>
  /// The term "Entity" is used in the following code to represent
  /// a type of TrakSYS entity (Product, Material, Event, etc...).
  /// 
  /// Replace the text "Entity" with an appropraite TrakSYS entity
  /// name (i.e. replace "Entity" with "Product").
  /// 
  /// Uncomment and alter code as needed.
  /// </templateinstructions>
  /// ***********************************************************
  public partial class temptest : ContentPageBase
  {
    /// Automatically map the decorated properties to incoming Ets.Values data (typically QueryString variables).
    /// The name of the property is used as the Key in accessing the Ets.Values collection during mapping.

    // actual entity id (used for editing)
    //[ValuesProperty()]
    public int ProductID { get; set; } = -1;

    // entity definition id or parent id (used for new)
    //[ValuesProperty()]
    //public int EntityDefinitionID { get; set; } = -1;

    // model object to hold the primary entity to be edited
    // private DbProduct model;
    
    // helper properties
    public bool IsNew { get { return this.ProductID.IsNew(); } }
    public bool IsEdit { get { return this.ProductID.IsEdit(); } }

    /// ***********************************************************
    protected override bool ContentPage_Init()
    {
      // place IsNew and IsEdit into Values to make it easier to show/hide parts in the UI
      this.Ets.Values["Model.IsNew"] = this.IsNew;
      this.Ets.Values["Model.IsEdit"] = this.IsEdit;
      this.Ets.Pages.IsClientDirtyCheckEnabled = true;
      
     

      //model = this.Ets.Api.Data.DbProduct.Load.ByID(this.ProductID).ThrowIfLoadFailed("ProductID", this.ProductID);

      if (this.IsNew)
      {
        // create new object from definition or parent       
        //var parent = this.Ets.Api.Data.DbEntityDefinition.Load.ByID(this.EntityDefinitionID).ThrowIfLoadFailed("EntityDefinitionID", this.EntityDefinitionID);

        //_model = this.Ets.Api.Data.DbEntity.Create.FromParent(parent).ThrowIfNull("Unable to create new Entity.");
      }
      else
      {
        // load existing object
        //_model = this.Ets.Api.Data.DbEntity.Load.ByID(this.EntityID).ThrowIfLoadFailed("EntityID", this.EntityID);
      }
      
      // copy model into Values to make it accessible to input parts
      // if (!this.Ets.Values.CopyFromModel(model, "Model.")) return false;
      return true;
    }
    
    /// ***********************************************************
    private void Save_Click(object sender, EventArgs e)
    {
      try
      {

        // Get instance of api service
        var api = ETS.Core.Api.ApiService.GetInstance();

        // Create a model object to populate
        ETS.Core.Api.Models.Data.DbItem item = new ETS.Core.Api.Models.Data.DbItem();

        // Populate the model object as needed
        item.ItemDefinitionID = this.Ets.Form.UpdateModelValueByFormKey("Model.ItemDefinitionID", item.ItemDefinitionID, "ItemDefiniton");
        item.MaterialID = this.Ets.Form.UpdateModelValueByFormKey("Model.MaterialID", item.MaterialID, "Material");
        item.LocationID = this.Ets.Form.UpdateModelValueByFormKey("Model.LocationID", item.LocationID, "Location");
        item.UniqueID = this.Ets.Form.UpdateModelValueByFormKey("Model.UniqueID", item.UniqueID, "UniqueID");
        item.Attribute01 = this.Ets.Form.UpdateModelValueByFormKey("Model.RFID", item.Attribute01, "RFID");
        item.ValidFromDateTime = DateTime.Now;
        item.ValidToDateTime = DateTime.Now.AddDays(10);
        //job.PlannedStartDateTime = DateTime.Now;

        // create a result object to determine the success of the operation
        ETS.Core.Api.Models.Result<ETS.Core.Api.Models.Data.DbItem> result;
        
        // Here we insert the entity in the database (The ID will be created when inserted to the database)
        result = api.Data.DbItem.Save.InsertAsNew(item);

        if (result.Success)
        {
          int newID = result.Return.ID;
        }
        else {
          return;
        }


         // populate model with inputs from the UI
         //if (!this.Ets.Form.UpdateModelWithKeyPrefix(model, "Model.")) return;
      


         // assign any calculated data prior to save
         if (this.IsNew)
         {
           //_model.CreatedDateTime = this.Ets.SiteNow;
         }
      
         // validate the entity
         if (!this.ValidateModelData()) return;
      
         // save the entity
         if (!this.SaveModelData()) return;
      
         // has successfully saved, redirect to success
         this.Ets.Pages.RedirectToSuccessUrl();
      }
      catch (Exception ex)
      {
        this.Ets.Debug.FailFromException(ex);
      }
    }
    
    /// ***********************************************************
    private bool ValidateModelData()
    {
      // perform built-in validation
      // var coreValidate = this.Ets.Api.Data.DbProduct.ValidateForMerge(model, this.IsNew);
      // if (!this.Ets.Form.AddResultMessagesWithPrefixIfFailed(coreValidate, "Model.")) return false;
      
      // perform custom validations
      //this.Ets.Form.AddValidationError( ... );
      
      // return validation results
      return this.Ets.Form.IsValid;
    }
    
    /// ***********************************************************
    private bool SaveModelData()
    {
      var uow = this.Ets.Api.CreateUnitOfWork();
      
      // save model (validation can be ignored as it has already been done)
      // this.Ets.Api.Data.DbProduct.MergeIgnoreValidation(model, this.IsNew, uow).ThrowIfFailed();
      
      // perform any related save operations (sinks, tag updates, etc...)
      var result = uow.ExecuteReturnsResultObject();

      if(!result.Success){
        this.Ets.Debug.FailFromResultMessages(result.Messages);
      }


      // execute final save
      return uow.ExecuteReturnsResultObject().ThrowIfFailed();
    }
  }
}
