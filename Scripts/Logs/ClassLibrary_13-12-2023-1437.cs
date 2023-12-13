using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using ETS.Core;
using ETS.Core.Api;
using ETS.Core.Api.Models.Data;
using ETS.Core.Enums;
using ETS.Core.Extensions;
using ETS.Core.Collections;

// Changes have been mate at 23/11/2023 - Pavel Ibrahim - New First Change

namespace ETS.Core.Scripting
{
  /// ******************************************************************
  /// <summary>Class Description</summary>
  /// ******************************************************************
  public static class ClassLibrary
  {
    public static ApiService _api = ETS.Core.Api.ApiService.GetInstance();
    /// <summary>
    /// Copies the properties and their values from an object to a dictionary with the property names as keys.
    /// </summary>
    /// <param name="obj">The object to copy properties from.</param>
    /// <param name="dict">The dictionary to copy properties and values to.</param>
    public static Dictionary<string, object> TransferPropertiesToDictionary(object obj)
    {
      var dict = new Dictionary<string, object>();
      //For every property in the config file: transfer them from Settings class to the correct key in Ets.Values.
      foreach (PropertyInfo prop in obj.GetType().GetProperties())
      {
        var value = prop.GetValue(obj);  //The value of the property
        string name = prop.Name;              //The name of the property

        dict[name] = value;        //Transfer value to Ets.
      }
      return dict;
    }
    /// <summary>
    /// Copies the properties and their values from an object to a dictionary with the property names as keys.
    /// </summary>
    /// <param name="obj">The object to copy properties from.</param>
    /// <param name="dict">The dictionary to copy properties and values to.</param>
    public static void TransferPropertiesToDictionary(object obj, Dictionary<string, object> dict)

    {
      //For every property in the config file: transfer them from Settings class to the correct key in Ets.Values.
      foreach (PropertyInfo prop in obj.GetType().GetProperties())
      {
        var value = prop.GetValue(obj);  //The value of the property
        string name = prop.Name;              //The name of the property

        dict[name] = value;        //Transfer value to Ets.
      }
    }
    /// <summary>
    /// Copies values from a dictionary to properties of an object with matching names.
    /// </summary>
    /// <param name="obj">The object to copy property values to.</param>
    /// <param name="prefix">A string to prepend to property names when looking for keys in the dictionary.</param>
    /// <param name="collection">The dictionary to copy values from.</param>
    public static void WriteValuesToProperties(object obj, string prefix, Dictionary<string, object> collection)

    {
      var uow = _api.CreateUnitOfWork();
      try
      {
        //For every property in the config file: transfer them from class to the correct key in Ets.Values.
        foreach (PropertyInfo prop in obj.GetType().GetProperties())
        {
          try
          {
            string name = prefix + prop.Name;              //The name of the property
            if (!collection.ContainsKey(name)) throw new NullReferenceException("The key " + name + " does not exist in Ets.Values.");
            var value = collection[name];    //The value of the property on the page
            var stringValue = Convert.ToString(value);
            var propValue = prop.GetValue(obj);

            if (prop.PropertyType == typeof(bool) && value is string)
            {
              bool boolValue;
              if (bool.TryParse(stringValue, out boolValue))
              {
                // Successfully parsed the string as a boolean value
                prop.SetValue(obj, boolValue);
              }
              else
              {
                // String value is not "True" or "False", handle the error as necessary
                throw new Exception("Invalid boolean value: " + stringValue);
              }
            }
            else
            {
              // Cast value
              var castValue = Convert.ChangeType(value, propValue.GetType());
              prop.SetValue(obj, castValue);
            }
            // _api.Util.Log.WriteInformation("" + name + " " + castValue, "Debug");
          }
          catch (Exception ex)
          {
            _api.Util.Log.WriteError(ex.Message, "Error");
          }

        }
      }
      catch (System.Exception ex)
      {
        _api.Util.Log.WriteError(ex.Message, "Error");
        //return false;
      }
      finally
      {
        var result = uow.ExecuteReturnsResultObject();

        if (!result.Success)
        {
          _api.Util.Log.WriteError(result.AsString(), "Error");
        }
      }
    }

    /// <summary>
    /// Populates an object's properties with values from a DataTable row where the column names match the property names.
    /// </summary>
    /// <param name="dataTable">The DataTable to read data from.</param>
    /// <param name="obj">The object to populate with data.</param>
    public static void ConvertDatatableToObject(DataTable dataTable, object obj)
    {
      //Store the columns
      string[] keys = dataTable.Columns.Cast<DataColumn>()
          .Select(column => column.Caption)
          .ToArray();
      //Store the values
      var values = dataTable.Rows[0].ItemArray;

      //Store in dictionary
      Dictionary<string, object> dataDict = keys.Zip(values, (k, v) => new { Key = k, Value = v })
          .ToDictionary(x => x.Key, x => x.Value);

      //Where keys = obj.prop, write the value
      foreach (PropertyInfo prop in obj.GetType().GetProperties())
      {
        try
        {
          string name = prop.Name;              //The name of the property
          var value = dataDict[name];
          var propValue = prop.GetValue(obj);

          //Check if value is DBNull
          if (value == DBNull.Value)
          {
            //Set the property value to null
            prop.SetValue(obj, null);
          }
          else if (prop.PropertyType == typeof(bool) && value is string)
          {
            // Convert "0" and "1" strings to boolean values
            bool boolValue = (value == "1");
            prop.SetValue(obj, boolValue);
          }
          else
          {
            //Cast value
            var castValue = Convert.ChangeType(value, propValue.GetType());
            prop.SetValue(obj, castValue);
          }
        }
        catch (Exception ex)
        {
          _api.Util.Log.WriteError(ex.Message, "Error");
        }
      }
      //return obj;
    }

    /// <summary>
    /// Copies an object's properties and their values to a dictionary with property names as keys.
    /// </summary>
    /// <param name="obj">The object to copy properties and values from.</param>
    /// <returns>A dictionary with property names as keys and property values as values.</returns>
    public static Dictionary<string, object> WriteObjectToSqlDictionary(object obj)

    {
      var dict = new Dictionary<string, object>();

      foreach (var prop in obj.GetType().GetProperties())
      {
        var value = prop.GetValue(obj);  //The value of the property
        var name = prop.Name;              //The name of the property

        dict.Add(name, value);
      }

      return dict;
    }
    /// <summary>
    /// Loops through an object's properties and returns two strings containing the property names and their values. The strings can be used in a SQL query.
    /// </summary>
    /// <param name="obj">The object to loop through.</param>
    /// <returns>An array containing two strings: one with property names and one with property values.</returns>
    public static string[] WriteObjectToSqlKeyValueStrings(object obj)

    {
      var props = obj.GetType().GetProperties();
      var keys = new string[props.Length];
      var values = new object[props.Length];

      for (var i = 0; i < props.Length; i++)
      {
        var value = props[i].GetValue(obj);  //The value of the property
        var name = props[i].Name;              //The name of the property
        keys[i] = name;
        values[i] = value;
      }
      var keysStr = string.Join(",", keys);
      var valuesStr = string.Join(",", values);
      var keysAndValues = new[] { keysStr, valuesStr };
      return keysAndValues;
    }
    /// <summary>
    /// Converts an object to a JSON string.
    /// </summary>
    /// <param name="obj">The object to convert to JSON.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public static string WriteObjectToJson(object obj)

    {
      var text = JsonConvert.SerializeObject(obj);
      return text;
    }
    /// <summary>
    /// Constructs an SQL INSERT query from a table name and a string array containing property names and their values.
    /// </summary>
    /// <param name="kvp">A string array containing property names and their values.</param>
    /// <param name="table">The name of the table to insert data into.</param>
    /// <returns>An SQL INSERT query string.</returns>
    public static string GetSqlInsertQuery(string[] kvp, string table)

    {
      string query = @"INSERT INTO " + table + "(" + kvp[0] + ") VALUES (" + kvp[1] + ")";
      return query;
    }
    public static string GetSqlUpdateQuery(Dictionary<string, object> dict, string table, string whereCondition)
    {
      string query = @"UPDATE " + table + " SET ";
      var lines = new List<string>();
      foreach (var kvp in dict)
      {
        var line = "" + kvp.Key + "=";

        line += kvp.Value.GetType() == typeof(string) ? ("'" + kvp.Value + "'") : Convert.ToString((Convert.ToInt16(kvp.Value)));
        lines.Add(line);
      }
      query += string.Join(",", lines.ToArray());
      query += " WHERE " + whereCondition;
      return query;
    }
  }

  public class DbMqttConfiguration
  {
    public string Name { get; set; }
    public string MqttHostname { get; set; }
    public int MqttPort { get; set; }
    public bool MqttUseTls { get; set; }
    public string MqttUsername { get; set; }
    public string MqttPassword { get; set; }
    public string MqttClientId { get; set; }
    public string MqttSpBNamespace { get; set; }
    public string MqttGroupId { get; set; }
    public string MqttNodeId { get; set; }
    public string MqttDeviceId { get; set; }
    public string LookupSetGroupSubscriptions { get; set; }
    public string LookupSetNodesDevicesSubscriptions { get; set; }
    public bool SettingEnableDiscovery { get; set; }
    public bool SettingEnableSqlPush { get; set; }
    public bool SettingEnableTimeLogging { get; set; }
    public string DbName { get; set; }
    public string DbHostname { get; set; }
    public string DbUsername { get; set; }
    public string DbPassword { get; set; }

    public DbMqttConfiguration()
    {
      // Initialize properties with default values
      Name = string.Empty;
      MqttHostname = string.Empty;
      MqttPort = 0;
      MqttUseTls = false;
      MqttUsername = string.Empty;
      MqttPassword = string.Empty;
      MqttClientId = string.Empty;
      MqttSpBNamespace = string.Empty;
      MqttGroupId = string.Empty;
      MqttNodeId = string.Empty;
      MqttDeviceId = string.Empty;
      LookupSetGroupSubscriptions = string.Empty;
      LookupSetNodesDevicesSubscriptions = string.Empty;
      SettingEnableDiscovery = false;
      SettingEnableSqlPush = false;
      SettingEnableTimeLogging = false;
      DbName = string.Empty;
      DbHostname = string.Empty;
      DbUsername = string.Empty;
      DbPassword = string.Empty;
    }
  }
}