using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WilshireIQImport.Reports
{
    public class StatusEntry
    {
        public DateTime FileDate
        {
            get;
            set;
        }

        public string StatusMessage
        {
            get;
            set;
        }

        public StatusEntry(DateTime date, string message)
        {
            FileDate = date;
            StatusMessage = message;
        }

        public string ToString()
        {
            return FileDate + Constants.DELIMITER_REPORT + StatusMessage;
        }
    }
}
