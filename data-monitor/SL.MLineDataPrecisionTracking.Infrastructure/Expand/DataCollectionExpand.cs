using SL.MLineDataPrecisionTracking.Models.Domain;
using SL.MLineDataPrecisionTracking.Models.Dtos;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Expand
{
    public static class DataCollectionExpand
    {
        static Type _dfStringtype = typeof(string);
        public static Result<object> SugarColumnReflectAssign(
       Result<List<DevPlcPointDto>> readValue,
       Type dataModelType
   )
        {
            object t = Activator.CreateInstance(dataModelType);
            var props = dataModelType.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                var attr = prop.GetCustomAttribute<SugarColumn>();
                if (attr?.ColumnDescription == null)
                {
                    continue;
                }
                try
                {
                    var readInfo = readValue.Data.FirstOrDefault(x =>
                        x.PointName == attr.ColumnDescription
                    );
                    if (readInfo == null)
                    {
                        if (_dfStringtype == prop.PropertyType)
                        {
                            prop.SetValue(t, "");
                        }
                    }
                    else
                    {
                        if (readInfo.Length == 1)
                        {
                            //if (readInfo.ReadFormula == null || readInfo.ReadFormula.Length == 0)
                            //{
                            prop.SetValue(t, readInfo.Value[0].ToString());
                            //}
                            //else
                            //{
                            //    var val = readInfo.ReadFormula.StringCompute(
                            //        readInfo.Value[0].ToString()
                            //    );
                            //    prop.SetValue(t, val.ToString());
                            //}
                        }
                        else
                        {
                            if (readInfo.DataType == TypeCode.String)
                            {
                                var val = string.Concat(readInfo.Value);
                                prop.SetValue(t, val);
                            }
                            else
                            {
                                prop.SetValue(t, readInfo.Value.ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Result<object>.Fail($"反射数据失败:{ex.Message}");
                }
            }

            return Result<object>.Success(t);
        }
    }
}
