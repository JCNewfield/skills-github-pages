using System;
using System.Web;
using System.Data;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using ETS.Core;
using ETS.Core.Api;
using ETS.Core.Api.Models.Data;
using ETS.Core.Enums;
using ETS.Core.Extensions;
using Newtonsoft.Json.Linq;

// Changes have been made at 7/11/2023.
// New changes have been made from TrakSYS

// Changes have been made at 15/11/2023 - Saevar Valdimarsson - First change
// Changes have been made at 15/11/2023 - Saevar Valdimarsson - Second change
// Changes have been made at 15/11/2023 - Saevar Valdimarsson - Third change

// Changes have been made at 16/11/2023 - Pavel Ibrahim - First Change
// Changes have been made at 16/11/2023 - Pavel Ibrahim - Second Change
// Changes have been made at 16/11/2023 - Pavel Ibrahim - Third Change
// Changes have been made at 16/11/2023 - Pavel Ibrahim - Fourth Change

namespace ETS.Core.Scripting
{
  /// ******************************************************************
  /// <summary>
  /// This class contains code that can be executed by a Logic Service
  /// instance that is attached using the Script Class Name setting.
  /// </summary>
  /// ******************************************************************
  public class paveltt : ETS.Core.Scripting.LogicManagerScriptClassBase
  {

    // These are the values in the tags value
    // The value of the tags is in a JSON String, which means that we need to split the string
    // There are three values, however, sId and sType is important, sState will always be the same in our case
    // sId will be used as part of the RFID
    // sType will be used as the value to recognize what raw material to assign to the Item when it is created
    public string sId;
    public string sType;
    public string sState;

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

      // Subscribe to the tags that represent the raw materials in the raw material storage
      // Whenever one of these tags is updated this function will trigger which will then run PostScanTagChanged
      context.SubscribeToTagByTagID(535);
      context.SubscribeToTagByTagID(536);
      context.SubscribeToTagByTagID(538);
      context.SubscribeToTagByTagID(520);
      context.SubscribeToTagByTagID(498);
      context.SubscribeToTagByTagID(490);
      context.SubscribeToTagByTagID(548);
      context.SubscribeToTagByTagID(489);
      context.SubscribeToTagByTagID(573);

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

    // This Function extracts a string which represents a JSON string
    private void ProcessJson(string json)
    {
      JObject jsonObject = JObject.Parse(json);

      // It will extract each value and assign them to the variables in the class
      sId = jsonObject["s_id"].ToString();
      sType = jsonObject["s_type"].ToString();
      sState = jsonObject["s_state"].ToString();
    }

    // This function is to get the last 3 characters in a string
    string GetLastThreeCharacters(string input)
    {
      if (input.Length >= 3)
      {
        return input.Substring(input.Length - 3);
      }
      else
      {
        return input;
      }
    }

    // This converts the name of the tag to a location on the minifactory
    int MapLocationCodeToId(string locationCode)
    {
      switch (locationCode)
      {
        case "0_0":
          return 7;  // Location A1
        case "0_1":
          return 8;  // Location A2
        case "0_2":
          return 9;  // Location A3
        case "1_0":
          return 10; // Location B1
        case "1_1":
          return 11; // Location B2
        case "1_2":
          return 12; // Location B3
        case "2_0":
          return 13; // Location C1
        case "2_1":
          return 14; // Location C2
        case "2_2":
          return 15; // Location C3
        default:
          return -1; // Unknown location
      }
    }

    int BrickToRawMaterialID(string value)
    {

      // Different colors of the bricks in the Minifactory
      const string red = "RED";
      const string blue = "BLUE";
      const string white = "WHITE";

      // Different Raw Material IDs
      int raw_Salmon = 3; // RED
      int raw_Trout = 1; // BLUE
      int raw_Cod = 2; // WHITE

      // This will check the color and return the id of the Raw Material
      // This is used in the process of creating an Item
      switch (value)
      {
        case red:
          return raw_Salmon;
        case blue:
          return raw_Trout;
        case white:
          return raw_Cod;
        default:
          return -1;
      }
    }

    // Generates a random number between 1 and 10
    int GenerateRandomNumber()
    {
      Random random = new Random();
      return random.Next(1, 11);
    }

    /// ******************************************************************
    /// The following method is triggered if a subscribed tag's value was 
    /// changed in the preceding scan.  If triggered, this method is called
    /// at the end of the Logic Service scan.  To trigger this method, subscribe
    /// to tags in LoadAndInitialize.
    /// ******************************************************************
    public override void PostScanTagChanged(IPostScanTagChangedContext context)
    {

      // Get instance of api service
      var api = ETS.Core.Api.ApiService.GetInstance();

      // Getting date locally from computer
      DateTime datetime = DateTime.Now;

      // Get the list of changed tags which we subscribed to in the "LoadAndInitialize" function
      List<ISourceCodeTag> tags = context.ChangedTags.Tags.ToList();

      // Create a model which we are going to populate and then add to the database
      Api.Models.Data.DbItem item = new Api.Models.Data.DbItem();

      foreach (ISourceCodeTag tag in tags)
      {

        // Get the value of the changed tag: 
        string tagValue = tag.ValueString;
        string tagLocation = tag.Name;

        Api.Util.Log.WriteInformation(tagLocation, "debug");

        ProcessJson(tagValue);

        int itemDefinitionID = 1;

        // If the id is changed and it is 0, it will leave the loop and end the code. 
        // It is not going to populate a model and save it to the db if the id is 0, because that only means it was removed from the
        // raw material storage.
        if (sId == "0") { break; }

        else
        {

          // Setting ItemDefinitionID to 1 because all the raw materials are in pallets.
          item.ItemDefinitionID = itemDefinitionID;

          // Setting MaterialID based on the sType value in the tag
          item.MaterialID = BrickToRawMaterialID(sType);

          // Set location id by converting the last 3 characters of the tag name to a location in the minifactory
          string location = GetLastThreeCharacters(tagLocation);
          item.LocationID = MapLocationCodeToId(location);

          // This is the RFID or the id of the tag 
          item.Attribute01 = sId;

          // This is the expiration date of the raw material, this will be a constant value. 
          item.Attribute02 = "10";

          // Setting lot number for the item
          item.Lot = "Lot_" + datetime.ToString("dd-MM-yyyy-HH:mm:ss");

          // Converting expiration date to an int
          int expDate = Int32.Parse(item.Attribute02);

          // Setting quantity to a random value so it does not look constant
          item.Quantity = GenerateRandomNumber();

          // Setting valid from date
          item.ValidFromDateTime = datetime;

          // Setting valid to the date plus the expiration date
          item.ValidToDateTime = DateTime.Now.AddDays(expDate);

          // Setting UniqueID to I_ + the RFID/ID of the tag
          item.UniqueID = "I_" + sId;

          Api.Models.Result<ETS.Core.Api.Models.Data.DbItem> result;

          ////// Code below this line is the ItemLog part of the implementation

          // Here we insert the entity in the database (The ID will be created when inserted to the database)
          result = api.Data.DbItem.Save.InsertAsNew(item);

          if (result.Success)
          {
            int newID = result.Return.ID;
            var _itemID = this.Api.Util.Db.ExecuteScalar<int>(@"SELECT Value FROM tSequence WHERE Name = 'ItemID'").Return;
            Api.Models.Data.DbItemLog itemLog = new Api.Models.Data.DbItemLog();

            itemLog.ItemLogDefinitionID = 1;
            itemLog.ItemID = _itemID;
            itemLog.LogDateTime = datetime;
            itemLog.Lot = "Lot_" + datetime.ToString("dd-MM-yyyy-HH:mm:ss");
            itemLog.MaterialID = BrickToRawMaterialID(sType);
            itemLog.Quantity = 1;
            itemLog.LocationID = MapLocationCodeToId(location);
            itemLog.Notes = "Recived to HBW storage";
            this.Api.Data.DbItemLog.Save.ValidateIgnoredForInsertAsNew(itemLog);
          }
          else
          {
            return;
          }
        }

      }

    }
  }
}
