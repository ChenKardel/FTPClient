using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ftp.error
{
    public class NoConnectionException: Exception
    {
        public override string Message { get; }
    }
}
