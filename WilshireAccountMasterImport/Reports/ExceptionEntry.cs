/*
 * <ExceptionEntry.cs>
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

namespace WilshireAccountMasterImport.Reports
{

    public enum Level
    {
        Error,
        Warning
    }

    /// <summary>
    /// This class is a container for Exception Report message information
    /// </summary>
    public class ExceptionEntry
    {
        #region Fields

        string fileName;
        DateTime fileDate;
        long lineNumber;
        string message;

        #endregion

        #region Properties

        public Level ErrorLevel
        {
            get;
            set;
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public long LineNumber
        {
            get { return lineNumber; }
            set { lineNumber = value; }
        }

        public DateTime FileDate
        {
            get { return fileDate; }
            set { fileDate = value; }
        }

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        public ExceptionEntry(string fileName, DateTime fileDate, long lineNumber, string message, Level errorLevel)
        {
            this.ErrorLevel = errorLevel;
            this.FileName = fileName;
            this.FileDate = fileDate;
            this.LineNumber = lineNumber;
            this.Message = message;
        }

        #endregion

        #region Public

        /// <summary>
        /// Create the delimited string that will go in the exception report
        /// </summary>
        /// <returns>the string</returns>
        override public string ToString()
        {
            return string.Format("{0}{1}{2}{1}{3}{1}{4}{1}{5}", PutInQuotes(fileName), Constants.DELIMITER_REPORT, PutInQuotes(ErrorLevel.ToString()), PutInQuotes(fileDate.ToString()), PutInQuotes(lineNumber.ToString()), PutInQuotes(message));
        }

        private string PutInQuotes(string field)
        {
            return "SERVER FILE PATH";
        }

        #endregion
    }
}
