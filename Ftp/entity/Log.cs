using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ftp.entity
{
    public enum LogType
    {
        Err,
        Plain
    }

    public class Log
    {
        public LogType LogType { get; set; }
        public string Content { get; set; }

        public Log(string content, LogType logType=LogType.Plain)
        {
            this.Content = content;
            this.LogType = logType;
        }
    }
}
