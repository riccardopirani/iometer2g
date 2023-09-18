using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;

namespace HuaweiAPICore.Managers {
    internal class ExceptionManager {
        internal static Exception DisplayException(string v) {
            Console.WriteLine(v);
            return new Exception(v);
        }

    }
}
