using System;
using Newtonsoft.Json;

namespace Ftp.@interface
{
    public class VisualFile
    {
        public enum FType
        {
            Directory,
            NormalFile,
            Link
        }

        public int Bytes { get; }

        public DateTime Time { get; set; }

        public FType FileType { get; set; }

        public string Filename { get; }

        public VisualFile(string filename, FType fileType, int bytes,DateTime time)
        {
            Filename = filename;
            FileType = fileType;
            Time = time;
            Bytes = bytes;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}