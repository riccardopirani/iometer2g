using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPICore.Models
{
    public sealed class GetDevFiveMinutesRequest
    {
        public Int64 devId { get; set; }
        public Int64 collectTime { get; set; }
        //public DataItemMapResponse dataItemMap { get; set; }

        public Dictionary<string,object> dataItemMap { get; set; }

        public DateTime? util_date { get; set; }

        public GetDevFiveMinutesRequest()
        {
            dataItemMap = new Dictionary<string, object>();
        }
        public void Dispose()
        {
            _Dispose();
        }
        void _Dispose()
        {
            if (dataItemMap != null)
                dataItemMap.Clear();
            dataItemMap = null;
        }
        ~GetDevFiveMinutesRequest()
        {
            _Dispose();
        }
    }
}
