using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.SymbolStore;
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
    public class A_ProLineDataCollectionService : ProLineDataCollectionServiceAbstract
    {
        protected override string _lineName { get; set; } = "A线";

        public A_ProLineDataCollectionService(
            Tb_EquipmentRepository equipmentRepositor,
            McpCommunication mcp
        )
            : base(equipmentRepositor, mcp) { }

        protected override async Task<bool> CollectionData()
        {
            var readValue = await _mcp.ReadAsync(_lineReadPlcInfo);
            if (readValue.IsSuccess is false)
            {
                return false;
            }
            Tb_LineA tb_LineA = new Tb_LineA();
            var props = typeof(Tb_LineA).GetProperties();
            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttribute<SugarColumn>();
                var columnDescription = attr?.ColumnDescription;
                if (attr?.ColumnDescription == null || attr?.ColumnDescription is null)
                {
                    continue;
                }
                var readInfo = readValue.Data.First(x => x.PointName == attr?.ColumnDescription);

                if (readInfo.Length == 1) { }
                prop.SetValue(tb_LineA, readInfo.Value);
            }

            // var trayNo= readValue.Data.First(x => x.PointName == "托盘号");
            // var shieldStation=  readValue.Data.First(x => x.PointName == "屏蔽工位");
            //var ngCode=   readValue.Data.First(x => x.PointName == "NG代码");
            // var smallInnerRingSortingData=  readValue.Data.First(x => x.PointName == "小内圈分选数据");
            //  var outerFlangeSortingData= readValue.Data.First(x => x.PointName == "外法兰分选数据");
            //   readValue.Data.First(x => x.PointName == "内法兰分选数据");
            //   readValue.Data.First(x => x.PointName == "A面钢球组差");
            //   readValue.Data.First(x => x.PointName == "B面钢球组差");
            //   readValue.Data.First(x => x.PointName == "B面钢球注脂量");
            //   readValue.Data.First(x => x.PointName == "密封圈注脂量");
            //   readValue.Data.First(x => x.PointName == "A面钢球注脂量");
            //   readValue.Data.First(x => x.PointName == "密封圈压装力");
            //   readValue.Data.First(x => x.PointName == "密封圈压装位移");
            //   readValue.Data.First(x => x.PointName == "挡水环压装力");
            //   readValue.Data.First(x => x.PointName == "挡水环压装位移");
            //   readValue.Data.First(x => x.PointName == "小内圈合套压力");
            //   readValue.Data.First(x => x.PointName == "小内圈合套位移");
            //   readValue.Data.First(x => x.PointName == "密封圈平行差");
            //   readValue.Data.First(x => x.PointName == "挡水环平行差");

            return true;
        }
    }
}
