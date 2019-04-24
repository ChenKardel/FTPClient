using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ftp.@interface;

namespace Ftp.entity
{
    static class VisualFileConverter
    {
        public static IEnumerable<VisualFile> ConvertTextToVisualFiles(string text)
        {
            var items = text.Split('\n');
            foreach (var item in items)
            {
                Debug.WriteLine(item);
                var s = item.Trim();

                Regex regex =
                    new Regex("(\\d{2}-\\d{2}-\\d{2}\\s+\\d{2}:\\d{2})(PM|AM)\\s+(<\\S+>)?\\s+(\\d*)\\s+(\\S+)");
                var match = regex.Match(s);
                if (!match.Success)
                {
                    continue;
                }
                var time = match.Groups[1].Value;
                var ampm = match.Groups[2].Value;
                var type = match.Groups[3].Value;
                var bytesStr = match.Groups[4].Value;
                var filename = match.Groups[5].Value;
                DateTime dateTime;
                try
                {
                    dateTime = DateTime.ParseExact(time, "MM-dd-yy  HH:mm", null);
                }
                catch (FormatException)
                {
                    continue;
                }

                if (ampm == "PM")
                {
                    dateTime = dateTime.AddHours(12);
                }

                VisualFile.FType fType;
                if (type.Trim() == "<DIR>")
                {
                    fType = VisualFile.FType.Directory;
                }
                else if (type.Trim() == "")
                {
                    fType = VisualFile.FType.NormalFile;
                }
                else
                {
                    fType = VisualFile.FType.NormalFile;
                }

                int bytes;
                try
                {
                    bytes = Convert.ToInt32(bytesStr);
                }
                catch (FormatException format)
                {
                    bytes = 0;
                }

                yield return new VisualFile(filename, fType, bytes, dateTime);
            }
        }
    }
}
