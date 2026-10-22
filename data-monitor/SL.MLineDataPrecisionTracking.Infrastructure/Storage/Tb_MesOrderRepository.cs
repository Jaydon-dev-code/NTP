using SL.MLineDataPrecisionTracking.Models.Entities.Mes;
using SL.MLineDataPrecisionTracking.Models.Enum;
using SqlSugar;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    public class Tb_MesOrderRepository : BaseRepository<Tb_MesOrder>
    {
        public Tb_MesOrderRepository(ISqlSugarClient db)
            : base(db) { }

        /// <summary>
        /// 按工单号查询
        /// </summary>
        public async Task<Tb_MesOrder> GetByWorkOrderNoAsync(string workOrderNo)
        {
            return await _db
                .Queryable<Tb_MesOrder>()
                .Where(x => x.WorkOrderNo == workOrderNo)
                .FirstAsync();
        }

        /// <summary>
        /// 查询全部工单（按创建时间倒序）
        /// </summary>
        public async Task<List<Tb_MesOrder>> GetListAsync()
        {
            return await _db
                .Queryable<Tb_MesOrder>()
                .OrderBy(x => x.CreateTime, OrderByType.Desc)
                .ToListAsync();
        }

        /// <summary>
        /// 查询最新一条执行中订单（Status=1，按创建时间倒序）
        /// </summary>
        public async Task<Tb_MesOrder> GetExecutingAsync()
        {
            return await _db
                .Queryable<Tb_MesOrder>()
                .Where(x => x.Status == MesOrderStatusEnum.Executing)
                .OrderBy(x => x.CreateTime, OrderByType.Desc)
                .FirstAsync();
        }

        /// <summary>
        /// 新增工单，返回主键ID
        /// </summary>
        public async Task<int> InsertAsync(Tb_MesOrder entity)
        {
            return await _db.Insertable(entity).ExecuteReturnIdentityAsync();
        }

        /// <summary>
        /// 批量新增BOM组件
        /// </summary>
        public async Task<int> InsertBomsAsync(List<Tb_MesOrderBom> boms)
        {
            return await _db.Insertable(boms).ExecuteCommandAsync();
        }

        /// <summary>
        /// 更新工单计划数量
        /// </summary>
        public async Task<int> UpdatePlanQtyAsync(string workOrderNo, double planQty)
        {
            return await _db
                .Updateable<Tb_MesOrder>()
                .SetColumns(x => x.PlanQty == planQty)
                .Where(x => x.WorkOrderNo == workOrderNo)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 更新工单状态
        /// </summary>
        public async Task<int> SetStatusAsync(string workOrderNo, MesOrderStatusEnum status)
        {
            return await _db
                .Updateable<Tb_MesOrder>()
                .SetColumns(x => x.Status == status)
                .Where(x => x.WorkOrderNo == workOrderNo)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 将除指定工单外的其他执行中工单改回待执行（保证同一时间只有一个执行中工单）
        /// </summary>
        public async Task<int> ResetOtherExecutingAsync(string workOrderNo)
        {
            return await _db
                .Updateable<Tb_MesOrder>()
                .SetColumns(x => x.Status == MesOrderStatusEnum.Pending)
                .Where(x => x.Status == MesOrderStatusEnum.Executing && x.WorkOrderNo != workOrderNo)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 按工单号删除工单及其BOM
        /// </summary>
        public async Task<int> DeleteByWorkOrderNoAsync(string workOrderNo)
        {
            var order = await _db
                .Queryable<Tb_MesOrder>()
                .Where(x => x.WorkOrderNo == workOrderNo)
                .FirstAsync();
            if (order == null)
            {
                return 0;
            }

            await _db
                .Deleteable<Tb_MesOrderBom>()
                .Where(x => x.MesOrderId == order.Id)
                .ExecuteCommandAsync();

            return await _db
                .Deleteable<Tb_MesOrder>()
                .Where(x => x.Id == order.Id)
                .ExecuteCommandAsync();
        }
    }
}
