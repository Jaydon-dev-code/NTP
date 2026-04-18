using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.Formula.Functions;
using SL.MLineDataPrecisionTracking.Infrastructure.PLCCommunication;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Entities;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Core.Services
{
    public class B_ProLineDataCollectionService : ProLineDataCollectionServiceAbstract<Tb_LineB>
    {
        Tb_LineBRepository _lineBRepository;
        Tb_LineARepository _lineARepository;
        Tb_LineSummaryRepository _lineSummaryRepository;
        Tb_ModelNoToNameRepository _modelNoToNameRepository;

        public B_ProLineDataCollectionService(
            Tb_EquipmentRepository equipmentRepositor,
            McpCommunication mcp,
            Tb_LineBRepository tb_LineBRepository,
            Tb_LineARepository tb_LineARepository,
            Tb_LineSummaryRepository tb_LineSummaryRepository,
            Tb_ModelNoToNameRepository tb_ModelNoToNameRepository
        )
            : base(equipmentRepositor, mcp)
        {
            _lineBRepository = tb_LineBRepository;
            _lineARepository = tb_LineARepository;
            _lineSummaryRepository = tb_LineSummaryRepository;
            _modelNoToNameRepository = tb_ModelNoToNameRepository;
        }

        protected override string _lineName { get; set; } = "B线";

        protected override async Task<bool> InsterCollectionData(Tb_LineB data)
        {
            var aLineInfo = await _lineARepository.QueryableFirstAsync(
                x => x.TrayNoA == data.LineATrayNo,
                o => o.RecordTime
            );
            if (aLineInfo == null)
            {
                return await _lineBRepository.InsertableAsync(data) > 0;
            }
            else
            {
                Tb_LineSummary tb_LineSummary = new Tb_LineSummary();

                ABToSummary(aLineInfo, data, tb_LineSummary, new List<string>() { "A线托盘编号" });
                var models = await _modelNoToNameRepository.QueryabletAsync(x => true);

                tb_LineSummary.ModelNo = data.ModelNoB;
                var modelNameB = models
                    .FirstOrDefault(x => x.ModelNo == tb_LineSummary.ModelNo)
                    ?.ModelName;
                tb_LineSummary.ModelName= modelNameB== null ? "" : modelNameB;

            
                return await _lineSummaryRepository.InsertableAsync(tb_LineSummary) > 0;
            }
        }

        /// <summary>
        /// 把 Tb_LineA + Tb_LineB 的值 赋值给 Tb_LineSummary
        /// 只赋值带 [SugarColumn] 的字段
        /// 支持过滤字段
        /// </summary>
        public void ABToSummary(
            object sourceA,
            object sourceB,
            object summary,
            List<string> ignoreFields = null
        )
        {
            if (ignoreFields == null)
            {
                ignoreFields = new List<string>();
            }

            // 拿到汇总类所有带 SugarColumn 的属性
            var summaryProperties = summary
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.IsDefined(typeof(SugarColumn), true))
                .ToList();

            foreach (var prop in summaryProperties)
            {
                var fieldName = prop.Name;

                // 过滤字段
                if (
                    ignoreFields.Any(ig => ig.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                )
                    continue;

                // 先从 A 类取值
                object value = GetValue(sourceA, fieldName);

                // A 类没有，再从 B 类取值
                if (value == null)
                {
                    value = GetValue(sourceB, fieldName);
                }

                // 赋值给汇总类
                if (value != null)
                {
                    prop.SetValue(summary, value);
                }
            }
        }

        /// <summary>
        /// 从对象中根据字段名取值
        /// </summary>
        private static object GetValue(object obj, string fieldName)
        {
            if (obj == null)
                return null;

            var prop = obj.GetType().GetProperty(fieldName);
            return prop?.CanRead == true ? prop.GetValue(obj) : null;
        }
    }
}
