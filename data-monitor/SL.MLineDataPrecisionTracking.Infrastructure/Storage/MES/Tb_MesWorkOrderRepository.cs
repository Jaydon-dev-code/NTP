using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Infrastructure.Storage;
using SL.MLineDataPrecisionTracking.Models.Entities.MES;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage.MES
{
    /// <summary>
    /// MES 工单仓储（工单主表 + BOM 明细，主从一体维护）
    /// </summary>
    public class Tb_MesWorkOrderRepository : BaseRepository<Tb_Mes_WorkOrder>
    {
        public Tb_MesWorkOrderRepository(ISqlSugarClient db)
            : base(db) { }

        public async Task<bool> AnyAsync(Expression<Func<Tb_Mes_WorkOrder, bool>> expression)
        {
            return await _db.Queryable<Tb_Mes_WorkOrder>().AnyAsync(expression);
        }

        public async Task<Tb_Mes_WorkOrder> GetFirstAsync(
            Expression<Func<Tb_Mes_WorkOrder, bool>> expression
        )
        {
            return await _db.Queryable<Tb_Mes_WorkOrder>().FirstAsync(expression);
        }

        /// <summary>
        /// 插入工单及其 BOM（同一事务）
        /// </summary>
        public async Task<int> InsertOrderWithBomsAsync(
            Tb_Mes_WorkOrder order,
            List<Tb_Mes_WorkOrderBom> boms
        )
        {
            // BaseRepository 的 _db 每次访问返回新实例，此处固定引用以保证事务作用于同一上下文
            var db = _db;
            try
            {
                db.Ado.BeginTran();
                // 先插主表拿自增 Id，再批量插明细
                order.Id = await db.Insertable(order).ExecuteReturnIdentityAsync();
                if (boms != null && boms.Count > 0)
                    await db.Insertable(boms).ExecuteCommandAsync();
                db.Ado.CommitTran();
                return order.Id;
            }
            catch
            {
                // 任一步失败：回滚主表与明细，避免出现"有工单无 BOM"的脏数据
                db.Ado.RollbackTran();
                throw;
            }
        }

        /// <summary>
        /// 更新工单计划数量
        /// </summary>
        public async Task<int> UpdatePlanQtyAsync(string workOrderNo, decimal planQty)
        {
            // 只更新 PlanQty 单列，不动其他字段（SetColumns 生成 SET 局部更新）
            return await _db.Updateable<Tb_Mes_WorkOrder>()
                .SetColumns(x => x.PlanQty == planQty)
                .Where(x => x.WorkOrderNo == workOrderNo)
                .ExecuteCommandAsync();
        }

        public async Task<int> DeleteOrderAsync(string workOrderNo)
        {
            return await _db.Updateable<Tb_Mes_WorkOrder>()
                .SetColumns(x => x.IsDeleted == true)
                .Where(x => x.WorkOrderNo == workOrderNo)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除工单及其 BOM（同一事务）
        /// </summary>
        public async Task<int> DeleteOrderWithBomsAsync(string workOrderNo)
        {
            // 同一 db 实例保证主表/明细删除在同一事务内
            var db = _db;
            try
            {
                db.Ado.BeginTran();
                var count = await db.Deleteable<Tb_Mes_WorkOrder>()
                    .Where(x => x.WorkOrderNo == workOrderNo)
                    .ExecuteCommandAsync();
                await db.Deleteable<Tb_Mes_WorkOrderBom>()
                    .Where(x => x.WorkOrderNo == workOrderNo)
                    .ExecuteCommandAsync();
                db.Ado.CommitTran();
                return count;
            }
            catch
            {
                // 任一步失败：回滚，避免只删了主表或只删了明细
                db.Ado.RollbackTran();
                throw;
            }
        }
    }
}
