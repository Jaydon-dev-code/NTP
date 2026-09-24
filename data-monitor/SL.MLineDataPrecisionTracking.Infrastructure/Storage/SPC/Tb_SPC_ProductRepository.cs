using System.Collections.Generic;
using System.Threading.Tasks;
using SL.MLineDataPrecisionTracking.Models.Entities.SPC;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage.SPC
{
    public class Tb_SPC_ProductRepository : BaseRepository<Tb_SPC_Product>
    {
        public Tb_SPC_ProductRepository(ISqlSugarClient db) : base(db) { }

        public async Task<List<Tb_SPC_Product>> GetListAsync()
        {
            return await _db.Queryable<Tb_SPC_Product>().OrderBy(x => x.Id).ToListAsync();
        }

        /// <summary>按主键取产品</summary>
        public async Task<Tb_SPC_Product> GetByIdAsync(int id)
        {
            return await _db.Queryable<Tb_SPC_Product>().InSingleAsync(id);
        }

        /// <summary>
        /// 按产品型号精确匹配（忽略大小写、去首尾空格）。
        /// 型号接入主链路：三方当前型号 → 本方法找 SPC_Product。
        /// 未找到返回 null（解析器记 UnknownModel）。
        /// </summary>
        public async Task<Tb_SPC_Product> GetByModelNoAsync(string modelNo)
        {
            if (string.IsNullOrWhiteSpace(modelNo)) return null;
            var key = modelNo.Trim();
            return await _db.Queryable<Tb_SPC_Product>()
                .Where(x => SqlFunc.ToLower(x.ModelNo) == key.ToLower())
                .FirstAsync();
        }

        public async Task<int> InsertAsync(Tb_SPC_Product entity)
        {
            return await _db.Insertable(entity).ExecuteReturnIdentityAsync();
        }

        public async Task<bool> UpdateAsync(Tb_SPC_Product entity)
        {
            return await _db.Updateable(entity).ExecuteCommandHasChangeAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _db.Deleteable<Tb_SPC_Product>().In(id).ExecuteCommandHasChangeAsync();
        }
    }
}
