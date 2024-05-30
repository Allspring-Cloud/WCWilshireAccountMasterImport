/*
 * <AccountMasterFileParser.cs>
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

using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// Wells Cap
using ImportAPI.InboundFileHandler;

// Project Level
using WilshireAccountMasterImport.Records;
using WilshireAccountMasterImport.DataAccess;
using WilshireAccountMasterImport.Reports;
using LoggerWrapperServices.Services;

namespace WilshireAccountMasterImport.Parsers
{
    /// <summary>
    /// This class parses the data from the account master file
    /// </summary>
    class AccountMasterFileParser : FileParserBase
    {
        public DateTime EffectiveDate
        {
            get;
            set;
        }

        private static readonly LoggingService log = new LoggingService(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AccountMasterFileParser() : base()
        {
        }

        /// <summary>
        /// This method parses the account master file, populates record objects, loads the data
        /// </summary>
        /// <returns>success</returns>
        public override bool ParseAndLoad()
        {
            int rowCounter = 0;

            try
            {
                using (StreamReader sr = new StreamReader(Path.Combine(FilePath, FileName)))
                {
                    using (TextFieldParser parser = new TextFieldParser(sr))
                    {
                        log.Info("Beginning to parse " + FileName);
                        parser.Delimiters = new string[] { Constants.DELIMITER };

                        string[] fileHeader = parser.ReadFields();

                        if (fileHeader.Length != 3)
                        {
                            ExceptionReport.Instance.AddEntry(new ExceptionEntry(this.FileName, this.EffectiveDate, 0, String.Format("The file header for {0} is invalid", Path.Combine(FilePath, FileName)), Level.Error));
                            return false;
                        }

                        string[] columnHeader = parser.ReadFields();

                        if (columnHeader.Length != 4 || columnHeader[0] != Constants.COLUMN_HEADER_0 || columnHeader[1] != Constants.COLUMN_HEADER_1 || columnHeader[2] != Constants.COLUMN_HEADER_2 || columnHeader[3] != Constants.COLUMN_HEADER_3)
                        {
                            ExceptionReport.Instance.AddEntry(new ExceptionEntry(this.FileName, this.EffectiveDate, 0, String.Format("The column header for {0} is invalid", Path.Combine(FilePath, FileName)), Level.Error));
                            return false;
                        }

                        rowCounter = 2;

                        string[] fields;

                        AccountMasterRecord record;
                        AccountMasterDAO dataAccess = new AccountMasterDAO(Program.ConnString);

                        while (parser.EndOfData == false)
                        {
                            fields = parser.ReadFields();

                            if (fields[0] != Constants.TLR)
                            {
                                if (fields.Length != 4)
                                {
                                    ExceptionReport.Instance.AddEntry(new ExceptionEntry(this.FileName, this.EffectiveDate, rowCounter, String.Format("The row is invalid.", Path.Combine(FilePath, FileName)), Level.Error));
                                    return false;
                                }

                                if (Program.Environment == RuntimeEnvironments.DEV)
                                {
                                    fields[Constants.POSITION_ACCOUNTNAME] = Constants.ENV_DEV_STR + fields[Constants.POSITION_ACCOUNTCODE];
                                }

                                record = AccountMasterRecord.GetRecord(fields.ToList());
                                dataAccess.LoadRecord(record);
                                rowCounter++;

                                if (rowCounter % 1000 == 0)
                                {
                                    log.Info(rowCounter + " rows parsed");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            log.Info("Account Master Import was successful.");
            return true;
        }
    }
}
