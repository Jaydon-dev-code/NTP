using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Models.Domain
{
    public class PointData<T1, T2>
    {
        // 无参构造
        public PointData()
        {
        }

        // 带参构造
        public PointData(T1 x, T2 y)
        {
            X = x;
            Y = y;
        }

        public T1 X
        {
            get; set;
        }
        public T2 Y
        {
            get; set;
        }

      
    }
}
