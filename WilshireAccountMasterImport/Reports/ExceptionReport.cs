/*
 * <ExceptionReport.cs>
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
using System.IO;
using System.Linq;
using System.Text;

// Wells Cap
using EmailCommon;
using ImportAPI.InboundFileHandler;

namespace WilshireAccountMasterImport.Reports
{
    /// <summary>
    /// This is a singleton class to handle a list of Exception Entries
    /// </summary>

    public class ExceptionReport : ExceptionReportBase
    {
        #region Singleton

        //Singleton Implementation
        private static ExceptionReport instance;

        private ExceptionReport()
        {
            this.Messages.Add(Constants.EXCEPTION_REPORT_HEADER);
        }

        public static ExceptionReport Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ExceptionReport();
                }

                return instance;
            }
        }

        #endregion

        #region Fields
        string filename;
        string uncFilename;

        public string UncFilename
        {
            get { return uncFilename; }
            set { uncFilename = value; }
        }

        public string Filename
        {
            get { return filename; }
            set { filename = value; }
        }

        EmailUtil email = new EmailUtil();

        private List<string> header = new List<string>();

        #endregion

        #region Properties

        public EmailUtil Email
        {
            get { return email; }
            set { email = value; }
        }

        #endregion

        #region Public

        /// <summary>
        /// Adds an entry to the exception list
        /// </summary>
        /// <param name="entry">the entry to add</param>
        public void AddEntry(ExceptionEntry entry)
        {
            this.Messages.Add(entry.ToString());
        }

        #endregion
    }
}
