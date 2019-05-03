using System;
using System.IO;
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

        public long Bytes { get; }

        public DateTime Time { get; set; }

        public FType FileType { get; set; }

        public string Filename { get; }

        public VisualFile(string filename, FType fileType, long bytes,DateTime time)
        {
            Filename = filename;
            FileType = fileType;
            Time = time;
            Bytes = bytes;
        }

        public VisualFile(FileInfo fileInfo, FType fType)
        {
            Filename = fileInfo.Name;
            Time = fileInfo.LastWriteTime;
            Bytes = fileInfo.Length;
            FileType = fType;
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}