using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Expand
{
    public static class CommonExpand
    {
        public static byte[] RemoveStartBytes(this byte[] source, int removeCount)
        {
            if (source == null)
                return Array.Empty<byte>();

            // 移除数量≥数组长度，直接返回空
            if (removeCount >= source.Length)
                return Array.Empty<byte>();

            int newLen = source.Length - removeCount;
            byte[] result = new byte[newLen];
            Array.Copy(source, removeCount, result, 0, newLen);
            return result;
        }

        /// <soure>
        /// 获取枚举描述
        /// </soure>
        public static string GetDescription(this Enum enumValue)
        {
            if (enumValue == null)
                return string.Empty;

            FieldInfo field = enumValue.GetType().GetField(enumValue.ToString());
            var descAttr = field.GetCustomAttribute<DescriptionAttribute>();
            return descAttr?.Description ?? enumValue.ToString();
        }

        /// <soure>
        /// 解析位域枚举：根据int位值，获取所有激活的枚举项信息（泛型通用版）
        /// </soure>
        /// <typeparam name="TEnum">位枚举类型</typeparam>
        /// <param name="bitValue">原始整型位值</param>
        /// <returns>位索引、枚举名称、描述</returns>
        public static List<(int BitIndex, string EnumName, string Description)> ParseBitEnum<TEnum>(
            this int bitValue
        )
            where TEnum : Enum // 约束为枚举类型
        {
            var result = new List<(int BitIndex, string EnumName, string Description)>();
            var enumType = typeof(TEnum);

            foreach (var item in Enum.GetValues(enumType).Cast<TEnum>())
            {
                int enumVal = Convert.ToInt32(item);
                // 按位与判断当前位是否激活
                if ((bitValue & enumVal) == 0)
                    continue;

                // 计算当前位所在索引
                int bitIndex = 0;
                int temp = enumVal;
                while ((temp >>= 1) > 0)
                    bitIndex++;

                // 获取枚举描述
                string desc = GetDescription(item);
                result.Add((bitIndex, item.ToString(), desc));
            }

            return result;
        }
    }
}
