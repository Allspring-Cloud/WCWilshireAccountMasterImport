/*
 * <AccountMasterDAO.cs>
 *  $Date$
 *  $Revision$
 *  $Author$
 *  $HeadURL$
 *  $Id$
 *
 * Copyright (c) 2012 Wells Capital Management, Inc.
 * All rights reserved.
 *
 * This software is the confidential and proprietary information
 * of Wells Capital Management. ("Confidential Information").  You
 * shall not disclose such Confidential Information and shall use
 * it only in accordance with the terms of the license agreement
 * you entered into with Wells Capital Management.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;


// Wells Cap
using DatabaseCommonUtils;

// Project Level
using WilshireAccountMasterImport.Records;
using LoggerWrapperServices.Services;

namespace WilshireAccountMasterImport.DataAccess
{
    /// <summary>
    /// This class performs data access
    /// </summary>
    class AccountMasterDAO
    {
        #region Fields

        private static readonly LoggingService log = new LoggingService(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DbConnection dbConn;
        private DBProviderFactory.ProviderName wcmProviderName = DBProviderFactory.ProviderName.SqlClientDataProvider;

        #endregion

        #region Constructor

        public AccountMasterDAO(string connectionString)
        {
            dbConn = DBConnectionUtil.GetConnection(wcmProviderName, connectionString);
        }

        #endregion

        #region Methods

        public void LoadRecord(AccountMasterRecord record)
        {
            if (RecordExists(record))
            {
                UpdateRecord(record);
            }
            else
            {
                InsertRecord(record);
            }
        }

        private bool RecordExists(AccountMasterRecord record)
        {
            bool returnValue;

            string commandStr = "select * from PUB_WFAM_INV_COMPOSITE.QRP.stg_account_master where account_code = @account_code";

            List<DBParameterUtil.ParamInfo> lstParams = new List<DBParameterUtil.ParamInfo>();

            lstParams.Add(DBParameterUtil.BuildNullableParam(typeof(string), "@account_code",
                TranslateNullValue(DbType.String, record.Values[Constants.POSITION_ACCOUNTCODE])));

            using (DbDataReader reader = FillDataReader(dbConn, commandStr, lstParams))
            {
                returnValue = reader.HasRows;
            }

            return returnValue;
        }

        private void InsertRecord(AccountMasterRecord record)
        {
            string commandStr = "INSERT INTO PUB_WFAM_INV_COMPOSITE.QRP.stg_account_master " +
                                "(account_code, account_name, strategy, performance_start_date) " +
                                "VALUES ( @account_code, @account_name, @strategy, @performance_start_date)";

            List<DBParameterUtil.ParamInfo> lstParams = new List<DBParameterUtil.ParamInfo>();

            lstParams.Add(DBParameterUtil.BuildNullableParam(typeof(string), "@account_code",
                TranslateNullValue(DbType.String, record.Values[AccountMasterFields.AccountCode])));

            lstParams.Add(DBParameterUtil.BuildNullableParam(typeof(string), "@account_name",
                TranslateNullValue(DbType.String, record.Values[AccountMasterFields.AccountName])));

            lstParams.Add(DBParameterUtil.BuildNullableParam(typeof(string), "@strategy",
                TranslateNullValue(DbType.String, record.Values[AccountMasterFields.Strategy])));

            lstParams.Add(DBParameterUtil.BuildNullableParam(typeof(DateTime), "@performance_start_date",
                TranslateNullValue(DbType.DateTime, record.Values[AccountMasterFields.PerformanceStartDate])));

            ExecuteNonQuery(wcmProviderName, dbConn.ConnectionString, commandStr, lstParams);
        }

        private void UpdateRecord(AccountMasterRecord record)
        {
            string commandStr = "UPDATE PUB_WFAM_INV_COMPOSITE.QRP.stg_account_master " +
                                "SET account_name = @account_name, strategy = @strategy, performance_start_date = @performance_start_date " +
                                "WHERE account_code = @account_code";

            List<DBParameterUtil.ParamInfo> lstParams = new List<DBParameterUtil.ParamInfo>();

            lstParams.Add(DBParameterUtil.BuildNullableParam(typeof(string), "@account_code",
                TranslateNullValue(DbType.String, record.Values[AccountMasterFields.AccountCode])));

            lstParams.Add(DBParameterUtil.BuildNullableParam(typeof(string), "@account_name",
                TranslateNullValue(DbType.String, record.Values[AccountMasterFields.AccountName])));

            lstParams.Add(DBParameterUtil.BuildNullableParam(typeof(string), "@strategy",
                TranslateNullValue(DbType.String, record.Values[AccountMasterFields.Strategy])));

            lstParams.Add(DBParameterUtil.BuildNullableParam(typeof(DateTime), "@performance_start_date",
                TranslateNullValue(DbType.DateTime, record.Values[AccountMasterFields.PerformanceStartDate])));

            ExecuteNonQuery(wcmProviderName, dbConn.ConnectionString, commandStr, lstParams);
        }

        #endregion Methods

        #region Helper Methods

        /* helper methods with error handling */

        private int ExecuteNonQuery(DBProviderFactory.ProviderName providerName, string connString, string qryString, List<DBParameterUtil.ParamInfo> lstParams)
        {
            int result;
            
            DbCommand myCommand = dbConn.CreateCommand();
            try
            {
                result = SqlQuery.ExecuteNonQuery(providerName, myCommand, connString, qryString, lstParams);
            }
            catch (DbException dbEx)
            {
                log.Error(
                    "DB Exception getting user query information: " + dbEx.Message + ". PROBLEM SQL WAS: " +
                    myCommand.FetchCommandTextWithParameters(), dbEx);
                throw new Exception("DB exception getting user query information", dbEx);
            }
            catch (Exception ex)
            {
                log.Error(
                    "General Exception getting user query information: " + ex.Message + ". PROBLEM SQL WAS: " +
                    myCommand.FetchCommandTextWithParameters(), ex);
                throw new Exception("General exception getting user query information", ex);
            }
            return result;
        }

        private DbDataReader FillDataReader(DbConnection conn, string qryString, List<DBParameterUtil.ParamInfo> lstParams)
        {
            DbDataReader myReader = null;
            DbCommand myCommand = conn.CreateCommand();

            try
            {
                myReader = SqlQuery.FillDataReader(conn, myCommand, qryString, lstParams);
            }
            catch (DbException dbEx)
            {
                log.Error("DB Exception getting user query information: " + dbEx.Message + ". PROBLEM SQL WAS: " +
                myCommand.FetchCommandTextWithParameters(), dbEx);
                throw new Exception("DB exception getting user query information", dbEx);
            }
            catch (Exception ex)
            {
                log.Error("General Exception getting user query information: " + ex.Message + ". PROBLEM SQL WAS: " +
                myCommand.FetchCommandTextWithParameters(), ex);
                throw new Exception("General exception getting user query information", ex);
            }

            return myReader;
        }

        // TODO need to convert/remove this once parser/columnInfo is equipped to properly deal with blank/empty values
        /// <summary>
        /// Applies rules for parsed data in application regarding treating special cases as DBNull.Value
        /// The main purpose of this method is to handle special cases where spaces or empty strings are present.
        /// We are not validating the type, only determining if an empty string should be considered a null for the type of variable
        /// The future plan is to add this logic to the column infos as a setting using an enmerator to describe how to treat each column
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object TranslateNullValue(DbType type, object value)
        {
            object returnValue = DBNull.Value;

            // nulls are handled fine by the parameter method
            if (value != null)
            {
                switch (type)
                {
                    case DbType.DateTime:
                    case DbType.DateTime2:
                    case DbType.Date:

                        try
                        {
                            DateTime myDate;
                            // emptystring or space will not parse and nets a DBNull.Value
                            if (DateTime.TryParse(value.ToString(), out myDate))
                            {
                                // uninitialized DateTime is 1/1/0001
                                if (myDate.Year > 1900)
                                {
                                    returnValue = myDate;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(
                                "Invalid date - could not parse, here is what was entered: " + value, ex);
                        }
                        break;

                    case DbType.Double:
                        try
                        {
                            double myDouble;
                            // emptystring or space will not parse and nets a DBNull.Value
                            if (double.TryParse(value.ToString(), out myDouble))
                            {
                                returnValue = myDouble;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(
                                "Invalid double - could not parse, here is what was entered: " + value, ex);
                        }
                        break;

                    case DbType.Int32:
                        try
                        {
                            int myInt;
                            // emptystring or space will not parse and nets a DBNull.Value
                            if (int.TryParse(value.ToString(), out myInt))
                            {
                                returnValue = myInt;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(
                                "Invalid int - could not parse, here is what was entered: " + value, ex);
                        }
                        break;

                    case DbType.Int64:
                        try
                        {
                            long myLong;
                            // emptystring or space will not parse and nets a DBNull.Value
                            if (long.TryParse(value.ToString(), out myLong))
                            {
                                returnValue = myLong;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(
                                "Invalid long - could not parse, here is what was entered: " + value, ex);
                        }
                        break;

                    case DbType.String:
                        // emptystring or space is considered DBNull.Value
                        returnValue = string.IsNullOrWhiteSpace(value.ToString()) ? DBNull.Value : value;
                        break;

                    default:
                        log.Warn("CommonDAO.TranslateNullValue: Unhandled DbType- this type is not handled for DbNull: " + type);
                        throw new Exception("CommonDAO.TranslateNullValue: Unhandled DbType- this type is not handled for DbNull: " + type);
                }

            }

            return returnValue;
        }

        #endregion Helper Methods
    }
}
