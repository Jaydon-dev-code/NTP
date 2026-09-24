using System.Collections.Generic;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Entities.SPC;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage.SPC
{
    public class Tb_SPC_PartRepository : BaseRepository<Tb_SPC_Part>
    {
        public Tb_SPC_PartRepository(ISqlSugarClient db) : base(db) { }

        public async Task<List<Tb_SPC_Part>> GetListAsync()
        {
            return await _db.Queryable<Tb_SPC_Part>()
                .Includes(x => x.Product)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        public async Task<List<Tb_SPC_Part>> GetByProductIdAsync(int productId)
        {
            return await _db.Queryable<Tb_SPC_Part>()
                .Where(x => x.ProductId == productId)
                .Includes(x => x.Product)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        /// <summary>按主键取零件（含 Product 导航）</summary>
        public async Task<Tb_SPC_Part> GetByIdAsync(int id)
        {
            return await _db.Queryable<Tb_SPC_Part>().Includes(x => x.Product).InSingleAsync(id);
        }

        /// <summary>
        /// 按「产品 + 零件类别」查零件（忽略类别大小写，按 Id 升序）。
        /// 型号接入主链路第二步：产品确定后，用特性 PartCategory 找到该型号下的具体零件。
        /// 0 条 = NoPartForCategory；多条由解析器取最小 Id 并告警。
        /// </summary>
        public async Task<List<Tb_SPC_Part>> GetByProductAndCategoryAsync(int productId, string category)
        {
            if (string.IsNullOrWhiteSpace(category)) return new List<Tb_SPC_Part>();
            var key = category.Trim();
            return await _db.Queryable<Tb_SPC_Part>()
                .Where(x => x.ProductId == productId
                    && SqlFunc.ToLower(x.PartCategory) == key.ToLower())
                .Includes(x => x.Product)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        public async Task<int> InsertAsync(Tb_SPC_Part entity)
        {
            return await _db.Insertable(entity).ExecuteReturnIdentityAsync();
        }

        public async Task<bool> UpdateAsync(Tb_SPC_Part entity)
        {
            return await _db.Updateable(entity).ExecuteCommandHasChangeAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _db.Deleteable<Tb_SPC_Part>().In(id).ExecuteCommandHasChangeAsync();
        }
    }
}
