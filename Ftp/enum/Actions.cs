using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ftp.@enum
{
    static class Actions
    {
        public static string Abort()
        {
            return "ABOR" + "\r\n";
        }

        public static string Account(string account)
        {
            return "ACCT " + account + "\r\n";
        }

        public static string Allocate(int bytes)
        {
            return "ALLO " + bytes + "\r\n";
        }

        public static string Append(string filename)
        {
            return "APPE " + filename + "\r\n";
        }

        public static string ChangeDup(string dirpath)
        {
            return "CDUP " + dirpath + "\r\n";
        }

        public static string ChangeCwd(string dirpath)
        {
            return "CWD " + dirpath + "\r\n";
        }

        public static string Delete(string filename)
        {
            return "DELE " + filename + "\r\n";
        }

        public static string Help(string command)
        {
            return "HELP " + command + "\r\n";
        }

        public static string List(string name)
        {
            return "LIST " + name + "\r\n";
        }

        internal enum ModeType
        {
            Stream, Block, Compressed
        }
        public static string Mode(ModeType mode)
        {
            switch (mode)
            {
                case ModeType.Stream:
                    return "MODE S" + "\r\n";
                case ModeType.Block:
                    return "MODE B" + "\r\n";
                case ModeType.Compressed:
                    return "MODE C" + "\r\n";
                default:
                    throw new ArgumentException("MODE ONLY HAVE THREE TYPES!");
            }
        }

        public static string MakeDir(string directory)
        {
            return "MKD " + directory + "\r\n";
        }

        public static string NList(string directory)
        {
            return "NLST " + directory + "\r\n";
        }

        public static string Noop()
        {
            return "NOOP" + "\r\n";
        }

        public static string Password(string password)
        {
            return "PASS " + password + "\r\n";
        }

        public static string Passsive()
        {
            return "PASV" + "\r\n";
        }

        public static string Port(string address)
        {
            return "PORT " + address + "\r\n";
        }

        public static string Pwd()
        {
            return "PWD" + "\r\n";
        }

        public static string Quit()
        {
            return "QUIT" + "\r\n";
        }
        public static string Reinitialize()
        {
            return "REIN" + "\r\n";
        }

        public static string Reset(int offset)
        {
            return "REST " + offset + "\r\n";
        }

        public static string Retreat(string filename)
        {
            return "RETR " + filename + "\r\n";
        }

        public static string RemoveDir(string directory)
        {
            return "RMD " + directory + "\r\n";
        }

        public static string RenameFrom(string oldPath)
        {
            return "RNFR " + oldPath + "\r\n";
        }

        public static string RenameTo(string newPath)
        {
            return "RNTO " + newPath + "\r\n";
        }

        public static string Site(string @params)
        {
            return "SITE " + @params + "\r\n";
        }

        public static string Smnt(string pathname)
        {
            return "SMNT " + pathname + "\r\n";
        }

        public static string State(string directory)
        {
            return "STAT " + directory + "\r\n";
        }

        public static string Stor(string filename)
        {
            return "STOR " + filename + "\r\n";
        }

        public static string Stou(string filename)
        {
            return "STOU " + filename + "\r\n";
        }

        internal enum StructType
        {
            File, Record, Page
        }
        public static string Structure(StructType structType)
        {
            switch (structType)
            {
                case StructType.File:
                    return "STRU F" + "\r\n";
                case StructType.Record:
                    return "STRU R" + "\r\n";
                case StructType.Page:
                    return "STRU P" + "\r\n";
                default:
                    throw new ArgumentException("STRUCT ONLY HAVE THREE TYPES!");
            }
        }

        public static string System()
        {
            return "SYST" + "\r\n";
        }

        internal enum DataType
        {
            Ascii, Ebcdic, Binary
        }

        public static string Type(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Ascii:
                    return "TYPE A" + "\r\n";
                case DataType.Binary:
                    return "TYPE I" + "\r\n";
                case DataType.Ebcdic:
                    return "TYPE E" + "\r\n";
                default:
                    throw new ArgumentException("data-type ONLY HAVE THREE TYPES!");
            }
        }

        public static string User(string username)
        {
            return "USER " + username + "\r\n";
        }
        
    }
}
