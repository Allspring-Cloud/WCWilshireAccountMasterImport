/*
 * <AccountMasterFileHandler.cs>
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
using System.Configuration;
using System.IO;

// Wells Cap
using ImportAPI.InboundFileHandler;
using LoggerWrapperServices.Services;
using System.Threading;
using CommonUtils;

namespace WilshireAccountMasterImport.FileHandlers
{
    public class AccountMasterFileHandler : InboundFileHandlerBase
    {
        private static readonly LoggingService log = new LoggingService(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string[] args;

        public bool HandleInboundFile()
        {
            return this.ParseFile();
        }

        private Boolean CheckandExtractFile()
        {
            // we need to look for our files.
            string fpath = ConfigurationManager.AppSettings["SetDataPath"];
            string farchive = ConfigurationManager.AppSettings["SetArchivePath"];
            string fname = string.Format("{0}_{1}.{2}", ConfigurationManager.AppSettings["Filename"], DateTime.Now.ToString("yyyyMMdd"),"txt");
            string[] foundem = System.IO.Directory.GetFiles(fpath);
            var match = foundem.Where(x => x.Contains(fname)).FirstOrDefault();
            if (!string.IsNullOrEmpty(match))
            {

                return true;
            }
            log.Info("File not found at " + fpath);
            return false;


        }


        public Boolean WaitForFile(String FName, string[] args)
        {

            if (CheckandExtractFile())
            {
                return true;
            }

            Dictionary<string, string> inputArgs = ArgUtils.GetInputArgs(args);

            string fileWatchSleepStr = "60";  // default
            string fileWatchRetryTimesStr = "1";
            string fileWatchEndTimeStr = null;
            string failOnFileNotFoundStr = "no";

            // get any file watch parmams passed
            bool fileWatchSleepExists = inputArgs.ContainsKey("fileWatchSleep");
            if (fileWatchSleepExists)
            {
                fileWatchSleepStr = inputArgs["fileWatchSleep"];
            }

            bool fileWatchRetryTimesExists = inputArgs.ContainsKey("fileWatchRetryTimes");
            if (fileWatchRetryTimesExists)
            {
                fileWatchRetryTimesStr = inputArgs["fileWatchRetryTimes"];
            }

            bool fileWatchEndTimeExists = inputArgs.ContainsKey("fileWatchEndTime");
            if (fileWatchEndTimeExists)
            {
                fileWatchEndTimeStr = inputArgs["fileWatchEndTime"];
            }

            bool failOnFileNotFoundExists = inputArgs.ContainsKey("failOnFileNotFound");
            if (failOnFileNotFoundExists)
            {
                failOnFileNotFoundStr = inputArgs["failOnFileNotFound"];
            }

            int sleepSec = Int32.Parse(fileWatchSleepStr);
            
            if (fileWatchRetryTimesExists && !fileWatchEndTimeExists)
            {

                int retryTimes = Int32.Parse(fileWatchRetryTimesStr);
                for (int i = 0; i <= retryTimes; i++)
                {
                    if(CheckandExtractFile())
                    {
                        return true;
                    }
                    log.Info("Waiting for file " + FName + ". Counter:" + i + "/" + retryTimes);

                        Thread.Sleep(sleepSec * 1000);

                    }
                }
                       
            else

            {
                string[] cultureNames = { "en-US" };
                DateTime endTime = DateTime.ParseExact(fileWatchEndTimeStr, "h:mmtt", System.Globalization.CultureInfo.InvariantCulture);
                
                while (DateTime.Now < endTime)
                {
                    if (CheckandExtractFile())
                    {
                        return true;
                    }
                                          
                        log.Info("Waiting for file " + FName + ". Will try till " + endTime);
                        Thread.Sleep(sleepSec * 1000);

                    }
                }

            if (!CheckandExtractFile())
            {
                if (String.Equals(failOnFileNotFoundStr, "yes", StringComparison.OrdinalIgnoreCase))

                {

                    log.Info("Failing out becuase files were not found and FailOnFileNotFound is set to yes.");

                    System.Environment.Exit(1);

                }

                return false;
            }
            else
            {
                return true;
            }
                        
        }
        

    }
}
