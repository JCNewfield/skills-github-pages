using System;
using System.Data;
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
  /// <summary>Class Description</summary>
  /// ******************************************************************
  public class CustomerAddress
  {
    /// ******************************************************************
    /// <summary>
    /// Method Description
    /// </summary>
    /// ******************************************************************
    public void DoSomething(ApiService api)
    {
    }
    public DataTable GetCustomerGroup(ApiService api, int customerGroupID)
    {
      string customerSql = @"
      SELECT cg.[ID]
      , cg.[name]
      , cg.[address]
      , cg.[email]
      , cg.[phonenumber]
      FROM _customer cg
      WHERE cg.ID = {0}
      ".FormatWith(customerGroupID);
      return api.Util.Db.GetDataTable(customerSql).ThrowIfFailed();
    }

    public bool UpdateCustomerGroup(ApiService api, int customerGroupID, string name, string address, string email, string phonenumber)
    {
      string updateCustomerSql = @"
      UPDATE _customer
      SET name = '{1}'
      , address = '{2}'
      , email = '{3}'
      , phonenumber = '{4}'
      WHERE ID = {0};
      ".FormatWith(customerGroupID, name, address,email,phonenumber);
      return api.Util.Db.ExecuteSql(updateCustomerSql).ThrowIfFailed();
    }
  }
}