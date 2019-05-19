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
        DataConnectionOpen125 = 125,
        PassiveMode227 = 227,
        ChangeDir250 = 250,
        NoSuchFileDirectory550 = 550,
    }


}
