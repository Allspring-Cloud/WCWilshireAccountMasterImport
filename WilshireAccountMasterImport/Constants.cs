/*
 * <Constants.cs>
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

namespace WilshireAccountMasterImport
{
    public enum RuntimeEnvironments
    {
        DEV,
        UAT,
        PROD
    }

    class Constants
    {
        public const string DELIMITER_REPORT = ",";
        public const string DELIMITER = "|";
        public const string EFFECTIVE_DATE_ARG = "effectivedate";
        public const string ENV_DEV_STR = "WELLSCAP?";
        public const string EXCEPTION_REPORT_HEADER = "Filename" + Constants.DELIMITER_REPORT + "ErrorLevel" + Constants.DELIMITER_REPORT + "File Date" + Constants.DELIMITER_REPORT + "Line Number" + Constants.DELIMITER_REPORT + "Message";

        public const string COLUMN_HEADER_0 = "AccountNumber";
        public const string COLUMN_HEADER_1 = "AccountName";
        public const string COLUMN_HEADER_2 = "Strategy";
        public const string COLUMN_HEADER_3 = "PeformanceStartDate";

        public const int POSITION_ACCOUNTCODE = 0;
        public const int POSITION_ACCOUNTNAME = 1;
        public const int POSITION_STRATEGY = 2;
        public const int POSITION_STARTDATE = 3;

        public const string TLR = "TLR";
        public const string NULL = "NULL";

        public const string DB_COL_ACCOUNTCODE = "account_code";
        public const string DB_COL_ACCOUNTNAME = "account_name";
        public const string DB_COL_STRATEGY = "strategy";
        public const string DB_COL_PERFORMANCESTARTDATE = "performance_start_date";

        public const string FileWatchSleep = "fileWatchSleep";
        public const string FileWatcherEtryTimes = "fileWatchRetryTimes";
        public const string FileWatchEndTime = "fileWatchEndTime";
        public const string FailOnFileNotFound = "failOnFileNotFound";


        public const int FILE_KEEP_DAYS = 5;
    }
}
