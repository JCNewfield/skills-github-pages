using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using System.Web.UI.DataVisualization.Charting;
using System.Diagnostics;
using System.Security;
using System.Text;
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
using Newtonsoft.Json;


namespace ETS.Ts.Content
{

  public class DbMqttConfiguration
  {
    // The hostname of the MQTT broker to connect to
    public string MqttHostname { get; set; }

    // The port number of the MQTT broker to connect to
    public int MqttPort { get; set; }

    // Whether or not to use TLS encryption for the MQTT connection
    public bool MqttUseTls { get; set; }

    // The username to use when connecting to the MQTT broker
    public string MqttUsername { get; set; }

    // The password to use when connecting to the MQTT broker
    public string MqttPassword { get; set; }

    // The MQTT client ID to use when connecting to the broker
    public string MqttClientId { get; set; }

    // The Sparkplug B namespace to use for MQTT topics
    public string MqttSpBNamespace { get; set; }

    // The group ID for this client (if any)
    public string MqttGroupId { get; set; }

    // The node ID for this client (if any)
    public string MqttNodeId { get; set; }

    // The device ID for this client (if any)
    public string MqttDeviceId { get; set; }

    // The timout timespan in seconds
    public int MqttTimeout { get; set; } = 30;

    // The SparkplugB Node Abbreviation to be used. i.e "_MES"
    public string MqttSparkplugNodeAbbreviation { get; set; }

    // Limits how often the node can respond to a birth command. Null/0 = no limit
    public int MqttSparkplugBirthReplyLimitSeconds { get; set; }

    // Limits how often the application can send a birth command. Null/0 = no limit
    public int MqttSparkplugBirthCommandLimitSeconds { get; set; }

    // Whether or not to automatically subscribe to group-level topics
    public string LookupSetGroupSubscriptions { get; set; }

    // Whether or not to automatically subscribe to node and device-level topics
    public string LookupSetNodesDevicesSubscriptions { get; set; }

    // Whether or not to enable discovery of other Sparkplug B nodes on the network
    public bool SettingEnableDiscovery { get; set; }

    // Whether or not to enable pushing data to an SQL database
    public bool SettingEnableSqlPush { get; set; }

    // Whether or not to enable time logging of messages received from the MQTT broker
    public bool SettingEnableTimeLogging { get; set; }

    // Whether or not to enable parsing of outgoing names as SparkplugB. This will append the 
    public bool SettingParseOutgoingSparkplugB { get; set; }

    // Whether or not to enable parsing of outgoing names as ISA95
    public bool SettingParseIncomingIsa95 { get; set; }

    // Can be used if logs should be placed somewhere specific
    public string SettingLogsFolder { get; set; }

    // Indicates what Tag Type the producer should filter on when publishing tags.
    public int TraksysSourceTagTypeId { get; set; }

    // Indicates the target Tag Group where the consumer should place incoming tags.
    public int TraksysTargetTagGroupId { get; set; }

    // The name of the SQL database to push data to (if applicable)
    public string DbName { get; set; }

    // The hostname of the SQL database server (if applicable)
    public string DbHostname { get; set; }

    // The username to use when connecting to the SQL database (if applicable)
    public string DbUsername { get; set; }

    // The password to use when connecting to the SQL database (if applicable)
    public string DbPassword { get; set; }

    // Constructor that initializes all properties to their default values
    public DbMqttConfiguration()
    {
      MqttHostname = "Hostname";
      MqttPort = 1883;
      MqttUseTls = false;
      MqttUsername = "Username";
      MqttPassword = "Password";
      MqttClientId = "ClientId";
      MqttSpBNamespace = "Namespace";
      MqttGroupId = "GroupId";
      MqttNodeId = "NodeId";
      MqttDeviceId = "DeviceId";
      MqttTimeout = 30;
      MqttSparkplugNodeAbbreviation = "_MES";
      MqttSparkplugBirthReplyLimitSeconds = 0;
      MqttSparkplugBirthCommandLimitSeconds = 0;
      LookupSetGroupSubscriptions = string.Empty;
      LookupSetNodesDevicesSubscriptions = string.Empty;
      SettingEnableDiscovery = false;
      SettingEnableSqlPush = false;
      SettingEnableTimeLogging = false;
      SettingParseOutgoingSparkplugB = false;
      SettingParseIncomingIsa95 = false;
      SettingLogsFolder = string.Empty;
      TraksysSourceTagTypeId = 0;
      TraksysTargetTagGroupId = 0;
      DbName = string.Empty;
      DbHostname = "Hostname";
      DbUsername = "Username";
      DbPassword = "Password";
    }
  }
  public class TsProcessStatus
  {
    public Process Process { get; set; }
    public string PathProcess { get; set; }
    public string Status
    {
      get
      {
        if (this.Process == null || this.Process.HasExited)
        {
          return "Not running";
        }
        else
        {
          return "Running";
        }
      }
    }

    public string Color
    {
      get
      {
        if (this.Status == "Not running")
        {
          return "tscolor-inactive";
        }
        else
        {
          return "tscolor-success";
        }
      }
    }

    public TsProcessStatus(string processName, string processPath)
    {
      Process = FindProcessByName(processName);
      PathProcess = processPath;
    }
    public Process FindProcessByName(string processName)
    {
      Process[] processes = Process.GetProcessesByName(processName);
      if (processes.Length > 0)
      {
        // Return the first process with matching name
        return processes[0];
      }
      else
      {
        // Process not found
        return null;
      }
    }
  }


  /// ***********************************************************
  public partial class MQTTService : ContentPageBase
  {
    /// Automatically map the decorated property to incoming Ets.Values data (typically QueryString variables).
    /// The name of the property is used as the Key in accessing the Ets.Values collection during mapping.
    //[ValuesProperty()]
    //public int SystemID { get; set; } = -1;
    public string TotalTableName = "";
    public string TableName = "";
    public DbMqttConfiguration Settings = new DbMqttConfiguration();
    TsProcessStatus processProducer;
    TsProcessStatus processConsumer;
    public const string MqttConfigTable = "_tMqttClientConfig";
    public const string PathProducerApplication = @"C:\Program Files (x86)\Parsec\TrakSYS\MQTT\Producer\bin\Debug\net6.0\Producer.exe";
    public const string PathConsumerApplication = @"C:\Program Files (x86)\Parsec\TrakSYS\MQTT\Consumer\bin\Debug\net6.0\Consumer.exe";
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

      TotalTableName = Settings.DbName == string.Empty ? $"[{this.Ets.Api.Util.Db.DatabaseName}].[dbo].[{MqttConfigTable}]" : $"[{Settings.DbName}].[dbo].[{MqttConfigTable}]";
      TableName = $"dbo.[{MqttConfigTable}]";
      var tableExits = this.Ets.Api.Util.Db.ExecuteSql($@"SELECT COUNT(*) FROM {TotalTableName}").Return;
      if (!tableExits)
      {
        this.Ets.Api.Util.Log.WriteInformation($"{TotalTableName} does not exist in database. Creating new table.", "Info");
        var createTableSql = HelperMethods.SqlQueryBuilder.GenerateCreateTableQuery<DbMqttConfiguration>(MqttConfigTable);
        this.Ets.Api.Util.Db.ExecuteSql(createTableSql);
      }
      DataTable dtMqttConfig = this.Ets.Api.Util.Db.GetDataTable($@"SELECT TOP (1) * FROM {TotalTableName}").Return.ThrowIfLoadFailed("Couldn't select the configuration table.", null);
      if (dtMqttConfig.Rows.Count == 0)
      {
        Settings.DbName = this.Ets.Api.Util.Db.DatabaseName;
        var sql = HelperMethods.SqlQueryBuilder.GenerateInsertQuery(Settings, $"{TotalTableName}");
        this.Ets.Api.Util.Db.ExecuteSql(sql).ThrowIfFailed("No existing configuration. Unable to insert a new row. Check that the query is correct: {0}".FormatWith(sql));
      }
      //TransferToEts("Data.MqttConfiguration.", Settings);
      //Cast the datatable to the object
      HelperMethods.ConvertDatatableToObject(dtMqttConfig, Settings);
      return true;
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
    protected override bool ContentPage_PartPreInit02()
    {
      return true;
    }

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
    protected override bool ContentPage_PartPreInit05()
    {
      //Get the values from the SQL query, which lies in ETS Values.
      //ClassLibrary.WriteValuesToProperties(Settings, "Data.MqttConfiguration.", this.Ets.Values.ToDictionary(p => p.Key, p => p.Value));

      //Get the datatable from ETS Values
      // DataTable dtMqttConfig = (DataTable)this.Ets.Values.Get("Data.MqttConfiguration", null)
      //     .ThrowIfNull("No MQTT Configuration defined in Values: [Data.MqttConfiguration]. Check SQL query.");


      

      //Transfer the properties of the object, to ETS Values
      TransferToEts("Data.MqttConfiguration.", Settings);

      // DataTable dtMqttGroups = (DataTable)this.Ets.Values.Get("Data.MqttConfiguration.DtGroups", null);
      // DataTable dtMqttNodesDevices = (DataTable)this.Ets.Values.Get("Data.MqttConfiguration.DtNodesDevices", null);

      // IEnumerable<DataRow> rowsMqttGroups = dtMqttGroups.Rows.Cast<DataRow>();
      // IEnumerable<DataRow> rowsMqttNodesDevices = dtMqttNodesDevices.Rows.Cast<DataRow>();

      // var groupsString = string.Join(", ", rowsMqttGroups.Select(r => r.GetString("Value")));
      // var nodesDevicesString = string.Join(", ", rowsMqttNodesDevices.Select(r => r.GetString("Value")));
      // Ets.Values["Data.MqttConfiguration.LookupSetGroupSubscriptions"] = groupsString;
      // Ets.Values["Data.MqttConfiguration.LookupSetNodesDevicesSubscriptions"] = nodesDevicesString;
      return true;
    }

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
    protected override bool ContentPage_PartPreInit10()
    {
      processProducer = new TsProcessStatus("Producer", PathProducerApplication);
      processConsumer = new TsProcessStatus("Consumer", PathConsumerApplication);

      TransferToEts(nameof(processProducer) + ".", processProducer);
      TransferToEts(nameof(processConsumer) + ".", processConsumer);

      return true;
    }

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
    protected override bool ContentPage_Final()
    {
      return true;
    }

    /// <summary>
    /// This method will transfer values from an object, to ETS Values, with their corresponding key names
    /// </summary>
    public void TransferToEts(string prefix, object obj)
    {
      //For every property in the config file: transfer them from Settings class to the correct key in Ets.Values.
      foreach (PropertyInfo prop in obj.GetType().GetProperties())
      {
        var value = prop.GetValue(obj);  //The value of the property
        string name = prop.Name;              //The name of the property

        this.Ets.Values[prefix + name] = value;        //Transfer value to Ets.
      }
    }
    // public void getMqttParametersFromFile(string filepath)
    // {
    //   try
    //   {
    //     var text = File.ReadAllText(filepath).ThrowIfLoadFailed("Couldn't find the configuration file.", filepath);
    //     MqttConfig Settings = JsonConvert.DeserializeObject<MqttConfig>(text).ThrowIfLoadFailed("Couldn't deserialize the configuration file.", filepath);

    //     //For every property in the config file: transfer them from Settings class to the correct key in Ets.Values.
    //     TransferToEts(Settings);
    //   }
    //   catch (System.Exception ex)
    //   {
    //     this.Ets.Api.Util.Log.WriteError(ex.Message, "Error");
    //   }
    // }
    private void btnAddGroup(object sender, EventArgs e)
    {
      this.Ets.Pages.RedirectToUrl("/TS/pages/home/config/misc/lookupsets/?S2ID=10&S3Key=Item.LookupValue&S3ID=10&S2Key=List.LookupSet.LookupValues&S2ParentID=2&S1ID=2");
    }
    private void btnRunProducer(object sender, EventArgs e)
    {
      runProcess(processProducer);
      this.Ets.Pages.RedirectToSelf();
    }
    private void btnStopProducer(object sender, EventArgs e)
    {
      stopProcess(processProducer);
      this.Ets.Pages.RedirectToSelf();
    }
    private void btnRunConsumer(object sender, EventArgs e)
    {
      runProcess(processConsumer);
      this.Ets.Pages.RedirectToSelf();
    }
    private void btnStopConsumer(object sender, EventArgs e)
    {
      stopProcess(processConsumer);
      this.Ets.Pages.RedirectToSelf();
    }

    private void btnNodeDevice(object sender, EventArgs e)
    {

    }
    protected void btnSave(object sender, EventArgs e)
    {
      try
      {
        Ets.Values.Set("Data.MqttConfiguration.DbPassword", Password_DbPassword.InputValue, "Debug");

        HelperMethods.WriteValuesToProperties(Settings, "Data.MqttConfiguration.", this.Ets.Values.ToDictionary(p => p.Key, p => p.Value));
        //var kvp = ClassLibrary.WriteObjectToSqlDictionary(Settings);

        string sql = HelperMethods.SqlQueryBuilder.GenerateUpdateQuery(Settings, TotalTableName);
        //ClassLibrary.GetSqlUpdateQuery(kvp, MqttConfigTable, "");
        this.Ets.Api.Util.Db.ExecuteSql(sql).ThrowIfLoadFailed("Update query to " + MqttConfigTable + " failed.\nQuery: " + sql, sql);
        this.Ets.Api.Util.Log.WriteInformation(sql, "Debug");
      }
      catch (Exception ex)
      {
        this.Ets.Api.Util.Log.WriteError(ex.Message, "Error");
      }
    }
    private void runProcess(TsProcessStatus _process)
    {
      if (_process.Process == null || _process.Process.HasExited)
      {
        _process.Process = new Process();
        _process.Process.StartInfo.FileName = _process.PathProcess;
        _process.Process.StartInfo.UseShellExecute = false;
        _process.Process.StartInfo.RedirectStandardOutput = true;
        // _process.Process.StartInfo.UserName = "goodtech";
        // SecureString password = new SecureString();
        // string passwordString = "rjnq79pnz4ytqq!"; // Set the password here
        // foreach (char c in passwordString)
        // {
        //   password.AppendChar(c);
        // }
        // _process.Process.StartInfo.Password = password;


        bool success = _process.Process.Start();
        if (!success)
        {
          this.Ets.Api.Util.Log.WriteError("Application not started.", "Error");
        }
        else this.Ets.Api.Util.Log.WriteInformation("Application started.", "Info");
      }
    }
    private void stopProcess(TsProcessStatus _process)
    {
      if (_process.Process != null && !_process.Process.HasExited)
      {
        _process.Process.Kill();
      }
    }
    private void restartProcess(TsProcessStatus _process)
    {
      stopProcess(_process);
      runProcess(_process);
    }
  }
  public static class HelperMethods
  {
    public static ApiService _api = ETS.Core.Api.ApiService.GetInstance();
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
            if (!collection.ContainsKey(name)) {
              throw new NullReferenceException("The key " + name + " does not exist in Ets.Values.");
              _api.Util.Log.WriteError("The key " + name + " does not exist in Ets.Values.", "Error");
            }
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
    public static string GenerateInsertQuery(object data, string tableName)
    {
      Type dataType = data.GetType();
      PropertyInfo[] properties = dataType.GetProperties();

      StringBuilder queryBuilder = new StringBuilder();
      StringBuilder columnsBuilder = new StringBuilder();
      StringBuilder valuesBuilder = new StringBuilder();

      foreach (PropertyInfo property in properties)
      {
        string columnName = property.Name;
        object columnValue = property.GetValue(data);

        if (columnsBuilder.Length > 0)
        {
          columnsBuilder.Append(", ");
          valuesBuilder.Append(", ");
        }

        columnsBuilder.Append(columnName);

        if (columnValue == null)
        {
          valuesBuilder.Append("NULL");
        }
        else if (property.PropertyType == typeof(string) || property.PropertyType == typeof(DateTime))
        {
          valuesBuilder.Append("'" + columnValue.ToString().Replace("'", "''") + "'");
        }
        else
        {
          valuesBuilder.Append(columnValue);
        }
      }

      queryBuilder.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2})", tableName, columnsBuilder.ToString(), valuesBuilder.ToString());

      return queryBuilder.ToString();
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
          string name = prop.Name;                  //The name of the property
          if (!dataDict.ContainsKey(name)) continue; //Skip if key isn't present in the dict
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



    public static class SqlQueryBuilder
    {
      public static string GenerateCreateTableQuery<T>(string tableName)
      {
        Type dataType = typeof(T);
        PropertyInfo[] properties = dataType.GetProperties();

        StringBuilder queryBuilder = new StringBuilder();
        queryBuilder.AppendFormat("CREATE TABLE {0} (", tableName);

        foreach (PropertyInfo property in properties)
        {
          string columnName = property.Name;
          string dataTypeString = GetSqlDataType(property.PropertyType);

          queryBuilder.AppendFormat("{0} {1}, ", columnName, dataTypeString);
        }

        // Remove the trailing comma and space
        queryBuilder.Length -= 2;
        queryBuilder.Append(")");

        return queryBuilder.ToString();
      }
      public static string GenerateUpdateQuery<T>(T data, string tableName)
    {
      Type dataType = typeof(T);
      PropertyInfo[] properties = dataType.GetProperties();

      StringBuilder queryBuilder = new StringBuilder();
      queryBuilder.AppendFormat("UPDATE {0} SET ", tableName);

      foreach (PropertyInfo property in properties)
      {
        string columnName = property.Name;
        object columnValue = property.GetValue(data);

        if (queryBuilder.Length > (14 + tableName.Length))
        {
          queryBuilder.Append(", ");
        }

        queryBuilder.AppendFormat("{0} = {1}", columnName, FormatValueForUpdate(columnValue));
      }

      return queryBuilder.ToString();
    }

    private static string FormatValueForUpdate(object value)
    {
      if (value == null)
      {
        return "NULL";
      }
      else if (value is bool boolValue)
      {
        return boolValue ? "1" : "0";
      }
      else if (value is string || value is DateTime)
      {
        return $"'{EscapeValue(value.ToString())}'";
      }
      else
      {
        return value.ToString();
      }
    }

      public static string GenerateInsertQuery<T>(T data, string tableName)
      {
        Type dataType = typeof(T);
        PropertyInfo[] properties = dataType.GetProperties();

        StringBuilder queryBuilder = new StringBuilder();
        StringBuilder columnsBuilder = new StringBuilder();
        StringBuilder valuesBuilder = new StringBuilder();

        foreach (PropertyInfo property in properties)
        {
          string columnName = property.Name;
          object columnValue = property.GetValue(data);

          if (columnsBuilder.Length > 0)
          {
            columnsBuilder.Append(", ");
            valuesBuilder.Append(", ");
          }

          columnsBuilder.Append(columnName);

          string formattedValue;
          if (columnValue == null)
          {
            formattedValue = "NULL";
          }
          else if (property.PropertyType == typeof(string) || property.PropertyType == typeof(DateTime))
          {
            formattedValue = $"'{EscapeValue(columnValue.ToString())}'";
          }
          else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(System.Boolean) || property.PropertyType == typeof(ETS.Core.Enums.Boolean))
          {
            formattedValue = ((bool)columnValue) ? "1" : "0";
          }
          else
          {
            formattedValue = columnValue.ToString();
          }

          valuesBuilder.Append(formattedValue);
        }

        queryBuilder.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2})", tableName, columnsBuilder.ToString(), valuesBuilder.ToString());

        return queryBuilder.ToString();
      }

      private static string EscapeValue(string value)
      {
        // Escape single quotes by doubling them
        return value.Replace("'", "''");
      }

      private static string GetSqlDataType(Type propertyType)
      {
        if (propertyType == typeof(string))
        {
          return "VARCHAR(255)";
        }
        else if (propertyType == typeof(int))
        {
          return "INT";
        }
        else if (propertyType == typeof(decimal))
        {
          return "DECIMAL(18, 2)";
        }
        else if (propertyType == typeof(DateTime))
        {
          return "DATETIME";
        }
        else if (propertyType == typeof(bool) || propertyType == typeof(System.Boolean) || propertyType == typeof(ETS.Core.Enums.Boolean))
        {
          return "BIT";
        }
        // Add more data type mappings as needed

        // Default to TEXT for unknown types
        return "TEXT";
      }
    }
  }
}