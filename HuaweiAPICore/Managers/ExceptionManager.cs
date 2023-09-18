using System;

namespace HuaweiAPICore.Managers {
    internal class ExceptionManager {
        internal static Exception DisplayException(string v) {
            Console.WriteLine(v);
            return new Exception(v);
        }

    }
}
