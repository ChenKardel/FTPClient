using System;
using System.IO;
using Newtonsoft.Json;

namespace Ftp.@interface
{
    /// <summary>
    /// 文件实体
    /// </summary>
    public class VisualFile
    {
        /// <summary>
        /// 文件类型，即文件夹，普通文件， 链接
        /// </summary>
        public enum FType
        {
            Directory,
            NormalFile,
            Link
        }

        public long Bytes { get; }
        /// <summary>
        /// 文件最后修改的时间
        /// </summary>
        public DateTime Time { get; set; }

        public FType FileType { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string Filename { get; }

        public VisualFile(string filename, FType fileType, long bytes,DateTime time)
        {
            Filename = filename;
            FileType = fileType;
            Time = time;
            Bytes = bytes;
        }

        public VisualFile(FileInfo fileInfo)
        {
            Filename = fileInfo.Name;
            Time = fileInfo.LastWriteTime;
            Bytes = fileInfo.Length;
            FileType = FType.NormalFile;
        }

        public VisualFile(DirectoryInfo dirInfo)
        {
            Filename = dirInfo.Name;
            Time = dirInfo.LastWriteTime;
            Bytes = 0;
            FileType = FType.Directory;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}