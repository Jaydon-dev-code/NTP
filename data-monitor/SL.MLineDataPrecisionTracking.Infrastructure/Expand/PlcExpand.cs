using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Expand
{
    public static class PlcExpand
    {
        public static int GetTypeOfShortOffset(this TypeCode type)
        {
            int result;
            switch (type)
            {
                case TypeCode.Boolean:
                    result = 1;
                    break;
                case TypeCode.String:
                    result = 1;
                    break;
                case TypeCode.Int16:
                    result = 1;
                    break;
                case TypeCode.UInt16:
                    result = 1;
                    break;
                case TypeCode.Int32:
                    result = 2;
                    break;
                case TypeCode.UInt32:
                    result = 2;
                    break;
                case TypeCode.Single:
                    result = 2;
                    break;
                case TypeCode.Double:
                    result = 4;
                    break;
                case TypeCode.Int64:
                    result = 4;
                    break;
                default:
                    throw new NotSupportedException($"不支持的数据类型: {type}");
            }
            return result;
        }

  
    }
}
