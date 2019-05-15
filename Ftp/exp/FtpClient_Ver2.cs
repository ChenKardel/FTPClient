using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using log4net;

namespace Ftp.exp
{

    #region   "共通用クラスの定義、　"
    /// <summary>
    /// サーバのファイル種類
    /// </summary>
    public enum FtpFileType
    {
        File,   　　  //ファイル
        Directory,　  //フォルダ
        Other,   　　 //その他(ファイル・フォルダ以外)
        All           //すべて
    }
    /// <summary>
    /// サーバフォルダの属性（絶対パス、
    /// （IP、URL、ルートフォルダ）など親フォルダにり、相対パス
    /// </summary>
    public enum FtpRemoteDirOption
    {

        /// <summary>
        /// 設定されるStrHostUrlURLにより、相対パス
        /// 例　StrHostUrl=199.199.199.199/dataの場合、
        /// 　　①、直下フォルダ「JNF]をアクセスしよう場合、
        /// 　　/JNFで、アクセスできる　
        /// 　　②、直下フォルダ「JNF]の子フォルダ「KK」をアクセスしよう場合、
        /// 　　/JNF/kkで、アクセスできる
        /// </summary>
        UrlOpposite = 0,   　　//設定されるURLにより、相対パス
        /// <summary>
        /// Ipアドレスより、相対パス
        /// 例　StrHostUrl=199.199.199.199/dataの場合、
        /// 　　①、直下フォルダ「JNF]をアクセスしよう場合、
        /// 　　/data/JNFで、アクセスできる　
        /// 　　②、直下フォルダ「JNF]の子フォルダ「KK」をアクセスしよう場合、
        /// 　　/data/JNF/kkで、アクセスできる
        /// </summary>
        IpOpposite = 1,   　　//Ipアドレスより、相対パス
        /// <summary>
        /// 現状作業フォルダに対して、相対パス 
        /// 例　StrHostUrl=199.199.199.199/data、　StrRemoteDir = /JNF の場合、
        /// 　　①、直下フォルダ「JNF]の子フォルダ「KK」をアクセスしよう場合、
        /// 　　/kkで、アクセスできる　
        /// </summary>
        CurWorkDirOpposite = 2,　    //外部設定済みフォルダにより、相対パス    
        /// <summary>
        /// ログインユーザの権限で、見えるルートフォルダより、絶対パス
        /// 例：Linuxの場合、   Var/tmp/xxxx
        ///     WinDowsの場合、　 /xxx/
        /// 例　StrHostUrl=199.199.199.199/dataの場合、(ルート権限で、
        /// 　　/JNF   ×
        /// 　　/var/www/html/dat/USERS/ndscst/JNF  
        /// </summary>
        Absolutely = 3,　  //ルートフォルダより、絶対パス(

    }
    /// <summary>
    /// 通信データタイプ
    /// </summary>
    public enum FtpTransType
    {
        // A=ASCII，E=EBCDIC，I=binary
        ASCII = 0,
        BINARY = 1,
        EBCDIC = 2
    }
    /// <summary>
    /// 接続モード（Port、Pasv,両方）
    /// </summary>
    public enum FtpDataTransMode
    {
        // ポートモード、パッシプコード、オール　
        Port = 0,
        Pasv = 1,
        All = 2
    }
    /// <summary>
    /// サーバのファイルの情報クラス
    /// </summary>
    public class FtpFileInfo
    {   //フィアル名
        public String FileName { get; set; }
        //ファイルサイズ
        public long fileSize { get; set; }
        //ファイルタイプ（ファイル、フォルダ）
        public FtpFileType FileType { get; set; }
        //コメント
        public String Perssion { get; set; }
        //フォルダの場合、とフォルダ下のファイル（フォルダ）情報
        public List<FtpFileInfo> FileList { get; set; }
        //Ftp://　～/
        //サーバの格納箇所(
        public String FileRemotePath { get; set; }
        //上記フォルダの命名種類
        public FtpRemoteDirOption FilePathType { get; set; }

        //更新日付
        public DateTime ModTime { get; set; }

        #region "初期化関数"

        /// <summary>
        /// 
        /// </summary>
        public FtpFileInfo()
            : this("", FtpFileType.All, 0, "", FtpRemoteDirOption.Absolutely)
        {
        }
        /// <summary>
        /// 初期化関数①
        /// </summary>
        /// <param name="curFileName"></param>
        /// <param name="curFiletype"></param>
        /// <param name="curFileSize"></param>
        /// <param name="curFileRemotePath"></param>
        /// <param name="curPerssion"></param>
        public FtpFileInfo(String curFileName,
                         FtpFileType curFiletype,
                         long curFileSize,
                         String curFileRemotePath,
                           FtpRemoteDirOption curFilePathType,
                    String curPerssion)
            : this(curFileName,
                   curFiletype,
                   curFileSize,
                   curFileRemotePath,
                   curFilePathType, null,
                   curPerssion, default(DateTime))
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="curFileName"></param>
        /// <param name="curFiletype"></param>
        /// <param name="curFileSize"></param>
        /// <param name="curFileRemotePath"></param>
        public FtpFileInfo(String curFileName,
                        FtpFileType curFiletype,
                        long curFileSize,
                           String curFileRemotePath,
                           FtpRemoteDirOption curFilePathType)
            : this(curFileName,
                   curFiletype,
                   curFileSize,
                   curFileRemotePath,
                   curFilePathType, null, "", default(DateTime))
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="curFileName"></param>
        /// <param name="curFiletype"></param>
        /// <param name="curFileSize"></param>
        /// <param name="curFileRemotePath"></param>
        /// <param name="curList"></param>
        /// <param name="curPerssion"></param>
        public FtpFileInfo(string curFileName,
                       FtpFileType curFiletype,
                       long curFileSize,
                       String curFileRemotePath,
                           FtpRemoteDirOption curFilePathType,
                         List<FtpFileInfo> curList,
                         String curPerssion,
                         DateTime curModTime)
        {
            this.FileName = curFileName;
            this.fileSize = curFileSize;
            this.FileType = curFiletype;
            this.Perssion = curPerssion;
            this.FileRemotePath = curFileRemotePath;
            this.FilePathType = curFilePathType;
            this.FileList = new List<FtpFileInfo>();
            this.ModTime = curModTime;
            if (curList != null)
            {
                this.FileList.AddRange(curList.GetRange(0, curList.Count));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="curFileName"></param>
        /// <param name="curFiletype"></param>
        /// <param name="curFileSize"></param>
        /// <param name="curFileRemotePath"></param>
        /// <param name="curList"></param>
        public FtpFileInfo(string curFileName,
                          FtpFileType curFiletype,
                          long curFileSize,
                          String curFileRemotePath,
                           FtpRemoteDirOption curFilePathType,
                          List<FtpFileInfo> curList)
            : this(curFileName,
                curFiletype,
                curFileSize,
                curFileRemotePath,
                curFilePathType,
                curList, "", default(DateTime))
        {
        }
        #endregion
    }

    /// <summary>
    /// コードのメッセージ
    /// </summary>
    public class FtpCodeMsg
    {
        String StrCode;
        String StrName;
        String StrComment;
        List<FtpCodeMsg> curListDetail;
        /// コード
        /// </summary>
        public String Code
        {
            get
            {
                return StrCode;
            }
            set
            {
                StrCode = value;
            }
        }
        /// <summary>
        /// コードのメッセージ
        /// </summary>
        public String Name
        {
            get
            {
                return StrName;
            }
            set
            {
                StrName = value;
            }
        }
        /// <summary>
        /// コードのコメント
        /// </summary>
        public String Comment
        {
            get
            {
                return StrComment;
            }
            set
            {
                StrComment = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public List<FtpCodeMsg> DetailList
        {
            get
            {
                return curListDetail;
            }
            set
            {
                curListDetail = new List<FtpCodeMsg>();
                if (value != null)
                {
                    curListDetail.AddRange(value.GetRange(0, value.Count));
                }
            }

        }
        /// <summary>
        /// 定義関数
        /// </summary>
        public FtpCodeMsg()
            : this("", "", null, "")
        {
        }
        /// <summary>
        /// 定義関数
        /// </summary>
        public FtpCodeMsg(String curCode,
                          String curName)
            : this(curCode, curName, null, "")
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="curCode"></param>
        /// <param name="curName"></param>
        /// <param name="curDetailArray"></param>
        public FtpCodeMsg(String curCode,
                        String curName,
                          FtpCodeMsg[] curDetailArray)
            : this(curCode, curName, curDetailArray, "")
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="curCode"></param>
        /// <param name="curName"></param>
        /// <param name="curComment"></param>
        public FtpCodeMsg(String curCode,
                        String curName,
                         String curComment)
            : this(curCode, curName, null, curComment)
        {
        }
        /// <summary>
        /// 定義関数
        /// </summary>
        /// <param name="curCode"></param>
        /// <param name="curMsg"></param>
        public FtpCodeMsg(String curCode,
                          String curName,
                          FtpCodeMsg[] curDetailArray,
                          String curComment)
        {
            this.Code = curCode;
            this.Name = curName;
            this.Comment = curComment;
            this.DetailList = null;
            if (curDetailArray != null)
            {
                this.DetailList.AddRange(curDetailArray);
            }

        }
        /// <summary>
        /// 明細の取得①
        /// </summary>
        /// <param name="curCode"></param>
        /// <returns></returns>
        public FtpCodeMsg GetDetailByCode(
                                 String curCode)
        {
            return this.DetailList.Find(delegate(FtpCodeMsg curObj)
            {
                return curObj.Name.Equals(curCode);
            });
        }
        /// <summary>
        /// 明細の取得①
        /// </summary>
        /// <param name="curCode"></param>
        /// <returns></returns>
        public FtpCodeMsg GetSubDetailByCode(
                                 String curCode)
        {
            FtpCodeMsg curObjItem = this.DetailList.Find(curObj => curObj.Code.Equals(curCode));
            if (curObjItem != null)
            {
                curObjItem = curObjItem.GetDetailByCode(curCode);
            }
            return curObjItem;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public String toString()
        {
            StringBuilder curStr = new StringBuilder();
            curStr.Append(@"[");
            curStr.Append(this.Code);
            curStr.Append(@"]:[");
            curStr.Append(this.Name);
            curStr.Append(@"]:(");
            curStr.Append(this.Comment);
            curStr.Append(@")");

            return curStr.ToString();
        }
    }

    /// <summary>
    /// ftpサーバコマンドクラス
    /// </summary>
    public class FtpCommand : FtpCodeMsg
    {
        /// <summary>
        /// 定義関数
        /// </summary>
        public FtpCommand()
            : this("", "", null, "")
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="curCmd"></param>
        /// <param name="curCmdName"></param>
        public FtpCommand(String curCmd,
                          String curCmdName)
            : this(curCmd, curCmdName, null, "")
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="curCmd"></param>
        /// <param name="curCmdName"></param>
        /// <param name="curRepMsgArry"></param>
        public FtpCommand(String curCmd,
                        String curCmdName,
                          FtpCodeMsg[] curRepMsgArry)
            : this(curCmd, curCmdName, curRepMsgArry, "")
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="curCmd"></param>
        /// <param name="curCmdName"></param>
        /// <param name="curComment"></param>
        public FtpCommand(String curCmd,
                        String curCmdName,
                         String curComment)
            : this(curCmd, curCmdName, null, curComment)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="curCmd"></param>
        /// <param name="curCmdName"></param>
        /// <param name="curRepMsgArry"></param>
        /// <param name="curComment"></param>
        public FtpCommand(String curCmd,
                          String curCmdName,
                          FtpCodeMsg[] curRepMsgArry,
                          String curComment)
            : base(curCmd,
            curCmdName,
            curRepMsgArry,
            curComment)
        {

        }

    }
    #endregion

    #region   "コマンド,応答コードなど共通情報の定義"
    public static class FtpCommon
    {

        #region   "データ転送タイプ（ASCII、
        /// <summary>
        /// 
        /// </summary>
        public static FtpCodeMsg TRAN_ASCII = new FtpCodeMsg("A", "ASCII", "データ通信データ種類");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCodeMsg TRAN_BINARY = new FtpCodeMsg("I", "BINARY", "データ通信データ種類");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCodeMsg TRAN_EBCDIC = new FtpCodeMsg("E", "EBCDIC", "データ通信データ種類");

        /// <summary>
        /// 
        /// </summary>
        public static FtpCodeMsg TRAN_A = new FtpCodeMsg(FtpTransType.ASCII.ToString(),
                                                                   "ASCII", new FtpCodeMsg[] { TRAN_ASCII }, "データ通信データ種類");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCodeMsg TRAN_I = new FtpCodeMsg(FtpTransType.BINARY.ToString(), "BINARY", new FtpCodeMsg[] { TRAN_BINARY }, "データ通信データ種類");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCodeMsg TRAN_E = new FtpCodeMsg(FtpTransType.EBCDIC.ToString(), "EBCDIC", new FtpCodeMsg[] { TRAN_EBCDIC }, "データ通信データ種類");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCodeMsg TRAN_TYPELIST = new FtpCodeMsg("TRANS_TYPELIST", "TRANS_TYPELIST", new FtpCodeMsg[] { TRAN_A, TRAN_I, TRAN_E }, "データ通信データ種類");


        #endregion

        #region   "コマンドむけ応答コード系"

        public static FtpCodeMsg REP_110 = new FtpCodeMsg("110", "110", "Restart marker reply.[ RESTコマンドのためのマーカー返答である]");
        public static FtpCodeMsg REP_120 = new FtpCodeMsg("120", "120", "Service ready in nnn minutes.[ サービスは停止しているが、nnn分後に準備できる]");
        public static FtpCodeMsg REP_125 = new FtpCodeMsg("125", "125", "Data connection already open; transfer starting.[ データコネクションはすでに確立されている。このコネクションで転送を開始する]");
        public static FtpCodeMsg REP_150 = new FtpCodeMsg("150", "150", "File status okay; about to open data connection.[ ファイルステータスは正常である。データコネクションを確立する]");
        public static FtpCodeMsg REP_200 = new FtpCodeMsg("200", "200", "Command okay.[ コマンドは正常に受け入れられた]");
        public static FtpCodeMsg REP_202 = new FtpCodeMsg("202", "202", "Command not implemented, superfluous at this site.[ コマンドは実装されていない。SITEコマンドでOSコマンドが適切でない場合など]");
        public static FtpCodeMsg REP_211 = new FtpCodeMsg("211", "211", "System status, or system help reply.[ STATコマンドに対するレスポンス]");
        public static FtpCodeMsg REP_212 = new FtpCodeMsg("212", "212", "Directory status.[ STATコマンドによるディレクトリ情報を示す]");
        public static FtpCodeMsg REP_213 = new FtpCodeMsg("213", "213", "File status.[ STATコマンドによるファイル情報を示す]");
        public static FtpCodeMsg REP_214 = new FtpCodeMsg("214", "214", "Help message.[ HELPコマンドに対するレスポンス]");
        public static FtpCodeMsg REP_215 = new FtpCodeMsg("215", "215", "NAME system type.Where NAME is an official system name from the list in the Assigned Numbers document.[ SYSTコマンドに対するレスポンス]");
        public static FtpCodeMsg REP_220 = new FtpCodeMsg("220", "220", "Service ready for new user.[ 新規ユーザー向けに準備が整った。ログイン時に表示される場合を想定している]");
        public static FtpCodeMsg REP_221 = new FtpCodeMsg("221", "221", "Service closing control connection.  Logged out if appropriate.[ コントロールコネクションを切断する。QUITコマンド時のレスポンス]");
        public static FtpCodeMsg REP_225 = new FtpCodeMsg("225", "225", "Data connection open; no transfer in progress.[ データコネクションを確立した。データの転送は行われていない]");
        public static FtpCodeMsg REP_226 = new FtpCodeMsg("226", "226", "Closing data connection.  Requested file action successful (for example, file transfer or file abort).[ 要求されたリクエストは成功した。データコネクションをクローズする]");
        public static FtpCodeMsg REP_227 = new FtpCodeMsg("227", "227", "Entering Passive Mode (h1,h2,h3,h4,p1,p2).[ PASVコマンドへのレスポンス。h1～h4はIPアドレス、p1～p2はポート番号を示す]");
        public static FtpCodeMsg REP_230 = new FtpCodeMsg("230", "230", "User logged in, proceed.[ ユーザーログインの成功]");
        public static FtpCodeMsg REP_250 = new FtpCodeMsg("250", "250", "Requested file action okay, completed.[ 要求されたコマンドによる操作は正常終了した]");
        public static FtpCodeMsg REP_257 = new FtpCodeMsg("257", "257", "PATHNAME created.[ ファイルやディレクトリを作成したというのがRFCでの意味だが、MKDコマンドの結果以外にも、実際にはPWDコマンドの結果にも用いられる]");
        public static FtpCodeMsg REP_331 = new FtpCodeMsg("331", "331", "User name okay, need password.[ パスワードの入力を求める]");
        public static FtpCodeMsg REP_332 = new FtpCodeMsg("332", "332", "Need account for login.[ ACCTコマンドで課金情報を指定する必要がある]");
        public static FtpCodeMsg REP_350 = new FtpCodeMsg("350", "350", "Requested file action pending further information.[ 他の何らかの情報を求めている]");
        public static FtpCodeMsg REP_421 = new FtpCodeMsg("421", "421", "Service not available, closing control connection.[ サービスを提供できない。コントロールコネクションを終了する。サーバのシャットダウン時など]");
        public static FtpCodeMsg REP_425 = new FtpCodeMsg("425", "425", "Can't open data connection.[ データコネクションをオープンできない]");
        public static FtpCodeMsg REP_426 = new FtpCodeMsg("426", "426", "Connection closed; transfer aborted.[ 何らかの原因により、コネクションをクローズし、データ転送も中止した]");
        public static FtpCodeMsg REP_450 = new FtpCodeMsg("450", "450", "Requested file action not taken.[ 要求されたリクエストはアクセス権限やファイルシステムの理由で実行できない]");
        public static FtpCodeMsg REP_451 = new FtpCodeMsg("451", "451", "Requested action aborted: local error in processing.[ ローカルエラーのため処理を中止した]");
        public static FtpCodeMsg REP_452 = new FtpCodeMsg("452", "452", "Requested action not taken.Insufficient storage space in system.[ ディスク容量の問題で実行できない]");
        public static FtpCodeMsg REP_500 = new FtpCodeMsg("500", "500", "Syntax error, command unrecognized. This may include errors such as command line too long.[ コマンドの文法エラー]");
        public static FtpCodeMsg REP_501 = new FtpCodeMsg("501", "501", "Syntax error in parameters or arguments.[ 引数やパラメータの文法エラー]");
        public static FtpCodeMsg REP_502 = new FtpCodeMsg("502", "502", "Command not implemented.[ コマンドは未実装である]");
        public static FtpCodeMsg REP_503 = new FtpCodeMsg("503", "503", "Bad sequence of commands.[ コマンドを用いる順番が間違っている]");
        public static FtpCodeMsg REP_504 = new FtpCodeMsg("504", "504", "Command not implemented for that parameter.[ 引数やパラメータが未実装]");
        public static FtpCodeMsg REP_530 = new FtpCodeMsg("530", "530", "Not logged in.[ ユーザーはログインできなかった]");
        public static FtpCodeMsg REP_532 = new FtpCodeMsg("532", "532", "Need account for storing files.[ ファイル送信には、ACCTコマンドで課金情報を確認しなくてはならない]");
        public static FtpCodeMsg REP_550 = new FtpCodeMsg("550", "550", "Requested action not taken.  File unavailable (e.g., file not found, no access).[ 要求されたリクエストはアクセス権限やファイルシステムの理由で実行できない]");
        public static FtpCodeMsg REP_551 = new FtpCodeMsg("551", "551", "Requested action aborted: page type unknown.[ ページ構造のタイプの問題で実行できない]");
        public static FtpCodeMsg REP_552 = new FtpCodeMsg("552", "552", "Requested file action aborted. Exceeded storage allocation (for current directory or  dataset).[ ディスク容量の問題で実行できない]");
        public static FtpCodeMsg REP_553 = new FtpCodeMsg("553", "553", "Requested action not taken.  File name not allowed.[ ファイル名が間違っているため実行できない]");
        /// <summary>
        /// 全体応答コードリスト
        /// </summary>
        public static FtpCodeMsg REP_LIST = new FtpCodeMsg("REP_LIST", "REP_LIST",
                                    new FtpCodeMsg[]{
                                        REP_110,
                                        REP_120,
                                        REP_125,
                                        REP_150,
                                        REP_200,
                                        REP_202,
                                        REP_211,
                                        REP_212,
                                        REP_213,
                                        REP_214,
                                        REP_215,
                                        REP_220,
                                        REP_221,
                                        REP_225,
                                        REP_226,
                                        REP_227,
                                        REP_230,
                                        REP_250,
                                        REP_257,
                                        REP_331,
                                        REP_332,
                                        REP_350,
                                        REP_421,
                                        REP_425,
                                        REP_426,
                                        REP_450,
                                        REP_451,
                                        REP_452,
                                        REP_500,
                                        REP_501,
                                        REP_502,
                                        REP_503,
                                        REP_504,
                                        REP_530,
                                        REP_532,
                                        REP_550,
                                        REP_551,
                                        REP_552,
                                        REP_553,
                                        }, "");

        #endregion

        #region   "コマンド系"
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_ABOR = new FtpCommand("ABOR", "ABOR");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_ACCT = new FtpCommand("ACCT", "ACCT");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_ALLO = new FtpCommand("ALLO", "ALLO");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_APPE = new FtpCommand("APPE", "APPE");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_CDUP = new FtpCommand("CDUP", "CDUP");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_CWD = new FtpCommand("CWD", "CWD",
                                            new FtpCodeMsg[] { FtpCommon.REP_250 });
        /// <summary>
        /// フォルダ作成の専用
        /// </summary>
        public static FtpCommand CMD_CWD_MKD = new FtpCommand("CWD", "CWD",
                                              new FtpCodeMsg[] { FtpCommon.REP_250,
                                                              FtpCommon.REP_550  });
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_DELE = new FtpCommand("DELE", "DELE",
                                            new FtpCodeMsg[] {FtpCommon.REP_200,
                                                              FtpCommon.REP_250,
                                                              FtpCommon.REP_550  });
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_HELP = new FtpCommand("HELP", "HELP",
                                            new FtpCodeMsg[] {FtpCommon.REP_214,
                                                              FtpCommon.REP_200,
                                                              FtpCommon.REP_250,
                                                              FtpCommon.REP_500});
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_LIST = new FtpCommand("LIST", "LIST",
                                            new FtpCodeMsg[] { FtpCommon.REP_150, FtpCommon.REP_125, FtpCommon.REP_226 });
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_MODE = new FtpCommand("MODE", "MODE");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_MKD = new FtpCommand("MKD", "MKD",
                                            new FtpCodeMsg[] { FtpCommon.REP_550,
                                                              FtpCommon.REP_257});
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_NLST = new FtpCommand("NLST", "NLST");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_NOOP = new FtpCommand("NOOP", "NOOP",
                                            new FtpCodeMsg[] { FtpCommon.REP_200,
                                                              FtpCommon.REP_250});
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_PASS = new FtpCommand("PASS", "PASS",
                                   new FtpCodeMsg[] { FtpCommon.REP_230, 
                              　　　　　　　      　FtpCommon.REP_202 });
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_PASV = new FtpCommand("PASV", "PASV",
                                      new FtpCodeMsg[] { FtpCommon.REP_227 });
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_PORT = new FtpCommand("PORT", "PORT",
                                   new FtpCodeMsg[] { FtpCommon.REP_220, 
                              　　　　　　　      　FtpCommon.REP_200,
                              　　　　　　　      　FtpCommon.REP_250 });
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_PWD = new FtpCommand("PWD", "PWD",
                                  new FtpCodeMsg[] { FtpCommon.REP_257, 
                          　　　　　　　            FtpCommon.REP_250});
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_QUIT = new FtpCommand("QUIT", "QUIT");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_REIN = new FtpCommand("REIN", "REIN");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_REST = new FtpCommand("REST", "REST");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_RETR = new FtpCommand("RETR", "RETR",
                                        new FtpCodeMsg[]{
                                         FtpCommon.REP_150,
                                         FtpCommon.REP_125,
                                         FtpCommon.REP_226,
                                         FtpCommon.REP_250});
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_RMD = new FtpCommand("RMD", "RMD",
                                        new FtpCodeMsg[]{
                                         FtpCommon.REP_250});
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_RNFR = new FtpCommand("RNFR", "RNFR",
                                        new FtpCodeMsg[]{
                                         FtpCommon.REP_350,
                                         FtpCommon.REP_200,
                                         FtpCommon.REP_250});
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_RNTO = new FtpCommand("RNTO", "RNTO",
                                        new FtpCodeMsg[]{ 
                                         FtpCommon.REP_200,
                                         FtpCommon.REP_250});
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_SITE = new FtpCommand("SITE", "SITE");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_SMNT = new FtpCommand("SMNT", "SMNT");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_STAT = new FtpCommand("STAT", "STAT");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_STOR = new FtpCommand("STOR", "STOR"
                                            , new FtpCodeMsg[] { FtpCommon.REP_125, FtpCommon.REP_150 }
                                             );
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_STOU = new FtpCommand("STOU", "STOU");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_STRU = new FtpCommand("STRU", "STRU");
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_SYST = new FtpCommand("SYST", "SYST"
                                            , new FtpCodeMsg[] { FtpCommon.REP_215, FtpCommon.REP_200,
                                             FtpCommon.REP_250}
                                             );
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_TYPE = new FtpCommand("TYPE", "TYPE"
                                            , new FtpCodeMsg[] { FtpCommon.REP_200 }
                                             );
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_USER = new FtpCommand("USER", "USER",
                                          new FtpCodeMsg[] { FtpCommon.REP_230,
                                                            FtpCommon.REP_331 }
                                         );
        /// <summary>
        /// 
        /// </summary>
        public static FtpCommand CMD_SIZE = new FtpCommand("SIZE", "SIZE"
                                            , new FtpCodeMsg[] { FtpCommon.REP_213 }
                                             );
        /// <summary>
        /// MDTM
        /// </summary>
        public static FtpCommand CMD_MDTM = new FtpCommand("MDTM", "MDTM"
                                            , new FtpCodeMsg[] { FtpCommon.REP_213 }
                                             );
        #endregion



        #region 　"エラーメッセージ"
        public static FtpCodeMsg ERR_ISNULL = new FtpCodeMsg("001", "ログイン情報が未設定IP[{0}],User[{1}],Pwd[{2}]");
        public static FtpCodeMsg ERR_CONNECTERR = new FtpCodeMsg("002", "コマンド実施エラー。[Command:{0}][Reply:{1}]");
        public static FtpCodeMsg ERR_GETDATASOCKETERR = new FtpCodeMsg("003", "ソケットデータ転送IPアドレス分析エラー。[MSG:{0}]");
        public static FtpCodeMsg ERR_CREATEDATASOCKETERR = new FtpCodeMsg("004", "データ転送ソケット作成エラー。[IPADR:{0}：{1}] ({2})");
        public static FtpCodeMsg ERR_GETLOCALIP = new FtpCodeMsg("005", "ローカルIPアドレス取得失敗。[IPADR:{0}]");
        public static FtpCodeMsg ERR_RENAMEFILE_ISNULL = new FtpCodeMsg("006", "ファイル名変更の場合、変更前後ファイル名未設定");
        public static FtpCodeMsg ERR_PARAM_ISNULL = new FtpCodeMsg("007", "ファイル名が未指定");
        #endregion
    }
    #endregion

    /// <summary>
    /// Ftp操作のメインLisクラス
    /// e-ziwei (上海日華)　
    /// </summary>
    public class FtpClient
    {
        #region ログ出力の設定
        /// <summary>
        /// ログの設定
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion


        #region "全体定数の定義"
        /// <summary>
        /// List of REGEX formats for different FTP server listing formats
        /// </summary>
        /// <remarks>
        /// The first three are various UNIX/LINUX formats, fourth is for MS FTP
        /// in detailed mode and the last for MS FTP in 'DOS' mode.
        /// I wish VB.NET had support for Const arrays like C# but there you go
        /// </remarks>
        private static string[] c_ListRegexFormats = new string[] { 
            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\S+\\s+\\S+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{4})\\s+(?<name>.+)", 
            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\d+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{4})\\s+(?<name>.+)", 
            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\d+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{1,2}:\\d{2})\\s+(?<name>.+)", 
            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})\\s+\\d+\\s+\\S+\\s+\\S+\\s+(?<size>\\d+)\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{1,2}:\\d{2})\\s+(?<name>.+)", 
            "(?<dir>[\\-d])(?<permission>([\\-r][\\-w][\\-xs]){3})(\\s+)(?<size>(\\d+))(\\s+)(?<ctbit>(\\w+\\s\\w+))(\\s+)(?<size2>(\\d+))\\s+(?<timestamp>\\w+\\s+\\d+\\s+\\d{2}:\\d{2})\\s+(?<name>.+)", 
            "(?<timestamp>\\d{2}\\-\\d{2}\\-\\d{2}\\s+\\d{2}:\\d{2}[Aa|Pp][mM])\\s+(?<dir>\\<\\w+\\>){0,1}(?<size>\\d+){0,1}\\s+(?<name>.+)" };

        private static string c_RegexDir = "dir";
        private static string c_RegexPer = "permission";
        private static string c_RegexSize = "size";
        private static string c_RegexTime = "timestamp";
        private static string c_RegexName = "name";

        /// <summary>
        ///Ftpアドレスの分析
        /// </summary>
        private const string c_REGEX_STR_FTPADR = @"^(?<ftp>((F|f)(T|t)(P|p)://)?)((?<User>[^:]+):(?<Pwd>[^@]*)@)?(?<Address>([\w-]+\.)+[\w-]+)([:]?(?<Port>[\d]+))?(?<SubDir>(/[\w-./?%&=]*)?)$";
        private const string c_RegexFtp = "ftp";
        private const string c_RegexUser = "User";
        private const string c_RegexPwd = "Pwd";
        private const string c_RegexAdr = "Address";
        private const string c_RegexPort = "Port";
        private const string c_RegexSubDir = "SubDir";
        /// <summary>
        /// IPアドレス
        /// </summary>
        private const string c_REGEX_STR_IP = @"\b(([01]?\d?\d|2[0-4]\d|25[0-5])\.){3}([01]?\d?\d|2[0-4]\d|25[0-5])\b";
        /// <summary>
        /// データ転送用IPアドレスの分析用
        /// </summary>
        private const string c_REGEX_STR_IP_TRANS = @"(?<Address>(([01]?\d?\d|2[0-4]\d|25[0-5])\,){3}([01]?\d?\d|2[0-4]\d|25[0-5]))\,(?<Port>[0-9]{1,})\,(?<Port1>[0-9]{1,})";
        private const string c_RegexPort1 = "Port1";
        ///応答コートの取得
        private const string c_REGEX_STR_REPCODE = @"^(?<Code>[0-9]{3})[^0-9]{1}(\S|\s)+$";
        private const string c_RepCode = "Code";

        ///サーバ接続の場合、ログインフォルダ
        private const string c_REGEX_STR_DEFAULTPATH = @"(?<Path>/{1}(((/{1}\.{1})?[a-zA-Z0-9 ]+/?)+(\.{1}[a-zA-Z0-9]{2,4})?)?)";
        private const string c_RepPath = "Path";

        /// <summary>
        /// 改行文字
        /// </summary>
        private const string c_CTRLF = "\r\n";
        private const string c_LF = "\n";

        /// <summary>
        /// 接続エラーの場合、3回で、繰り返し接続
        /// </summary>
        private const int RECONNECT_CNT = 3;

        ///接続エラーの場合、スリープ時間：10秒
        private const int SLEEP_SECONDS = 10 * 1000;


        /// <summary>
        /// データ送信・受信バッフのサイズ
        /// </summary>
        private const int c_BLOCK_SIZE = 1024;
        /// <summary>
        /// デフォルトポート値
        /// </summary>
        private const int c_DEFAULT_PORT = 21;

        #endregion

        #region "全体変数の定義"
        /// <summary>
        /// ホストアドレス(999:999:999:999)と方式
        /// 下記DNSで変換後のIpアドレス
        /// </summary>
        private String StrHost;
        public String Host
        {
            get { return StrHost; }
        }
        /// <summary>　
        /// 外部で設定されるホストアドレス
        /// ftp://guest:guest@ddd.ddfd.dd
        /// ftp://guest:guest@ddd.ddfd.dd:88
        /// ftp://guest:guest@ddd.ddfd.dd:88/dfdfd
        /// ftp://guest:guest@ddd.ddfd.dd/dfdfd
        /// 192.168.251.111:23/subfolad
        /// 192.168.251.111/21/subfolad
        /// </summary>
        private String StrHostUrl;
        public String HostUrl
        {
            get { return StrHostUrl; }
            set { StrHostUrl = value; }
        }
        /// <summary>
        /// ポート
        /// </summary>
        private String StrPort;
        public Int32 Port
        {
            get
            {
                if (StrPort.Length > 0)
                {
                    return Int32.Parse(StrPort);
                }
                else
                {
                    return c_DEFAULT_PORT;
                };
            }
            set { StrPort = value.ToString(); }
        }
        /// <summary>
        /// サブフォルダ③
        /// フォルダの構成（デフォルトフォルダ①　＋　ホストサブフォルダ②　 + サブフォルダ③）
        ///StrHostUrlにより、相対パスフォルダ（
        /// </summary>
        private String StrRemotesubDir;
        public String RemotesubDir
        {
            get
            {
                return StrRemotesubDir;
            }
            set { StrRemotesubDir = value; }
        }
        /// <summary>
        /// サブフォルダ①
        /// フォルダの構成（デフォルトフォルダ①　＋　ホストサブフォルダ②　 + サブフォルダ③）
        /// サーバログインのとき、アクセスされるデフォルトフォルダ
        /// （絶対パスFtpRemoteDirOption.Absolutely）
        /// Windows版FTP以外の場合、サーバを登録した場合、デフォルダフォルダが存在する
        ///外部で設定不可
        /// </summary>
        private String StrRemoteCurSubDir;
        public String RemoteCurSubDir
        {
            get { return StrRemoteCurSubDir; }
        }
        /// <summary>
        /// サブフォルダ②
        /// フォルダの構成（デフォルトフォルダ①　＋　ホストサブフォルダ②　 + サブフォルダ③）
        /// 外部で設定されるStrHostUrl中、含めてあるサブフォルダ（IPより、相対パス）
        /// 
        ///外部で設定
        /// </summary>
        private String StrRemoteHostUrlSubDir;
        public String RemoteHostUrlSubDir
        {
            get { return StrRemoteHostUrlSubDir; }
        }
        /// <summary>
        /// 当前作業のディレクトリ
        /// </summary>
        private String StrRemoteAbsolutDir;
        public String RemoteAbsolutDir
        {
            get { return StrRemoteAbsolutDir; }
        }
        /// <summary>
        /// ローカル作業ディレクトリ
        /// </summary>
        private String StrLocalDir;

        public String LocalDir
        {
            get { return StrLocalDir; }
            set { StrLocalDir = value; }
        }
        /// <summary>
        /// ログインユーザ
        /// </summary>
        private String StrUser;
        public String LoginUser
        {
            get { return StrUser; }
            set { StrUser = value; }
        }
        /// <summary>
        /// ログインパスワード
        /// </summary>
        private String StrPassWord;
        public String PassWord
        {
            get { return StrPassWord; }
            set { StrPassWord = value; }
        }
        /// <summary>
        /// サーバ接続フラグ（True：接続済み、False：未接続）
        /// </summary>
        private Boolean blnConnected;

        public Boolean Connected
        {
            get { return blnConnected; }
        }
        ///  
        /// コントロール用socket 
        ///  
        private Socket objsocketControl;

        private Socket socketControl
        {

            get { return objsocketControl; }
            set { objsocketControl = value; }
        }
        ///  
        /// Portモードの場合、監視用ソケット
        ///  
        private Socket objsocketListener;

        private Socket socketListener
        {

            get { return objsocketListener; }
            set { objsocketListener = value; }
        }
        ///  
        /// データ転送用タイプ
        ///  
        private FtpTransType objTranType;
        public FtpTransType TranType
        {
            get { return objTranType; }
            set
            {
                objTranType = value;
                //既に接続した場合、
                if (this.Connected)
                {
                    //コメントの実行
                    ///接続タイプの実行 
                    ///ユーザ
                    this.ExecCommand(FtpCommon.CMD_TYPE,
                              FtpCommon.TRAN_TYPELIST.GetSubDetailByCode(
                                           this.TranType.ToString()
                                           ).Code
                    );
                }
            }
        }
        /// <summary>
        /// 
        /// FTPコマンド発信後、サーバからの返事(最後一行目）
        /// </summary>
        private String strCmdRelayMsg;
        public String CmdRelayMsg
        {
            get { return strCmdRelayMsg; }
        }
        /// <summary>
        /// 
        /// FTPコマンド発信後、サーバからの返事(すべて）
        /// </summary>
        private String strCmdRelayFullMsg;
        public String CmdRelayFullMsg
        {
            get { return strCmdRelayFullMsg; }
        }

        /// <summary>
        /// 処理中、エラーあるかどうか判断フラグ
        /// </summary>
        private Boolean blnErrFlag;
        public Boolean ErrFlag
        {
            get
            {
                return blnErrFlag;
            }
        }
        /// <summary>
        /// ダウンロード、まだは、リストの場合、
        /// 子フォルダが対象になるかどうか
        /// </summary>
        private Boolean blnChildFolderDown;
        public Boolean ChildFolderDown
        {
            get
            {
                return blnChildFolderDown;
            }
            set { blnChildFolderDown = value; }
        }
        /// <summary>
        /// ダウンロードの場合、ローカルファイルに上書きするどうか
        /// </summary>
        private Boolean blnDownLoadOverWrite;
        public Boolean DownLoadOverWrite
        {
            get
            {
                return blnDownLoadOverWrite;
            }
            set { blnDownLoadOverWrite = value; }
        }

        /// <summary>
        /// データソケット接続モード設定　
        /// </summary>
        private FtpDataTransMode eumPasvMode;
        public FtpDataTransMode PasvMode
        {
            get
            {
                return eumPasvMode;
            }
            set
            {
                eumPasvMode = value;
            }
        }
        /// <summary>
        /// RECONNECT_CNT
        /// エラーを起きた場合、繰り返し接続回数
        /// デフォルト　3回
        /// </summary>
        private int intLoopConnectCnt;
        public int LoopConnectCnt
        {
            get
            {
                return (intLoopConnectCnt <= 0 ?
                RECONNECT_CNT : intLoopConnectCnt);
            }
            set
            {
                intLoopConnectCnt = value;
            }
        }
        /// <summary>
        /// エラーの場合、再接続までの待ち時間(単位：ミリ秒)
        /// </summary>
        private int intRetryConnWaitTime;
        public int RetryConnWaitTime
        {
            get
            {
                return (intRetryConnWaitTime <= 0 ?
                      SLEEP_SECONDS : intRetryConnWaitTime);
            }
            set
            {
                intRetryConnWaitTime = value;
            }
        }

        /// <summary>
        /// Ftpサーバのシステム名取得
        /// </summary>
        private String strSystemInfo;
        public String SystemInfo
        {
            get
            {
                return strSystemInfo;
            }
        }
        #endregion

        #region "初期化関数"


        /// <summary>
        /// 初期化関数
        /// </summary>
        /// <returns>接続可の場合、Trueとして返す</returns>
        private bool InitFtpClient()
        {
            Match curMatch = null, curMatchIp = null;
            bool ret = false;

            //個別情報の取得
            this.StrHost = "";
            ///URLが存在の場合、
            if (!this.HostUrl.Trim().Equals(""))
            {
                //
                curMatch = RegexMatch(this.HostUrl,
                          new String[] { c_REGEX_STR_FTPADR });
                //外部のURLから、下記情報の取得
                //ユーザ名、パスワード、アドレス、ポート、サブフォルダなど
                string curTemp = "";
                if (curMatch != null)
                {
                    //ユーザ
                    curTemp = curMatch.Groups[c_RegexUser].Value.Trim();

                    if (!curTemp.Equals(""))
                    {
                        this.LoginUser = curTemp;
                    }
                    //パスワード
                    curTemp = curMatch.Groups[c_RegexPwd].Value.Trim();
                    if (!curTemp.Equals(""))
                    {
                        this.PassWord = curTemp;
                    }
                    //アドレス
                    curTemp = curMatch.Groups[c_RegexAdr].Value.Trim();
                    curMatchIp = RegexMatch(curTemp, new String[] { c_REGEX_STR_IP });
                    //IPアドレス以外の場合、
                    if (!curTemp.Equals("")
                         && (curMatchIp == null || !curMatchIp.Success))
                    {

                        IPAddress[] curIPAddress = Dns.GetHostEntry(curTemp).AddressList;
                        if (curIPAddress != null && curIPAddress.Length != 0)
                        {
                            curTemp = curIPAddress[0].ToString();
                        }
                    }
                    //ホスト名設定
                    this.StrHost = curTemp;
                    //ポート
                    curTemp = curMatch.Groups[c_RegexPort].Value.Trim();
                    if (!curTemp.Equals(""))
                    {
                        this.Port = Int32.Parse(curTemp);
                    }
                    //サブフォルダ
                    curTemp = curMatch.Groups[c_RegexSubDir].Value.Trim();
                    this.StrRemoteHostUrlSubDir = (curTemp.Equals("") ? @"/" : curTemp);
                    //設定した場合、
                    if (!this.StrRemoteHostUrlSubDir.Equals(@"/")
                          && !this.StrRemoteHostUrlSubDir.EndsWith(@"/"))
                    {
                        this.StrRemoteHostUrlSubDir += @"/";
                    }
                }
            }
            //外部のサブフォルダ(未設定の場合、
            if (this.RemotesubDir.Equals(""))
            {
                this.RemotesubDir = @"/";
            }
            //設定した場合、
            if (!this.RemotesubDir.Equals(@"/")
                  && !this.RemotesubDir.EndsWith(@"/"))
            {
                this.RemotesubDir += @"/";
            }
            ///ローカルフォルダ
            if (this.LocalDir.Equals(""))
            {
                this.LocalDir = System.IO.Directory.GetCurrentDirectory();
            }
            //ホスト、ユーザ、パスワード設定済みの場合、
            if (!this.Host.Equals("") && !this.LoginUser.Equals("")
                   && !this.PassWord.Equals(""))
            {
                ret = true;
            }
            //エラー回数
            this.LoopConnectCnt = RECONNECT_CNT;
            //スリープ期間
            this.RetryConnWaitTime = SLEEP_SECONDS;
            //システム情報名
            this.strSystemInfo = "";
            //
            this.strCmdRelayFullMsg = "";
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        public FtpClient(
               )
            : this("",
                  "",
                  c_DEFAULT_PORT,
                  "",
                  "",
                  FtpTransType.BINARY,
                  "", FtpDataTransMode.All)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="curHostUrl"></param>
        /// <param name="curHostUser"></param>
        /// <param name="curHostPassWord"></param>
        public FtpClient(
                String curHostUrl,
                String curHostUser,
                String curHostPassWord
           )
            : this(curHostUrl,
                  "",
                  c_DEFAULT_PORT,
                  curHostUser,
                  curHostPassWord,
                  FtpTransType.BINARY,
                  "", FtpDataTransMode.All)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="curHostUrl"></param>
        /// <param name="curHostUser"></param>
        /// <param name="curHostPassWord"></param>
        /// <param name="curHostsubDir"></param>
        public FtpClient(
             String curHostUrl,
             String curHostUser,
             String curHostPassWord,
             String curHostsubDir
        )
            : this(curHostUrl,
                  curHostsubDir,
                  c_DEFAULT_PORT,
                  curHostUser,
                  curHostPassWord,
                  FtpTransType.BINARY,
                  "", FtpDataTransMode.All)
        {
        }
        /// <summary>
        /// 全体初期化
        /// </summary>
        /// <param name="curHostUrl"></param>
        /// <param name="curHostsubDir"></param>
        /// <param name="curPort"></param>
        /// <param name="curHostUser"></param>
        /// <param name="curHostPassWord"></param>
        /// <param name="curTrans"></param>
        /// <param name="curLocalDir"></param>
        public FtpClient(
                String curHostUrl,
                String curHostsubDir,
                Int32 curPort,
                String curHostUser,
                String curHostPassWord,
                FtpTransType curTrans,
                String curLocalDir,
                 FtpDataTransMode curPasvMode)
        {
            //URL
            this.HostUrl = (curHostUrl == null ? "" : curHostUrl);
            //サブフォルダ
            this.RemotesubDir = (curHostUrl == null ? "" : curHostsubDir);
            //
            this.StrRemoteHostUrlSubDir = "";
            this.PasvMode = curPasvMode;
            //
            this.StrRemoteCurSubDir = @"/";
            //ポート
            this.Port = (curHostUrl == null ? c_DEFAULT_PORT : curPort);
            //
            this.StrPort = c_DEFAULT_PORT.ToString();
            //ユーザ
            this.LoginUser = (curHostUser == null ? "anonymous" : curHostUser);
            //パスワード
            this.PassWord = (curHostPassWord == null ? "anonymous" : curHostPassWord);
            //転送タイプ
            this.TranType = curTrans;
            //ローカル作業フォルダ
            this.LocalDir = (curLocalDir == null ? "" : curLocalDir);
            //作業フォルダの絶対パス
            this.StrRemoteAbsolutDir = "";
            this.blnConnected = false;
            ///接続関数(エラーせずに、接続処理）
            Connect(false);
        }
        #endregion

        #region  "接続関数・クローズ関数"

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errFlag">初期化関数中、エラーしないように</param>
        public void Connect()
        {
            this.Connect(true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errFlag">初期化関数中、エラーしないように</param>
        private void Connect(Boolean errFlag)
        {
            Match curMatch = null;
            int intRetray = this.LoopConnectCnt;
            ///既に接続の場合、
            if (this.blnConnected)
            {
                return;
            }
            //初期化処理
            this.InitFtpClient();
            ///初期化チェック
            if ((this.Host.Equals("") ||
                 this.LoginUser.Equals("")))
            {

                //外で接続の場合、
                if (errFlag)
                {
                    throw (new Exception(String.Format(FtpCommon.ERR_ISNULL.Name,
                                           this.Host, this.LoginUser, this.PassWord)));
                }
                else
                {
                    return;
                }
            }
            while (true)
            {
                try
                {
                    //原状のクローズ
                    CloseControlSocket();
                    intRetray--;
                    ///TCPソケットの作成
                    socketControl = new Socket(AddressFamily.InterNetwork,
                                                 SocketType.Stream, ProtocolType.Tcp);
                    //IpAdress 
                    IPEndPoint ServerIP = new IPEndPoint(IPAddress.Parse(this.Host), this.Port);
                    ///サーバまで接続
                    try
                    {
                        socketControl.Connect(ServerIP);
                    }
                    catch (Exception e)
                    {
                        throw new IOException(
                                 String.Format(FtpCommon.ERR_CONNECTERR.toString(), e.Message)
                                             );
                    }
                    //サーバ接続応答の取得
                    ReadCmdReply();
                    //サーバ接続応答の確認  
                    CheckCmdStatus(ReturnCode(),
                          String.Format("Connect {0}:{1}@{2}:{3}",
                                       this.LoginUser,
                                       this.PassWord,
                                       this.Host,
                                       this.Port), new FtpCodeMsg[] { FtpCommon.REP_220 });
                    ///ユーザ
                    this.ExecCommand(FtpCommon.CMD_USER, this.LoginUser);
                    ///パースワード
                    ///パスワードが必要の場合。
                    if (ReturnCode().Equals(FtpCommon.REP_331.Code))
                    {
                        this.ExecCommand(FtpCommon.CMD_PASS, this.PassWord);
                    }
                    ///ログインの場合、デフォルトフォルダ取得
                    this.ExecCommand(FtpCommon.CMD_PWD);
                    ///当然ログインフォルダの保持
                    if ((curMatch =
                           this.RegexMatch(this.CmdRelayMsg,
                                new String[] { c_REGEX_STR_DEFAULTPATH })) != null)
                    {
                        this.StrRemoteCurSubDir = curMatch.Groups[c_RepPath].Value;
                        //[/XXX/]のように加工
                        if (this.StrRemoteCurSubDir.Length > 0
                            && !this.StrRemoteCurSubDir.EndsWith(@"/"))
                        {
                            this.StrRemoteCurSubDir += @"/";
                        }
                    }

                    //コメントの実行
                    ///接続タイプの実行 
                    ///ユーザ
                    this.ExecCommand(FtpCommon.CMD_TYPE,
                                        FtpCommon.TRAN_TYPELIST.GetSubDetailByCode(
                                                     this.TranType.ToString()
                                                     ).Code
                               );

                    ///システム名の取得
                    ///ユーザ
                    this.ExecCommand(FtpCommon.CMD_SYST);
                    //システム情報名
                    this.strSystemInfo = this.CmdRelayMsg.Replace(this.ReturnCode(), "");
                    //指定フォルダへ移動
                    this.blnConnected = true;
                    //完了
                    this.blnErrFlag = false;
                    //ダウンロード不可
                    this.ChildFolderDown = false;
                    //上書きフラグ
                    this.DownLoadOverWrite = true;
                    //外部フォルダより、アクセスフォルダ変更
                    this.ChangeDir(@"/", FtpRemoteDirOption.CurWorkDirOpposite);

                    break;
                }
                catch (Exception e)
                {
                    //ソケット応答エラー以外の場合、
                    if (this.CmdRelayMsg.Equals("") && intRetray > 0)
                    {
                        //スリープ
                        ExceptionSleepWaiting();
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }
        ///   
        ///サーバ接続のクローズ  
        ///   
        public void DisConnect()
        {
            if (this.socketControl != null
                    && this.blnConnected)
            {
                //クローズ関数
                ExecCommand(FtpCommon.CMD_QUIT);
            }
            CloseControlSocket();
        }
        /// <summary>
        ///  
        /// socketコントロールセッションのクローズ() 
        /// 
        /// </summary>
        private void CloseControlSocket()
        {
            if (this.socketControl != null)
            {
                this.socketControl.Close();
                this.socketControl = null;
            }
            if (this.socketListener != null)
            {
                this.socketListener.Close();
                this.socketListener = null;
            }
            this.blnConnected = false;
        }
        /// <summary>
        ///  
        ///応答エラーの場合、再接続するために、お待ち
        /// 
        /// </summary>
        private void ExceptionSleepWaiting()
        {
            CloseControlSocket();
            Thread.Sleep(this.RetryConnWaitTime);
        }
        #endregion

        #region "コマンド実行関数"

        /// <summary>
        /// コマンド関数
        /// </summary>
        /// <param name="curCmd"></param>
        /// <param name="Param"></param>
        /// <returns></returns>
        private String ExecCommand(FtpCommand curCmd, String[] Param)
        {
            StringBuilder CmdString = new StringBuilder();


            //初期化

            this.strCmdRelayFullMsg = "";
            //コマンド名
            CmdString.Append(curCmd.Name);
            //パラメータの設定
            if (Param != null)
            {
                for (int i = 0; i < Param.Length; i++)
                {
                    if (!Param[i].Trim().Equals(""))
                    {
                        CmdString.Append(" ");
                        CmdString.Append(Param[i].Trim());
                    }
                }
            }
            //コマンド
           //log.Info("FtpCommand:" + CmdString.ToString());
            //改行フラグ
            CmdString.Append(c_CTRLF);
            //コマンドのディコーデング
            byte[] cmdBytes = Encoding.Default.GetBytes((CmdString.ToString()).ToCharArray());
            try
            {
                //発信処理を行う
                this.socketControl.Send(cmdBytes, cmdBytes.Length, 0);
            }
            catch (SocketException e)
            {
                throw new Exception(String.Format("{0} {1} {2}", 
                                                  CmdString.ToString(), 
                                                   e.Message,
                                                   e.StackTrace));
            }
            //サーバから受信処理
            //ただし、QUIT 以外の場合、処理しないように
            if (!curCmd.Name.Equals(FtpCommon.CMD_QUIT.Name))
            {
                ReadCmdReply();
                //コマンドのステータスチェック
                ///応答の確認 
                CheckCmdStatus(ReturnCode(),
                             CmdString.ToString(), curCmd.DetailList.ToArray());
            }
            //コマンド
            //log.Info("FtpCmdResult:" + this.CmdRelayMsg);
            //サーバから受信処理
            return ReturnCode();
        }
        /// <summary>
        /// コマンド関数
        /// </summary>
        /// <param name="curCmd"></param>
        /// <param name="Param"></param>
        /// <returns></returns>
        private String ExecCommand(FtpCommand curCmd, String Param)
        {
            return this.ExecCommand(curCmd,
                   new String[] { (Param == null ? "" : Param) }
                   );
        }
        /// <summary>
        /// コマンド関数
        /// </summary>
        /// <param name="curCmd"></param>
        /// <param name="Param"></param>
        /// <returns></returns>
        private String ExecCommand(FtpCommand curCmd)
        {
            return ExecCommand(curCmd, "");
        }
        /// <summary>
        /// コマンドの実施結果取得
        /// </summary>
        /// <returns></returns>
        private String ReadCmdReply(int sleepTime)
        {
            int curSize = 0;
            Byte[] curGetBuffer = new byte[c_BLOCK_SIZE];
            StringBuilder strReplyMsg = new StringBuilder();
            String[] Message = null;
            String strSplit = "";

            this.strCmdRelayMsg = "";

            sleepTime = (sleepTime <= 0 ? 50 : sleepTime);

            while (true)
            {
                try
                {
                    //コーデング
                    curSize = this.socketControl.Receive(curGetBuffer, curGetBuffer.Length, 0);
                    strReplyMsg.Append(Encoding.ASCII.GetString(curGetBuffer, 0, curSize));
                    //データ未存在の場合、
                    if (curSize < curGetBuffer.Length)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    this.DisConnect();
                    ///エラーフラグ
                    this.blnErrFlag = true;
                    throw (new System.Net.WebException(e.ToString()));
                }
            }
            Thread.Sleep(sleepTime);
            strSplit = c_LF;
            if (strReplyMsg.ToString().Contains(c_CTRLF))
            {
                strSplit = c_CTRLF;
            }
            Message = strReplyMsg.ToString().Split(strSplit.ToCharArray());

            //存在の場合、
            if (Message.Length > 3)
            {
                this.strCmdRelayMsg = Message[Message.Length - 3].ToString();
            }
            else
            {
                this.strCmdRelayMsg = Message[0].ToString();
            }
            //全体結果
            this.strCmdRelayFullMsg += strReplyMsg.ToString();
            //応答コード以外の場合、再読取(応答フォマット：999　＋　　スペース　＋　メッセージ)
            if (!this.strCmdRelayMsg.Substring(3, 1).Equals(" "))
            {
                return ReadCmdReply(sleepTime);
            }
            return this.strCmdRelayMsg;
        }
        /// <summary>
        /// コマンドの実施結果取得
        /// </summary>
        /// <returns></returns>
        private String ReadCmdReply()
        {
            return ReadCmdReply(20);
        }
        ///結果コードの変換
        /// <summary>
        /// コマンドの実施結果取得
        /// </summary>
        /// <returns></returns>
        private String ReturnCode()
        {
            String curCode = "";
            Match curMatch = this.RegexMatch(this.CmdRelayMsg,
                                new String[] { c_REGEX_STR_REPCODE });
            //チェックの場合、
            curCode = (curMatch != null ?
                       curMatch.Groups[c_RepCode].Value.ToString() : "");
            return curCode;
        }
        /// <summary>
        /// コマンドの結果チェック
        /// </summary>
        /// <param name="ComdStatusCode"></param>
        /// <param name="Results"></param>
        private void CheckCmdStatus(String ComdStatusCode,
                                          String curCmdStr, FtpCodeMsg[] Results)
        {
            bool ret = false;

            curCmdStr = (curCmdStr == null ? "" : curCmdStr);
            if (Results != null && ComdStatusCode != null)
            {
                //指定コードが存在の場合、
                foreach (FtpCodeMsg curCode in Results)
                {
                    if (ComdStatusCode.Trim().Equals(curCode.Code))
                    {
                        ret = true;
                        break;
                    }
                }
            }
            //エラーにする
            if (!ret)
            {
                this.DisConnect();
                //エラー
                throw new IOException(
                         String.Format(FtpCommon.ERR_CONNECTERR.toString(),
                                       curCmdStr, this.CmdRelayMsg)
                 );
            }
        }


        #endregion


        #region "フォルダの切り替え処理"

        /// <summary>
        /// パス名の整理関数
        /// 整理後：　/xxxxx/xxx
        /// </summary>
        /// <param name="curPathName"></param>
        /// <returns></returns>
        private String FormatPathName(String curPathName)
        {
            //空白の場合、
            if (curPathName == null || curPathName.Equals(""))
            {
                curPathName = @"/";
            }

            //未設定の場合、
            if (!curPathName.StartsWith(@"/"))
            {
                curPathName = @"/" + curPathName;
            }

            //ダブル//の削除
            while (true)
            {
                if (curPathName.Length > 0 && curPathName.Contains(@"//"))
                {
                    curPathName = curPathName.Replace(@"//", @"/");
                }
                else
                {
                    break;
                }
            }
            //親フォルダ
            while (true)
            {
                if (curPathName.Length > 1 && curPathName.EndsWith(@"/"))
                {
                    curPathName = curPathName.Remove(curPathName.Length - 1);
                }
                else
                {
                    break;
                }
            }
            return curPathName;
        }
        /// <summary>
        /// リモートフォルダと子フォルダの結び処理
        /// </summary>
        /// <param name="pPathname"></param>
        /// <param name="cPathName"></param>
        /// <returns></returns>
        private String TrcatePath(String pPathname, String cPathName)
        {
            StringBuilder curPath = new StringBuilder();
            //設定済みの場合、 
            if (!FormatPathName(pPathname).Equals(@"/"))
            {
                curPath.Append(FormatPathName(pPathname));
            }
            //設定済みの場合、 
            if (!FormatPathName(cPathName).Equals(@"/"))
            {
                curPath.Append(FormatPathName(cPathName));
            }
            //未設定の場合、
            if (curPath.Length <= 0)
            {
                curPath.Append(@"/");
            }
            return curPath.ToString();

        }
        /// <summary>
        /// リモートフォルダの絶対ディレクトリ作成
        /// </summary>
        /// <param name="strDirName"></param>
        /// <param name="CkType"></param>
        /// <returns></returns>
        private String GetRemoteAbsolutePath(String strDirName,
                                      FtpRemoteDirOption CkType)
        {
            String curPath = "", curPath2 = "";
            switch (CkType)
            {
                case FtpRemoteDirOption.UrlOpposite: //URLによって、相対パス
                    ///ログインデフォルトフォルダ　
                    ///＋　URLのサブフォルダ
                    ///+　外部フォルダ
                    //サブフォルダ①＋フォルダ②
                    curPath = this.TrcatePath(this.RemoteCurSubDir,
                                 this.RemoteHostUrlSubDir);
                    //サブフォルダ①＋パラメータ
                    curPath = this.TrcatePath(curPath, strDirName);
                    //現時点の外部処理自動的に設定
                    this.RemotesubDir = this.FormatPathName(strDirName);
                    break;
                case FtpRemoteDirOption.IpOpposite: //IPによって、相対パス
                    //サブフォルダ①＋パラメータ
                    curPath = this.TrcatePath(this.RemoteCurSubDir,
                                                       strDirName);
                    //現時点の外部処理自動的に設定

                    curPath2 = this.TrcatePath(this.RemoteCurSubDir,
                                        this.RemoteHostUrlSubDir);
                    //と同じ親パスの場合、
                    if (curPath.StartsWith(curPath2)
                         && curPath2.Length <= curPath.Length
                         && curPath2.Length > 0)
                    {
                        this.RemotesubDir = curPath.Substring(
                                                        curPath2.Length);
                        //フォーマット処理
                        this.RemotesubDir = this.FormatPathName(this.RemotesubDir);
                    }
                    break;
                case FtpRemoteDirOption.CurWorkDirOpposite: //外部フォルダにって、相対パス
                    //サブフォルダ①＋フォルダ②
                    curPath = this.TrcatePath(this.RemoteCurSubDir,
                                 this.RemoteHostUrlSubDir);
                    //サブフォルダ①＋フォルダ②+フォルダ③
                    curPath = this.TrcatePath(curPath, this.RemotesubDir);
                    //サブフォルダ①＋フォルダ②+フォルダ③+パラメータ
                    curPath = this.TrcatePath(curPath, strDirName);
                    //入れ替え
                    this.RemotesubDir = this.TrcatePath(RemotesubDir, strDirName);
                    break;
                case FtpRemoteDirOption.Absolutely: //ルートパス、相対パス
                    //フォマット処理
                    curPath = this.FormatPathName(strDirName);

                    curPath2 = this.TrcatePath(this.RemoteCurSubDir,
                                      this.RemoteHostUrlSubDir);

                    //と同じ親パスの場合、
                    if (curPath.StartsWith(curPath2)
                          && curPath2.Length <= curPath.Length
                          && curPath2.Length > 0)
                    {
                        //現時点の外部処理自動的に設定
                        this.RemotesubDir = curPath.Substring(
                                                       curPath2.Length);
                        //フォーマット処理
                        this.RemotesubDir = this.FormatPathName(this.RemotesubDir);
                    }
                    else
                    {
                        this.RemotesubDir = "";
                    }
                    break;
            }
            return curPath;
        }
        /// <summary>
        /// フォルダの切り替え
        /// </summary>
        /// <param name="strDirName">HostURLに対して、相対パス</param> 
        public void ChangeDir(String strDirName)
        {
            ChangeDir(strDirName,
                   FtpRemoteDirOption.UrlOpposite);
        }
        /// <summary>
        /// フォルダの切り替え
        /// </summary>
        /// <param name="strDirName">Ip,HostURL、ルートなどに対す相対パス</param>
        /// <param name="CkType">相対パスの識別区分</param>
        public void ChangeDir(String strDirName,
                              FtpRemoteDirOption CkType)
        {

            bool retCnt = false;
            while (true)
            {
                try
                {
                    //未接続の場合、
                    if (!this.Connected)
                    {
                        //接続処理を行う
                        this.Connect();
                        retCnt = true;
                    }
                    //パス作成(未設定の場合、）
                    this.StrRemoteAbsolutDir = GetRemoteAbsolutePath(strDirName, CkType);
                    //実施コマンド実施
                    this.ExecCommand(FtpCommon.CMD_CWD,
                           this.RemoteAbsolutDir);
                    break;

                }
                catch (Exception e)
                {
                    //通信エラーの場合、かつ未再
                    if (this.CmdRelayMsg.Equals("") && !retCnt)
                    {
                        //スリープ
                        ExceptionSleepWaiting();
                        retCnt = !retCnt;

                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }
        /// <summary>
        /// 外部設定したサーバフォルダの子フォルダのアクセス
        /// 例： RemotesubDir　＝/Data/の場合、
        /// 　ChildDir(/kk) の場合、/Data/kkのアクセス可
        /// </summary>
        /// <param name="strDirName"></param>
        public void ChildDir(String strDirName)
        {
            ChangeDir(strDirName,
                    FtpRemoteDirOption.CurWorkDirOpposite);
        }
        /// <summary>
        /// 親フォルダへ変更
        /// </summary>
        public void CdUpDir()
        {
            String curPath2 = "";
            //フォーマット
            this.StrRemoteAbsolutDir = FormatPathName(this.RemoteAbsolutDir);
            //既にトップフォルダの場合、
            if (this.StrRemoteAbsolutDir.Equals(@"/"))
            {
                return;
            }
            this.StrRemoteAbsolutDir = this.RemoteAbsolutDir.Substring(0,
                                      this.RemoteAbsolutDir.LastIndexOf(@"/"));
            //フォルダの変更
            ChangeDir(this.RemoteAbsolutDir, FtpRemoteDirOption.Absolutely);
            //URLより、相対の外部フォルダの変更
            curPath2 = this.TrcatePath(this.RemoteCurSubDir,
                                      this.RemoteHostUrlSubDir);
            //と同じ親パスの場合、
            if (this.RemoteAbsolutDir.StartsWith(curPath2)
                  && curPath2.Length <= this.RemoteAbsolutDir.Length
                  && curPath2.Length > 0)
            {
                //現時点の外部処理自動的に設定
                this.RemotesubDir = this.RemoteAbsolutDir.Substring(
                                               curPath2.Length);
                //フォーマット処理
                this.RemotesubDir = this.FormatPathName(this.RemotesubDir);
            }
            else
            {
                this.RemotesubDir = "";
            }
        }
        #endregion
        #region "リスト一覧の取得"
        /// <summary>
        /// ftpサーバとデータアクセスソケットセッションの作成
        /// </summary>
        /// <returns></returns>
        private Socket GetDataSocket()
        {
            //初期化
            Socket dataSocket = null;
            String strErrMsg = "";
            FtpDataTransMode curMode = FtpDataTransMode.Port;
            //初期化回数（二回Pasv、Port）
            int curCnt = 2;
            bool ret = false;
            //外部接続モードを指定した場合、
            if (this.PasvMode
                  != FtpDataTransMode.All)
            {
                curCnt = 1;
                curMode = this.PasvMode;
            }
            while (true)
            {
                try
                {
                    //初期化
                    ret = false;
                    switch (curMode)
                    {
                        case FtpDataTransMode.Pasv: //パッシュモードの場合、
                            dataSocket = this.GetPasvDataSocket();
                            //モードの設定
                            this.PasvMode = FtpDataTransMode.Pasv;
                            break;
                        case FtpDataTransMode.Port: //ポートモードの場合、
                            dataSocket = this.GetPortDataSocket();
                            //モードの設定
                            this.PasvMode = FtpDataTransMode.Port;
                            break;
                    }
                    //接続済みの場合、　
                    ret = true;
                    break;
                }
                catch (SocketException e)
                {
                    strErrMsg = String.Format("{0}:{1}", e.ErrorCode, e.Message);
                }
                catch (Exception e)
                {
                    strErrMsg = e.Message;
                }
                finally
                {
                    curCnt--;
                    curMode = (curMode == FtpDataTransMode.Port ?
                        FtpDataTransMode.Pasv : FtpDataTransMode.Port);
                    //まだ未接続の場合、
                    if (dataSocket == null
                           && curCnt <= 0
                           && !ret)
                    {
                        throw (new Exception(strErrMsg));
                    }

                }
            }
            return dataSocket;
        }

        [DllImport("kernel32.dll")]
        extern static short QueryPerformanceCounter(ref long x);
        [DllImport("kernel32.dll")]
        extern static short QueryPerformanceFrequency(ref long x);

        /// <summary>
        /// ポートモードの場合、
        /// ローカル監視ソケットから、
        /// データ送受信ソケットの取得
        /// </summary>
        /// <returns></returns>
        private void RestoreDataSocket(ref Socket dataSocket)
        {
            //Portモードの場合、
            if (this.socketListener != null
                && this.PasvMode == FtpDataTransMode.Port)
            {
                dataSocket = this.socketListener.Accept();
                this.socketListener.Close();
                this.socketListener = null;
            }
            //this.Mode == FtpDataTransMode.Pasv の場合、そのまま終了
            if (dataSocket == null)
            {
                throw new Exception("データ転送用ソケット取得失敗");
            }
        }
        /// <summary>
        /// フリーポートの取得
        /// </summary>
        /// <returns></returns>
        private int GetFreePort()
        {
            long kk = 0;
            int intFreePort = 0;
            Socket curSocket = null;
            IPEndPoint curendPoint = null;
            Random ran = new Random();
            QueryPerformanceCounter(ref  kk);
            try
            {
                 
                intFreePort = int.Parse(kk.ToString().PadLeft(14, '0').Substring(9));
                //1025より、小さい野場合、
                if (intFreePort <= 1025)
                {
                    intFreePort += 5000;
                }
                //ポートモードの場合、＞＝１０２５
                intFreePort = ran.Next(1025, System.Math.Abs((int)intFreePort));
                curSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                curendPoint = new IPEndPoint(IPAddress.Any, intFreePort);
                curSocket.Bind(curendPoint);
                curSocket.Close();
                curSocket = null;
            }
            catch
            {
                if (curSocket != null)
                {
                    curSocket = null;
                }
                intFreePort = GetFreePort();
            }
            //1000より、
            if (intFreePort <= 1024)
            {
                Thread.Sleep(100);
                intFreePort = GetFreePort();
            }
            return intFreePort;
        }
        /// <summary>
        /// ftpサーバとデータアクセスソケットセッションの作成
        /// </summary>
        /// <returns></returns>
        private Socket GetPortDataSocket()
        {
            Match curMath = null;
            String IpAdr = "";
            int IpPort = 0;
            IPEndPoint IpDataPoint = null;
            ///未接続の場合、サーバの接続
            if (!this.Connected)
            {
                this.Connect();
            }
            //端末のIPアドレスと利用できるポートの作成

            this.socketListener = new Socket(AddressFamily.InterNetwork,
                                            SocketType.Stream, ProtocolType.Tcp);

            //現状のIPパドレス
            if ((curMath =
                 this.RegexMatch(this.socketControl.LocalEndPoint.ToString(),
                                 new String[] { c_REGEX_STR_FTPADR })) == null)
            {
                throw new Exception(
                    String.Format(FtpCommon.ERR_GETLOCALIP.toString(),
                       this.socketControl.LocalEndPoint.ToString()));
            }
            //IPアドレス
            IpAdr = curMath.Groups[c_RegexAdr].Value;
            //妥当性チェック
            if (this.RegexMatch(IpAdr,
                    new String[] { c_REGEX_STR_IP }) == null)
            {
                throw new Exception(
                    String.Format(FtpCommon.ERR_GETLOCALIP.toString(),
                       this.socketControl.LocalEndPoint.ToString()));
            }
            //ポート：
            if (curMath.Groups[c_RegexPort].Value.Equals(""))
            {
                throw new Exception(
                     String.Format(FtpCommon.ERR_GETLOCALIP.toString(),
                        this.socketControl.LocalEndPoint.ToString()));
            }
            IpPort = GetFreePort();
            //監視開始
            IpDataPoint = new IPEndPoint(IPAddress.Parse(IpAdr), IpPort);
            this.socketListener.Bind(IpDataPoint);
            this.socketListener.Listen(10);

            //コマンドパラメータ作成
            IpAdr = IpAdr.Replace(".", ",") + "," + (IpPort / 256) + "," + (IpPort % 256);
            //コマンドの実施
            this.ExecCommand(FtpCommon.CMD_PORT, IpAdr);

            return null;
        }
        /// <summary>
        /// ftpサーバとデータアクセスソケットセッションの作成
        /// </summary>
        /// <returns></returns>
        private Socket GetPasvDataSocket()
        {
            Match curMath = null;
            String IpAdr = "";
            Socket dataSocket = null;
            int Iport = 0;


            ///未接続の場合、サーバの接続
            if (!this.Connected)
            {
                this.Connect();
            }
            //Port か、PASVかの考える 
            ///データ転送セッション要求コマンドの発行
            this.ExecCommand(FtpCommon.CMD_PASV);

            curMath =
                   this.RegexMatch(this.CmdRelayMsg,
                       new String[] { c_REGEX_STR_IP_TRANS });
            //未存在の場合、
            if (curMath == null
                || !curMath.Success)
            {
                this.DisConnect();
                this.blnErrFlag = false;
                throw new Exception(
                       String.Format(FtpCommon.ERR_GETDATASOCKETERR.toString(),
                                  this.CmdRelayMsg));
            }
            //Ipアドレス
            IpAdr = curMath.Groups[c_RegexAdr].Value.Replace(@",", @".");
            //ポート①
            if (!curMath.Groups[c_RegexPort].Value.Equals(""))
            {
                Iport += (int.Parse(curMath.Groups[c_RegexPort].Value) << 8);
            }//ポート②
            if (!curMath.Groups[c_RegexPort1].Value.Equals(""))
            {
                Iport += int.Parse(curMath.Groups[c_RegexPort1].Value);
            }
            //データソケット新規
            dataSocket = new Socket(AddressFamily.InterNetwork,
                                 SocketType.Stream,
                                          ProtocolType.Tcp);
            try
            {
                //サーバへソケット接続
                dataSocket.Connect(new IPEndPoint(IPAddress.Parse(IpAdr),
                                         Iport));

            }
            catch (SocketException e)
            {

                throw e;
            }
            catch (Exception e)
            {
                throw (new Exception(
                             String.Format(FtpCommon.ERR_CREATEDATASOCKETERR.toString(),
                                IpAdr, Iport, e.ToString())));
            }
            return dataSocket;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="curType"></param>
        /// <param name="strListFileName"></param>
        /// <returns></returns>
        public List<FtpFileInfo> GetCurDirList(FtpFileType curType,
                                                    String strMasrkFileName)
        {
            return GetCurDirList(@"/",
                        FtpRemoteDirOption.CurWorkDirOpposite,
                         new FtpFileType[] { curType }, strMasrkFileName);
        }
        /// <summary>
        /// タイプより、リスト取得
        /// </summary>
        /// <returns></returns>
        public List<FtpFileInfo> GetCurDirList(FtpFileType curType)
        {
            return GetCurDirList(new FtpFileType[] { curType }, "");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="curType"></param>
        /// <param name="strListFileName"></param>
        /// <returns></returns>
        public List<FtpFileInfo> GetCurDirList(FtpFileType[] curType,
                                                    String strMasrkFileName)
        {
            return GetCurDirList(@"/",
                        FtpRemoteDirOption.CurWorkDirOpposite,
                         curType, strMasrkFileName);
        }
        /// <summary>
        /// タイプより、リスト取得
        /// </summary>
        /// <returns></returns>
        public List<FtpFileInfo> GetCurDirList(FtpFileType[] curType)
        {
            return GetCurDirList(curType, "");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strFolderName">フォルダ名</param>
        /// <param name="CkType">上記指定されるフォルダのタイプ(IP相対、URL相対、絶対パスなど。。。</param>
        /// <param name="FileType">リスト可の種類(ファイル、フォルダ、全部）</param>
        /// <param name="strListFileName">リスト対象ファイル名（フォルダ名）</param>
        /// <returns></returns>
        public List<FtpFileInfo> GetCurDirList(String strFolderName,
                                               FtpRemoteDirOption CkType,
                                               FtpFileType[] FileType,
                                               String strMasrkFileName)
        {
            //データソケット
            Socket DataSocket = null;
            int curSize = 0, intCount = 0;
            Byte[] curGetBuffer = new byte[c_BLOCK_SIZE];
            //ショケット
            StringBuilder curMsgBuffer = new StringBuilder();
            string curTemp = "";
            //
            List<FtpFileInfo> fileList = new List<FtpFileInfo>();
            List<FtpFileType> curTypeList = new List<FtpFileType>(FileType);
            //再接続フラグ
            bool reTry = false;


            while (true)
            {
                try
                {
                    #region "メイン処理"
                    //フォルダの変更
                    this.ChangeDir(strFolderName, CkType);
                    //初期化
                    DataSocket = GetDataSocket();
                    //リストコマンド
                    this.ExecCommand(FtpCommon.CMD_LIST,
                                    new String[] { "-AL", strMasrkFileName });
                    //ポートモードの場合、データソケットの取得
                    RestoreDataSocket(ref DataSocket);
                    //データ情報の読取
                    while (true)
                    {
                        curSize =
                             DataSocket.Receive(curGetBuffer,
                                    curGetBuffer.Length, 0);
                        curMsgBuffer.Append(
                                  Encoding.ASCII.GetString(curGetBuffer, 0, curSize)
                                       );
                        if (curSize < curGetBuffer.Length)
                        {
                            intCount++;
                            Thread.Sleep(10);
                            if (intCount >= 2)
                            {
                                break;
                            }
                        }
                    }
                    this.ReadCmdReply();
                    //サーバ接続応答の確認  
                    CheckCmdStatus(ReturnCode(),
                          this.CmdRelayMsg, new FtpCodeMsg[] { FtpCommon.REP_226 });
                    if (DataSocket.Connected)
                    {
                        DataSocket.Close();//データソケットのクローズ
                    }
                    DataSocket = null;
                    //結果の整理
                    //結果存在の場合、
                    if (curMsgBuffer.Length > 0)
                    {
                        string strSplit = c_LF;
                        if (curMsgBuffer.ToString().Contains(c_CTRLF))
                        {
                            strSplit = c_CTRLF;
                        }
                        //\r\tで、分割処理
                        String[] curFileArry = curMsgBuffer.ToString().Split(strSplit.ToCharArray());

                        foreach (String curStr in curFileArry)
                        {
                            //空白以外の場合。、
                            if (curStr.Equals(""))
                            {
                                continue;
                            }
                            Match curMatch = RegexMatch(curStr, c_ListRegexFormats);
                            //存在以外の場合、
                            if (curMatch == null)
                            {
                                continue;
                            }
                            FtpFileInfo curFileInfo = new FtpFileInfo();
                            //名称 
                            curFileInfo.FileName = curMatch.Groups[c_RegexName].Value;
                            //Per
                            curFileInfo.Perssion = curMatch.Groups[c_RegexPer].Value;
                            //サイズ
                            long _size = 0;
                            Int64.TryParse(curMatch.Groups[c_RegexSize].Value, out _size);
                            curFileInfo.fileSize = _size;
                            //格納箇所
                            curFileInfo.FileRemotePath = this.RemoteAbsolutDir;
                            //タイプ
                            curFileInfo.FileType = FtpFileType.File;
                            if (curMatch.Groups[c_RegexDir].Value != ""
                                 && curMatch.Groups[c_RegexDir].Value != "-")
                            {
                                curFileInfo.FileType = FtpFileType.Directory;
                            }
                            //ファイル名が””の場合、
                            if (curFileInfo.FileName.Equals(""))
                            {
                                curFileInfo.FileType = FtpFileType.Other;
                            }
                            //更新日付
                            curTemp = curMatch.Groups[c_RegexTime].Value;
                            if (!curTemp.Equals(""))
                            {
                                try
                                {
                                    curFileInfo.ModTime = DateTime.Parse(curTemp);
                                }
                                catch
                                {

                                }
                            }
                            //フォルダの場合、月の処理を身につく
                            if (curFileInfo.FileType.Equals(FtpFileType.Directory)
                                && this.ChildFolderDown)
                            {
                                this.ChangeDir(curFileInfo.FileName, FtpRemoteDirOption.CurWorkDirOpposite);
                                //サブフォルダのリスト取得
                                curFileInfo.FileList = this.GetCurDirList(
                                                       strFolderName,
                                                                 CkType,
                                                                      FileType,
                                                                           strMasrkFileName);
                                this.CdUpDir();
                            }
                            //すべて以外の場合、
                            if (curTypeList.FindAll(delegate(FtpFileType key)
                            {
                                return (key.CompareTo(curFileInfo.FileType) == 0);
                            }).Count > 0
                                ||
                              curTypeList.FindAll(delegate(FtpFileType key)
                              {
                                  return (key.CompareTo(FtpFileType.All) == 0);
                              }).Count > 0
                                || curFileInfo.FileList.Count > 0)
                            {
                                fileList.Add(curFileInfo);
                            }
                        }
                    }
                    break;
                    #endregion
                }
                catch (Exception e)
                {
                    if (this.CmdRelayMsg.Equals("") && !reTry)
                    {
                        this.ExceptionSleepWaiting();
                        reTry = !reTry;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
            return fileList;
        }
        #endregion

        #region  "ファイルのダウンロード"
        /// <summary>
        /// 指定フォルダのファイルを全部ダウンロードすること
        /// </summary>
        /// <param name="strDirName">サーバフォルダ</param>
        /// <param name="CkType">格納フォルダの引数パラメータ</param>
        /// <param name="strListFileName">ファイル名(パスなど情報を含めない)</param>
        public int DownLoad()
        {

            return DownLoad(@"/",
                           FtpRemoteDirOption.CurWorkDirOpposite);
        }
        /// <summary>
        /// 指定フォルダのファイルを全部ダウンロードすること
        /// </summary>
        /// <param name="strDirName">サーバフォルダ</param>
        /// <param name="CkType">格納フォルダの引数パラメータ</param>
        /// <param name="strListFileName">ファイル名(パスなど情報を含めない)</param>
        public int DownLoad(String strRemoteDirName,
                                 FtpRemoteDirOption CkType
                                )
        {
            int curFileCont = 0;
            //指定フォルダ下のすべてダウンロード
            List<FtpFileInfo> curList = null;
            FtpFileType[] curftpType = new FtpFileType[2];
            //フォルダの変更
            this.ChangeDir(strRemoteDirName, CkType);

            curftpType[0] = FtpFileType.File;
            curftpType[1] = FtpFileType.File;
            //サブフォルダダウンロード可の場合、
            if (this.ChildFolderDown)
            {
                curftpType[1] = FtpFileType.Directory;
            }
            //当フォルダ下のすべてファイルリスト取得
            curList = this.GetCurDirList(curftpType);
            //ダウンロード処理を行う
            curFileCont += DownLoad(curList, this.LocalDir);
            return curFileCont;
        }
        /// <summary>
        /// GetList関数で取ったファイルリストより、ファイルをダウンロードすること
        /// </summary>
        /// <param name="fileList">ファイルリスト</param>
        /// <param name="strParamLocPath">ローカルフォルダ</param>
        public int DownLoad(
                             List<FtpFileInfo> fileList,
                             String strParamLocPath)
        {
            int curFileCont = 0;
            //空白以外の場合、
            if (fileList == null)
            {
                return 0;
            }
            //ダウンロード処理を行う
            foreach (FtpFileInfo curFIle in fileList)
            {
                switch (curFIle.FileType)
                {
                    case FtpFileType.File:
                        //ダウンロードエラーの場合、次のファイルのダウンロートへ行く
                        curFileCont += DownLoad(curFIle.FileRemotePath,
                                          curFIle.FilePathType,
                                          curFIle.FileName, strParamLocPath);
                        break;
                    case FtpFileType.Directory:
                        //フォルダの作成
                        if (!Directory.Exists(Path.Combine(strParamLocPath, curFIle.FileName)))
                        {
                            Directory.CreateDirectory(Path.Combine(strParamLocPath, curFIle.FileName));
                        }
                        //サブフォルダファイルダウンロードフラグ＝Trueの場合、
                        if (this.ChildFolderDown)
                        {
                            curFileCont += DownLoad(curFIle.FileList,
                                  Path.Combine(strParamLocPath, curFIle.FileName));
                        }
                        break;
                }

            }
            return curFileCont;
        }

        /// <summary>
        /// 指定フォルダから、指定ファイルのダウンロード
        /// </summary>
        /// <param name="strRemoteDirName">サーバフォルダ</param>
        /// <param name="CkType">格納フォルダの引数パラメータ</param>
        /// <param name="strFileName">ファイル名(パスなど情報を含めない)</param>
        /// <param name="strLocalSavePath">ローカルフォルダ</param>
        public int DownLoad(String strRemoteDirName,
                                 FtpRemoteDirOption CkType,
                                 String strFileName,
                                 String strLocalSavePath)
        {
            String curAbusultePath = "", curLocSavePath = "";
            //データソケット
            Socket DataSocket = null;
            //ファイルStream
            FileStream outFileStream = null;
            int curSize = 0;
            Byte[] curGetBuffer = new byte[c_BLOCK_SIZE];

            bool retry = false;
            //初期化
            if (strFileName.Equals(""))
            {
                return 0;
            }

            while (true)
            {
                try
                {
                    #region "メイン処理"
                    //初期化
                    DataSocket = this.GetDataSocket();
                    //サーバ向け物理パスの取得
                    curAbusultePath = this.GetRemoteAbsolutePath(strRemoteDirName, CkType);
                    //ダウンロード　
                    strLocalSavePath = (strLocalSavePath == null ?
                                               this.LocalDir : strLocalSavePath);
                    curLocSavePath = (strLocalSavePath.Length > 0 ?
                                                       strLocalSavePath :
                                       this.LocalDir);
                    curLocSavePath = (curLocSavePath.Length > 0 ?
                                                       curLocSavePath :
                                       System.IO.Directory.GetCurrentDirectory());
                    //フォルダが存在しない場合、
                    if (!Directory.Exists(curLocSavePath))
                    {
                        Directory.CreateDirectory(curLocSavePath);
                    }
                    //と同じファイルが存在の場合、削除する
                    if (File.Exists(Path.Combine(curLocSavePath, strFileName)))
                    {
                        //上書き可の場合、ローカルファイルを削除する
                        if (!this.DownLoadOverWrite)
                        {
                            File.Delete(Path.Combine(curLocSavePath, strFileName));
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    //ダウンロードコマンドの実施
                    this.ExecCommand(FtpCommon.CMD_RETR,
                                  this.TrcatePath(curAbusultePath, strFileName)
                                  );

                    //ポートモードの場合、データソケットの取得
                    RestoreDataSocket(ref DataSocket);
                    //ファイルStream
                    outFileStream = new FileStream(Path.Combine(curLocSavePath, strFileName),
                                                   FileMode.Create);
                    while (true)
                    {
                        curSize =
                             DataSocket.Receive(curGetBuffer,
                                          curGetBuffer.Length, 0);
                        outFileStream.Write(curGetBuffer, 0, curSize);
                        //存在しない場合、
                        if (curSize <= 0)
                        {
                            break;
                        }
                    }
                    if (outFileStream != null)
                    {
                        outFileStream.Close();
                        outFileStream = null;
                    }
                    this.ReadCmdReply();
                    //サーバ接続応答の確認  
                    CheckCmdStatus(ReturnCode(),
                          this.CmdRelayMsg, new FtpCodeMsg[] { FtpCommon.REP_226, 
                                                        FtpCommon.REP_250});
                    if (DataSocket.Connected)
                    {
                        DataSocket.Close();//データソケットのクローズ
                    }
                    DataSocket = null;
                    break;
                    #endregion "メイン処理"
                }
                catch (Exception e)
                {
                    //接続エラーの場合、リスト
                    if (this.CmdRelayMsg.Equals("") && !retry)
                    {
                        this.ExceptionSleepWaiting();
                        retry = !retry;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
            return 1;
        }
        #endregion


        #region "フィアルアップロード"
        /// <summary>
        /// 当前指定したフォルダへファイルのアップロード
        /// </summary>
        /// <param name="strLocalFileFullName">ローカルファイル名(ディレクトリを含む）</param>
        public void Upload(String strLocalFileFullName)
        {
            FileInfo curFile = new FileInfo(strLocalFileFullName);
            //アップロード
            Upload(@"/",
                    FtpRemoteDirOption.CurWorkDirOpposite,
                   strLocalFileFullName,
                   curFile.Name);
        }
        /// <summary>
        /// 当前指定したフォルダへファイルのアップロード
        /// </summary>
        /// <param name="strLocalFileFullName">ローカルファイル名(ディレクトリを含む）</param>
        public void Upload(String strLocalFileFullName,
                           String strRemoteFileName)
        {

            FileInfo curFile = new FileInfo(strLocalFileFullName);
            //アップロード
            Upload(@"/",
                    FtpRemoteDirOption.CurWorkDirOpposite,
                   strLocalFileFullName,
                   strRemoteFileName);
        }
        /// <summary>
        /// サーバの指定フォルダへファイルのアップロード
        /// </summary>
        /// <param name="strRemoteDirName">サーバのフォルダ名(親パスなど情報を含める)</param>
        /// <param name="CkType">strRemoteDirNameの命名種類</param> 
        /// <param name="strLocalFileFullName">ローカルファイル名(ディレクトリを含む）</param>
        public void Upload(String strRemoteDirName,
                           FtpRemoteDirOption CkType,
                           String strLocalFileFullName
                          )
        {
            FileInfo curFile = new FileInfo(strLocalFileFullName);
            Upload(strRemoteDirName,
                  CkType,
                  strLocalFileFullName,
                   curFile.Name);
        }
        /// <summary>
        /// サーバの指定フォルダへファイルのアップロード
        /// </summary>
        /// <param name="strRemoteDirName">サーバのフォルダ名(親パスなど情報を含める)</param>
        /// <param name="CkType">strRemoteDirNameの命名種類</param>
        /// <param name="strLocalFileFullName">ローカルファイル名(ディレクトリを含む）</param>
        /// <param name="strRemoteFileName">サーバへ保存名称</param>
        public void Upload(String strRemoteDirName,
                           FtpRemoteDirOption CkType,
                           String strLocalFileFullName,
                           String strRemoteFileName
                           )
        {

            String curAbusultePath = "";
            //データソケット
            Socket DataSocket = null;
            //ファイルStream
            FileStream outFileStream = null;
            bool retry = false;

            int curSize = 0;
            Byte[] curGetBuffer = new byte[c_BLOCK_SIZE];

            //初期化
            if (strLocalFileFullName.Equals("")
                || !File.Exists(strLocalFileFullName)
                || strRemoteFileName.Equals(""))
            {
                return;
            }
            while (true)
            {
                try
                {
                    #region "メイン処理"
                    //データソケットの取得
                    DataSocket = this.GetDataSocket();
                    //アップロードフォルダ取得
                    curAbusultePath = this.GetRemoteAbsolutePath(strRemoteDirName, CkType);
                    //現状フォルダ以外の場合、
                    if (!curAbusultePath.Equals(this.RemoteAbsolutDir))
                    {
                        //this.ChangeDir(strRemoteDirName, CkType);
                    }
                    //アップロードコマンドの発行
                    this.ExecCommand(FtpCommon.CMD_STOR,
                          this.TrcatePath(curAbusultePath, strRemoteFileName));

                    //ポートモードの場合、データソケットの取得
                    RestoreDataSocket(ref DataSocket);
                    //ファイルの読取
                    outFileStream = new FileStream(strLocalFileFullName,
                                                         FileMode.Open,
                                                         FileAccess.Read);

                    //アップロード処理を行う
                    while (true)
                    {
                        curSize = outFileStream.Read(curGetBuffer,
                                                                0,
                                                                curGetBuffer.Length);
                        //ゼロバイトの場合、終了
                        if (curSize <= 0)
                        {
                            break;
                        }
                        //データ転送
                        DataSocket.Send(curGetBuffer, curSize, 0);
                    }
                    if (outFileStream != null)
                    {
                        outFileStream.Close();
                        outFileStream = null;
                    }
                    if (DataSocket.Connected)
                    {
                        DataSocket.Close();
                        DataSocket = null;
                    }
                    this.ReadCmdReply();
                    //サーバ接続応答の確認  
                    CheckCmdStatus(ReturnCode(),
                          this.CmdRelayMsg, new FtpCodeMsg[] { FtpCommon.REP_226, 
                                                        FtpCommon.REP_250});
                    break;
                    #endregion
                }
                catch (Exception e)
                {
                    if (this.CmdRelayMsg.Equals("") && !retry)
                    {
                        this.ExceptionSleepWaiting();
                        retry = !retry;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }

        #endregion
        #region  "ファイル名の変更"
        /// <summary>
        /// サーバファイル名の変更
        /// </summary>
        /// <param name="strReMoteDir">サーバフォルダ</param>
        /// <param name="ckDirOpt">サーバフォルダの属性</param>
        /// <param name="strOldeFileName">変更前名称（ディレクトリを含めない）</param>
        /// <param name="strNewFIleName">変更後名称（ディレクトリを含めない）</param>
        /// <returns></returns>
        public void ReName(String strOldeFileName,
                                String strNewFIleName)
        {
            //外部で指定したフォルダのファイル名変更
            this.ReName(@"/",
                    FtpRemoteDirOption.CurWorkDirOpposite,
                           strOldeFileName,
                           strNewFIleName);
        }
        /// <summary>
        /// サーバファイル名の変更
        /// </summary>
        /// <param name="strReMoteDir">サーバフォルダ</param>
        /// <param name="ckDirOpt">サーバフォルダの属性</param>
        /// <param name="strOldeFileName">変更前名称（ディレクトリを含めない）</param>
        /// <param name="strNewFIleName">変更後名称（ディレクトリを含めない）</param>
        /// <returns></returns>
        public void ReName(
                                String strReMoteDir,
                                FtpRemoteDirOption ckDirOpt,
                                String strOldeFileName,
                                String strNewFIleName)
        {
            String curAuDir = "";
            //未接続の場合。
            if (!this.Connected)
            {
                this.Connect();
            }
            //初期化
            if (strOldeFileName.Equals("")
                 || strNewFIleName.Equals("")
                 || strOldeFileName.Contains(@"/")
                || strNewFIleName.Contains(@"/"))
            {
                throw new Exception(FtpCommon.ERR_RENAMEFILE_ISNULL.toString());
            }
            //ディレクトリ取得
            curAuDir = this.GetRemoteAbsolutePath(strReMoteDir, ckDirOpt);
            //RNFRコマンド実施
            this.ExecCommand(FtpCommon.CMD_RNFR, this.TrcatePath
                               (curAuDir, strOldeFileName)
                               );
            //ファイル存在しない場合、そのまま終了
            if (this.ReturnCode().Equals(FtpCommon.REP_550.Code))
            {
                return;
            }
            //RNTOコマンド実施
            this.ExecCommand(FtpCommon.CMD_RNTO, this.TrcatePath
                               (curAuDir, strNewFIleName));
        }
        #endregion

        #region  "フィアルの削除"
        /// <summary>
        /// 現在作業フォルダのファイル野削除
        /// </summary> 
        /// <param name="FileName">変更後名称（ディレクトリを含めない）</param>
        public void Delete(String FileName)
        {

            this.Delete(@"/",
                    FtpRemoteDirOption.CurWorkDirOpposite,
                      FileName);
        }
        /// <summary>
        /// サーバファイル名の変更
        /// </summary>
        /// <param name="strReMoteDir">サーバフォルダ</param>
        /// <param name="ckDirOpt">サーバフォルダの属性</param> 
        /// <param name="FileName">変更後名称（ディレクトリを含めない）</param>
        public void Delete(String strReMoteDir,
                                FtpRemoteDirOption ckDirOpt,
                                String FileName)
        {
            String curAuDir = "";
            //未接続の場合。
            if (!this.Connected)
            {
                this.Connect();
            }
            //初期化
            if (FileName.Equals("") || FileName.Contains(@"/"))
            {
                throw new Exception(FtpCommon.ERR_PARAM_ISNULL.toString());
            }
            //ディレクトリ取得
            curAuDir = this.GetRemoteAbsolutePath(strReMoteDir, ckDirOpt);
            //RNFRコマンド実施
            this.ExecCommand(FtpCommon.CMD_DELE, this.TrcatePath
                               (curAuDir, FileName)
                               );

        }
        #endregion

        #region "フォルダの新規"

        /// <summary>
        /// 現行アクセスしているサーバフォルダ中に、新たなフォルダの新規
        /// </summary> 
        /// <param name="strNewDirName">新規フォルダ名</param>
        public void MkDir(String strNewDirName)
        {

            this.MkDir(@"/",
                    FtpRemoteDirOption.CurWorkDirOpposite,
                     strNewDirName);
        }
        /// <summary>
        /// 指定サーバフォルダの中に、新たなフォルダの新規
        /// </summary>
        /// <param name="strReMoteParDir">親フォルダ</param>
        /// <param name="ckDirOpt">親フォルダのアクセス属性</param>
        /// <param name="strNewDirName">新規フォルダ名</param>
        public void MkDir(String strReMoteParDir,
                            FtpRemoteDirOption ckDirOpt,
                            String strNewDirName)
        {
            String curAuDir = "";
            //未接続の場合。
            if (!this.Connected)
            {
                this.Connect();
            }
            //初期化
            if (strNewDirName.Equals("") || strNewDirName.Contains(@"/"))
            {
                throw new Exception(FtpCommon.ERR_PARAM_ISNULL.toString());
            }
            //ディレクトリ取得
            curAuDir = this.GetRemoteAbsolutePath(strReMoteParDir, ckDirOpt);
            //フォルダのチェック
            this.ExecCommand(FtpCommon.CMD_CWD_MKD,
                             this.TrcatePath(curAuDir,
                                             strNewDirName));
            //存在の場合、
            if (this.ReturnCode().Equals(FtpCommon.REP_250))
            {
                //親フォルダへ戻る
                this.CdUpDir();
                return;
            }
            //MKDコマンド実施
            this.ExecCommand(FtpCommon.CMD_MKD, this.TrcatePath
                               (curAuDir, strNewDirName)
                               );
        }

        #endregion

        #region "ディレクトリの削除"
        /// <summary>
        /// 指定サーバフォルダの中に、新たなフォルダの新規
        /// </summary>
        /// <param name="strReMoteParDir">親フォルダ</param>
        /// <param name="ckDirOpt">親フォルダのアクセス属性</param>
        /// <param name="strNewDirName">新規フォルダ名</param>
        public void DelDir(String strDirName)
        {
            //フォルダの削除
            this.DelDir(@"/",
                    FtpRemoteDirOption.CurWorkDirOpposite, strDirName);
        }
        /// <summary>
        /// 指定サーバフォルダの中に、新たなフォルダの新規
        /// </summary>
        /// <param name="strReMoteParDir">親フォルダ</param>
        /// <param name="ckDirOpt">親フォルダのアクセス属性</param>
        /// <param name="strNewDirName">新規フォルダ名</param>
        public void DelDir(String strReMoteParDir,
                            FtpRemoteDirOption ckDirOpt,
                            String strDirName)
        {
            String curAuDir = "";
            bool blnChildDown = this.ChildFolderDown;
            List<FtpFileInfo> curList = null;
            //未接続の場合。
            if (!this.Connected)
            {
                this.Connect();
            }
            //初期化
            if (strDirName.Equals("") || strDirName.Contains(@"/"))
            {
                throw new Exception(FtpCommon.ERR_PARAM_ISNULL.toString());
            }
            //ディレクトリ取得
            curAuDir = this.GetRemoteAbsolutePath(strReMoteParDir, ckDirOpt);
            try
            {
                //RNFRコマンド実施
                this.ChangeDir(this.TrcatePath
                        (curAuDir, strDirName),
                        FtpRemoteDirOption.Absolutely);
                //子フォルダのリストしないように
                this.ChildFolderDown = false;
                //当該フォルダ↓のリスリスト取得
                curList = this.GetCurDirList(new FtpFileType[]{
                                                FtpFileType.File,
                                                FtpFileType.Directory});

                this.ChildFolderDown = blnChildDown;
                if (curList != null && curList.Count > 0)
                {
                    foreach (FtpFileInfo curFile in curList)
                    {
                        switch (curFile.FileType)
                        {
                            case FtpFileType.File://ファイルの場合、
                                this.Delete(curFile.FileRemotePath,
                                     curFile.FilePathType,
                                     curFile.FileName);
                                break;
                            case FtpFileType.Directory://ファイルの場合、
                                this.DelDir(curFile.FileRemotePath,
                                     curFile.FilePathType,
                                     curFile.FileName);
                                break;
                        }
                    }
                }
                //親フォルダへ戻る
                this.CdUpDir();
            }
            catch
            {
                return;
            }
            //RNFRコマンド実施
            this.ExecCommand(FtpCommon.CMD_RMD, this.TrcatePath
                               (curAuDir, strDirName)
                               );
        }
        #endregion


        #region  "その他コマンド"

        /// <summary>
        /// 空白処理
        /// </summary>
        public void Noop()
        {
            //未接続の場合、
            if (!this.Connected)
            {
                this.Connect();
            }
            this.ExecCommand(FtpCommon.CMD_NOOP);
        }

        #region "ファイルのサイズ"

        /// <summary>
        /// 指定ファイルサイズ取得(単位：バイト)
        /// </summary>
        /// <param name="strFileName"></param>
        /// <returns></returns>
        public int Size(
                            String strFileName)
        {
            return Size(@"/",
                    FtpRemoteDirOption.CurWorkDirOpposite, strFileName);
        }
        /// <summary>
        /// 指定ファイルのサイズ取得(単位：バイト)
        /// </summary>
        /// <param name="strReMoteParDir">フォルダ</param>
        /// <param name="ckDirOpt">ディレクトリ属性</param>
        /// <param name="strFileName">ファイル名</param>
        /// <returns></returns>
        public int Size(String strReMoteParDir,
                            FtpRemoteDirOption ckDirOpt,
                            String strFileName)
        {
            String curAuDir = "";
            Match curMatch = null;
            int curSize = 0;
            //未接続の場合。
            if (!this.Connected)
            {
                this.Connect();
            }
            //初期化
            if (strFileName.Equals("") || strFileName.Contains(@"/"))
            {
                throw new Exception(FtpCommon.ERR_PARAM_ISNULL.toString());
            }
            //ディレクトリ取得
            curAuDir = this.GetRemoteAbsolutePath(strReMoteParDir, ckDirOpt);
            //フォルダのチェック
            this.ExecCommand(FtpCommon.CMD_SIZE,
                             this.TrcatePath(curAuDir,
                                             strFileName));

            curMatch = this.RegexMatch(this.CmdRelayMsg,
                              new String[] {
                               @"\s(?<Size>\d+)"});
            if (curMatch != null)
            {
                curSize = int.Parse(curMatch.Groups["Size"].Value);
            }
            return curSize;
        }
        #endregion

        #region "ファイルの最新変更時間"
        /// <summary>
        /// 指定ファイルの更新日付取得
        /// </summary>
        /// <param name="strFileName"></param>
        /// <returns></returns>
        public DateTime ModTime(
                            String strFileName)
        {
            return ModTime(@"/",
                    FtpRemoteDirOption.CurWorkDirOpposite, strFileName);
        }
        /// <summary>
        /// 指定ファイルの更新日付取得
        /// </summary>
        /// <param name="strReMoteParDir">フォルダ</param>
        /// <param name="ckDirOpt">ディレクトリ属性</param>
        /// <param name="strFileName">ファイル名</param>
        /// <returns></returns>
        public DateTime ModTime(String strReMoteParDir,
                            FtpRemoteDirOption ckDirOpt,
                            String strFileName)
        {
            String curAuDir = "";
            Match curMatch = null;
            DateTime curModTime = default(DateTime);
            //未接続の場合。
            if (!this.Connected)
            {
                this.Connect();
            }
            //初期化
            if (strFileName.Equals("") || strFileName.Contains(@"/"))
            {
                throw new Exception(FtpCommon.ERR_PARAM_ISNULL.toString());
            }
            //ディレクトリ取得
            curAuDir = this.GetRemoteAbsolutePath(strReMoteParDir, ckDirOpt);
            //フォルダのチェック
            this.ExecCommand(FtpCommon.CMD_MDTM,
                             this.TrcatePath(curAuDir,
                                             strFileName));

            //コマンド実行OKの場合、999　YYYYMMDDhhmmss
            curMatch = this.RegexMatch(this.CmdRelayMsg,
                              new String[] {
                               @"\s+(?<Year>\d{4})(?<Month>\d{2})(?<Day>\d{2})(?<Time>\d{2})(?<Min>\d{2})(?<Sec>\d{2})"});
            if (curMatch != null)
            {
                try
                {
                    //年
                    curAuDir = curMatch.Groups["Year"].Value;
                    //月
                    curAuDir += @"/" + curMatch.Groups["Month"].Value;
                    //日
                    curAuDir += @"/" + curMatch.Groups["Day"].Value;
                    //時
                    curAuDir += " " + curMatch.Groups["Time"].Value;
                    //分
                    curAuDir += ":" + curMatch.Groups["Min"].Value;
                    //秒
                    curAuDir += ":" + curMatch.Groups["Sec"].Value;
                    curModTime = DateTime.Parse(curAuDir);
                }
                catch
                {
                }
            }
            return curModTime;
        }
        #endregion
        #endregion

        #region "ヘルプ"
        /// <summary>
        /// ヘルプ情報
        /// </summary>
        /// <returns></returns>
        public String Help()
        {
            //未接続の場合、
            //未接続の場合。
            if (!this.Connected)
            {
                this.Connect();
            }
            //
            this.ExecCommand(FtpCommon.CMD_HELP);


            return this.CmdRelayFullMsg;
        }
        #endregion

        #region "共通関数"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strValue"></param>
        /// <param name="RegexStr"></param>
        /// <returns></returns>
        private Match RegexMatch(String strValue, String[] RegexStr)
        {

            //Regex curRegex;
            Match curMatch = null;

            //初期化
            if (RegexStr != null)
            {
                foreach (String curStr in RegexStr)
                {
                    if (!curStr.Equals(""))
                    {
                        //curRegex = new Regex();
                        curMatch = Regex.Match(strValue, curStr);
                        //OK
                        if (curMatch.Success)
                        {
                            return curMatch;
                        }
                    }
                }
            }
            return null;
        }
        #endregion
    }
}
