using McpXLib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Expand
{
    public static class McProtocolExpand
    {
        public static Prefix ToPrefix(this string prefix)
        {
            if (Enum.TryParse<Prefix>(prefix, out Prefix result))
            {
                return result;
            }
            else
            {
                throw new NotSupportedException($"不支转换类型: {prefix}");
            }
        }

        public static bool IsHexDevice(this Prefix prefix)
        {
            if (
                prefix != Prefix.X
                && prefix != Prefix.Y
                && prefix != Prefix.B
                && prefix != Prefix.W
                && prefix != Prefix.SB
                && prefix != Prefix.SW
                && prefix != Prefix.DX
            )
                return prefix == Prefix.DY;
            return true;
        }

        public static TypeCode ToTypeCode(this string type)
        {
            if (Enum.TryParse<TypeCode>(type, out TypeCode result))
            {
                return result;
            }
            else
            {
                throw new NotSupportedException($"不支持解析类型: {result}");
            }
        }

    }
}
