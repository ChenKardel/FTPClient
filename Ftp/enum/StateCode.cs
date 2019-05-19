using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ftp.@enum
{
    /// <summary>
    /// 状态码
    /// </summary>
    public enum StateCode
    {
        Reset = 202,
        ReadyInNMinutes = 120,
        ReadyConnection = 125,
        OpenConnection = 150,
        Success = 200,
        Fail = 202, 
        SystemState = 211,
        DirectoryState = 212,
        FileState = 213,
        HelpInfo = 214,
        NameSystemType = 215,
        PassiveMode227 = 227,
        ChangeDir250 = 250,
        NewUserReady = 220,
        Close = 221,
        NoTransport = 225,
        UserLogin = 230,
        UsernameCorrectNeedPwd = 331,
        LoginNeedAccountInfo = 332,
    }


}
