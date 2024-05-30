/*
 * <Program.cs>
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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

// Wells Cap
using CommonUtils;
using DependentUtils;

// Project Level
using WilshireAccountMasterImport.FileHandlers;
using WilshireAccountMasterImport.Parsers;
using WilshireAccountMasterImport.DataAccess;
using WilshireAccountMasterImport.Reports;
using LoggerWrapperServices.Services;

namespace WilshireAccountMasterImport
{
    class Program
    {
        private static readonly LoggingService log = new LoggingService(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public static DateTime EffectiveDate { get; set; }
        public static string file_watch_sleep { get; set; }
        public static string file_watch_entry { get; set; }
        public static string file_watch_endtime { get; set; }
        public static string fail_on_file_not_found { get; set; }

        public static RuntimeEnvironments Environment { get; set; }
        public static string ConnString { get; set; }
        static int Main(string[] args)
        {
            string rptFileName = string.Format("AccountMasterExceptionReport_{0}.csv", DateTime.Now.ToString("MMddyyyyhhmm"));
            ExceptionReport.Instance.Filename = Path.Combine(ConfigurationManager.AppSettings["ExceptionReportPath"], rptFileName);
            ExceptionReport.Instance.UncFilename = Path.Combine(ConfigurationManager.AppSettings["ExceptionReportPathUNC"], rptFileName);
            ExceptionReport.Instance.Email.Body = ConfigurationManager.AppSettings["ExceptionReportEmailBody"];
            ExceptionReport.Instance.Email.Hostname = ConfigurationManager.AppSettings["EmailHostname"];
            ExceptionReport.Instance.Email.Port = int.Parse(ConfigurationManager.AppSettings["EmailPort"]);
            ExceptionReport.Instance.Email.Recipient = ConfigurationManager.AppSettings["ExceptionReportEmailRecipient"];
            ExceptionReport.Instance.Email.Sender = ConfigurationManager.AppSettings["ExceptionReportEmailSender"];
            ExceptionReport.Instance.Email.Subject = ConfigurationManager.AppSettings["ExceptionReportEmailSubject"];
            string fname = ConfigurationManager.AppSettings["Filename"];


            Environment = (RuntimeEnvironments)Enum.Parse(typeof(RuntimeEnvironments), ConfigurationManager.AppSettings["Environment"]);
            log.Info("Runtime environment is " + Environment);

            ConnString = ConfigurationManager.ConnectionStrings["WCComposite"].ConnectionString;

            EffectiveDate = DateTime.Now;

            // using new ArgUtils.CommandOptionManager
            ProcessCommandLineArgs(args);

            log.Info("The run date is " + EffectiveDate);

            AccountMasterFileHandler fileHandler = InitAccountMasterFile();
            fileHandler.EffectiveDate = EffectiveDate;
            fileHandler.WaitForFile(fname,args);

            FileUtils utils = new FileUtils(log);
            utils.DeleteOldFiles(ConfigurationManager.AppSettings["FailedLocation"], "AccountMaster*.txt", Constants.FILE_KEEP_DAYS);
            utils.DeleteOldFiles(ConfigurationManager.AppSettings["ArchiveLocation"], "AccountMaster*.txt", Constants.FILE_KEEP_DAYS);
            utils.DeleteOldFiles(ConfigurationManager.AppSettings["ExceptionReportPath"], "AccountMasterExceptionReport*.csv", Constants.FILE_KEEP_DAYS);

            try
            {
                bool success = fileHandler.HandleInboundFile();

                if (success)
                {
                    fileHandler.MoveFile(false);
                }
                else
                {
                    fileHandler.MoveFile(true);
                }
            }
            catch (FileNotFoundException ex)
            {
                ExceptionReport.Instance.AddEntry(new ExceptionEntry(fileHandler.FileName, EffectiveDate, 0, string.Format("File {0} is missing so cannot load data into database", fileHandler.FileName), Level.Error));
                log.Error("The input file " + fileHandler.FileName + "  was not found.", ex);
                fileHandler.MoveFile(true);
                return 1;
            }
            catch (Exception ex)
            {
                log.Error("Account Master Import failed", ex);
                fileHandler.MoveFile(true);
                return 1;
            }

            ExceptionReport.Instance.Create(ExceptionReport.Instance.Filename);
            ExceptionReport.Instance.Email.Body += string.Format("{0}", ExceptionReport.Instance.UncFilename);
            ExceptionReport.Instance.Email.Send();
            return 0;
        }

        private static void ProcessCommandLineArgs(string[] args)
        {
            ArgUtils.CommandOptionManager cmdOptionMgr = new ArgUtils.CommandOptionManager(args);
            cmdOptionMgr.NameWidthForUsage = 14;

            ArgUtils.DateCommandOption dateCmdOption = new ArgUtils.DateCommandOption(Constants.EFFECTIVE_DATE_ARG, false,
                "The effective date used for this instance of the utility.",
                new string[] {"yyyyMMdd"});
            dateCmdOption.HelpTextAddon = new[] {"If not supplied, it will default to the current day."};
            // if not passed, this is the default value
            dateCmdOption.DefaultValue = DateTime.Now.ToString("yyyyMMdd");
            cmdOptionMgr.Add(dateCmdOption);

            ArgUtils.CommandOption fileWatchOption = new ArgUtils.CommandOption(Constants.FileWatchSleep, typeof(String), false,
                "The file watch sleep used for this instance");
            fileWatchOption.HelpTextAddon = new[] { "If not supplied, it will default to 60 seconds." };
            fileWatchOption.DefaultValue = "60";
            cmdOptionMgr.Add(fileWatchOption);

            ArgUtils.CommandOption fileWatchEntry = new ArgUtils.CommandOption(Constants.FileWatcherEtryTimes, typeof(String), false,
                "The file watch retry times used for this instance");
            fileWatchEntry.HelpTextAddon = new[] { "If not supplied, it will default to 1 try." };
            fileWatchEntry.DefaultValue = "1";
            cmdOptionMgr.Add(fileWatchEntry);

            ArgUtils.CommandOption filewatchendOption = new ArgUtils.CommandOption(Constants.FileWatchEndTime, typeof(String), false,
                "The file watch end time used for this instance");
            filewatchendOption.HelpTextAddon = new[] { "If not supplied, it will default to null." };
            filewatchendOption.DefaultValue = "";
            cmdOptionMgr.Add(filewatchendOption);
            

            ArgUtils.CommandOption failOnFileNotFound = new ArgUtils.CommandOption(Constants.FailOnFileNotFound, typeof(String), false,
                "The file watch fail on file not found");
            fileWatchOption.HelpTextAddon = new[] { "If not supplied, it will default to no." };
            fileWatchOption.DefaultValue = "no";
            cmdOptionMgr.Add(failOnFileNotFound);


            ArgUtils myArgUtils = new ArgUtils("=", cmdOptionMgr);
            List<string> validationMsgs = myArgUtils.CmdOptionManager.ProcessArguments();

            // look for any validation messages indicating something is wrong
            if (validationMsgs.Count > 0)
            {
                if (cmdOptionMgr.HelpFlag)
                {
                    // Help/Usage was invoked
                    LogInfoList(validationMsgs);
                    PrintUsage(cmdOptionMgr.GenerateUsage());
                    log.Info("USAGE printed to console as requested, exiting.");
                    log.Info(cmdOptionMgr.GenerateUsageString());
                    System.Environment.Exit(0);
                }

                // there is an ERROR parsing command line args
                foreach (string line in validationMsgs)
                {
                    log.Warn(line);
                }

                // log the list of command line args
                log.Info(cmdOptionMgr.GetArgsForLog());

                // log the list of parsed values
                LogInfoList(cmdOptionMgr.GenerateValueReport());
                PrintUsage(cmdOptionMgr.GenerateUsage());
                System.Environment.Exit(1);
            }

            // validation passed
            if (cmdOptionMgr.GetValue(Constants.EFFECTIVE_DATE_ARG) != null)
            {
                EffectiveDate = (DateTime) cmdOptionMgr.GetValue(Constants.EFFECTIVE_DATE_ARG);
                log.Info(string.Format("Using {0} as {1}", EffectiveDate.ToString("yyyyMMdd"), Constants.EFFECTIVE_DATE_ARG));
            }

            if (cmdOptionMgr.GetValue(Constants.FileWatchSleep) != null)
            {
                file_watch_sleep = (String)cmdOptionMgr.GetValue(Constants.FileWatchSleep);
                 log.Info(String.Format("File watch sleep time is {0}", file_watch_sleep));
            }

            if (cmdOptionMgr.GetValue(Constants.FileWatcherEtryTimes) != null)
            {
                file_watch_entry = (String)cmdOptionMgr.GetValue(Constants.FileWatcherEtryTimes);
                log.Info(String.Format("File watch reentry times is {0}", file_watch_entry));
            }

            if (cmdOptionMgr.GetValue(Constants.FileWatchEndTime) != null)
            {
                file_watch_endtime = (String)cmdOptionMgr.GetValue(Constants.FileWatchEndTime);
                log.Info(String.Format("File watch end time is {0}", file_watch_endtime));
            }

            if (cmdOptionMgr.GetValue(Constants.FailOnFileNotFound) != null)
            {
                fail_on_file_not_found = (String)cmdOptionMgr.GetValue(Constants.FailOnFileNotFound);
                log.Info(String.Format("Fail on file not found is {0}", fail_on_file_not_found));
            }

        }

        /// <summary>
        /// Prints the usage for the utility's command line args
        /// </summary>
        private static void PrintUsage(List<string> usageList)
        {
            foreach (string line in usageList)
            {
                System.Console.WriteLine(line);
            }
        }

        private static void LogInfoList(List<string> valueList)
        {
            foreach (string line in valueList)
            {
                log.Info(line);
            }
        }

        /// <summary>
        /// Initialize and return the account master file handler
        /// </summary>
        /// <returns>the file handler</returns>
        private static AccountMasterFileHandler InitAccountMasterFile()
        {
            log.Info("Initalizing Account File Handler");

            AccountMasterFileHandler accountMasterFile = new AccountMasterFileHandler();

            string curCalandarDay = Program.EffectiveDate.ToString("yyyyMMdd");

            accountMasterFile.FileName = string.Format("AccountMaster_{0}.txt", curCalandarDay);
            accountMasterFile.FileFromLocation = ConfigurationManager.AppSettings["FromLocation"];
            accountMasterFile.FileFailedLocation = ConfigurationManager.AppSettings["FailedLocation"];
            accountMasterFile.FileSuccessLocation = ConfigurationManager.AppSettings["ArchiveLocation"];

            AccountMasterFileParser parser = new AccountMasterFileParser();
            parser.EffectiveDate = EffectiveDate;

            accountMasterFile.FileParser = parser;
            accountMasterFile.FileParser.FileName = accountMasterFile.FileName;
            accountMasterFile.FileParser.FilePath = accountMasterFile.FileFromLocation;

            return accountMasterFile;
        }
    }
}
