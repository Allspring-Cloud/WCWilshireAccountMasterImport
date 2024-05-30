/*
 * <StatusReport.cs>
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
using System.IO;

using WilshireIQImport.DataAccess;
using WellsCapEnterpriseCommons;

namespace WilshireIQImport.Reports
{
    public enum ReportTypes
    {
        REPORT_TYPE_ACCOUNT,
        REPORT_TYPE_ACCOUNTXREF,
        REPORT_TYPE_COMPOSITE,
        REPORT_TYPE_ALL
    }

    /// <summary>
    /// This singleton class create a status report for each of the input files and overall status
    /// </summary>
    public class StatusReport : StatusReportBase
    {
        #region Singleton

        private static StatusReport instance;

        private StatusReport() { EntryList = new List<StatusEntry>(); }

        public static StatusReport Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new StatusReport();
                }

                return instance;
            }
        }

        #endregion

        #region Fields


        EmailUtil email = new EmailUtil();
        string exitStatus;
        DateTime effectiveDate;
        Dictionary<ReportTypes, string> processMap = new Dictionary<ReportTypes, string>();
        string fileName;

        #endregion

        #region Properties

        public EmailUtil Email
        {
            get { return email; }
            set { email = value; }
        }

        public Dictionary<ReportTypes, string> ProcessMap
        {
            get { return processMap; }
            set { processMap = value; }
        }

        public int LoadedRecordCount
        {
            get { return loadedRecordCount; }
            set { loadedRecordCount = value; }
        }

        public DateTime EffectiveDate
        {
            get { return effectiveDate; }
            set { effectiveDate = value; }
        }

        public string ExitStatus
        {
            get { return exitStatus; }
            set { exitStatus = value; }
        }

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        public List<StatusEntry> EntryList
        {
            get;
            set;
        }

        #endregion

        #region Public 

        /// <summary>
        /// Write the status report
        /// </summary>
        public override void WriteStatus()
        {
            ProcessStatusDAO dataAccess = new ProcessStatusDAO();

            if (File.Exists(fileName) == true)
            {
                File.Delete(fileName);
            }

            foreach (ReportTypes type in Enum.GetValues(typeof(ReportTypes)))
            {
                string statusMessage = "Variance in the number of rows received for the process " + processMap[type] + " is " + dataAccess.ReadPercentDifference(processMap[type]).ToString() + "%";
                this.EntryList.Add(new StatusEntry(Program.EffectiveDate, statusMessage));
            }

            dataAccess.ReadAccountsInMultipleComposites();

            using (StreamWriter stream = File.CreateText(fileName))
            {
                stream.WriteLine(Constants.STATUS_REPORT_HEADER);

                foreach(StatusEntry entry in EntryList)
                {
                    stream.WriteLine(entry.ToString());
                }
            }
        }

        private string PutInQuotes(string field)
        {
            return ### SERVER FILE PATH ###";
        }

        /// <summary>
        /// Update the status in the database
        /// </summary>
        /// <param name="type">the type of status to update</param>
        /// <param name="status">the value to use</param>
        public void UpdateStatus(ReportTypes type, string status)
        {
            ProcessStatusDAO processStatus = new ProcessStatusDAO();
            processStatus.CreateOrUpdateStatus(processMap[type], status);
        }

        /// <summary>
        /// Update the status in the database
        /// </summary>
        /// <param name="type">the type of status to update</param>
        /// <param name="status">the value to use</param>
        public void UpdateStatus(ReportTypes type, string status, long rows)
        {
            ProcessStatusDAO processStatus = new ProcessStatusDAO();
            processStatus.CreateOrUpdateStatus(processMap[type], status, rows);
        }

        /// <summary>
        /// Get the number of rows fro the report
        /// </summary>
        /// <param name="type">the type of report to get the rows for</param>
        /// <returns>the number of rows</returns>
        public long GetRowCount(ReportTypes type)
        {
            ProcessStatusDAO processStatus = new ProcessStatusDAO();
            return processStatus.ReadStatusRows(processMap[type]);
        }
        #endregion
    }
}
