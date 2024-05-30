/*
 * <AccountMasterRecord.cs>
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

// Wells Cap
using ImportAPI;
using ImportAPI.ColumnInfos;

namespace WilshireAccountMasterImport.Records
{
    public enum AccountMasterFields
    {
        AccountCode,
        AccountName,
        Strategy,
        PerformanceStartDate
    }

    class AccountMasterRecord : RecordBase<AccountMasterFields>
    {
        public Dictionary<string, int> PositionMap
        {
            get;
            set;
        }

        public AccountMasterRecord()
        {
            PositionMap = new Dictionary<string, int>();

            PositionMap.Add(Constants.DB_COL_ACCOUNTCODE, Constants.POSITION_ACCOUNTCODE);
            PositionMap.Add(Constants.DB_COL_ACCOUNTNAME, Constants.POSITION_ACCOUNTNAME);
            PositionMap.Add(Constants.DB_COL_STRATEGY, Constants.POSITION_STRATEGY);
            PositionMap.Add(Constants.DB_COL_PERFORMANCESTARTDATE, Constants.POSITION_STARTDATE);
        }

        public override List<ColumnInfo> GetColumnInfos()
        {
            List<ColumnInfo> infos = new List<ColumnInfo>();

            ColumnInfo accountNumber = new ColumnInfo();
            accountNumber.FieldName = "account_code";
            accountNumber.IsMandatory = true;
            accountNumber.SqlType = System.Data.SqlDbType.VarChar;
            infos.Add(accountNumber);

            ColumnInfo accountName = new ColumnInfo();
            accountName.FieldName = "account_name";
            accountName.IsMandatory = false;
            accountName.SqlType = System.Data.SqlDbType.VarChar;
            infos.Add(accountName);

            ColumnInfo strategy = new ColumnInfo();
            strategy.FieldName = "strategy";
            strategy.IsMandatory = false;
            strategy.SqlType = System.Data.SqlDbType.VarChar;
            infos.Add(strategy);

            ColumnInfo performanceStartDate = new ColumnInfo();
            performanceStartDate.FieldName = "performance_start_date";
            performanceStartDate.IsMandatory = false;
            performanceStartDate.SqlType = System.Data.SqlDbType.DateTime;
            infos.Add(performanceStartDate);

            return infos;
        }

        public override List<AccountMasterFields> GetKeys()
        {
            return Enum.GetValues(typeof(AccountMasterFields)).Cast<AccountMasterFields>().ToList();
        }

        private static Dictionary<AccountMasterFields, ColumnInfo> infos = null;

        public override Dictionary<AccountMasterFields, ColumnInfo> Infos
        {
            get
            {
                if (infos == null)
                {
                    infos = new Dictionary<AccountMasterFields, ColumnInfo>();
                }

                return infos;
            }
        }

        public static AccountMasterRecord GetRecord(List<string> fieldValues)
        {
            return RecordBase<AccountMasterFields>.GetRecord(typeof(AccountMasterRecord), fieldValues) as AccountMasterRecord;
        }
    }
}
