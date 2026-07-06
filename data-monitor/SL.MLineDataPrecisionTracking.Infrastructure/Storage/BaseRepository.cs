using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using SqlSugar;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{


    /// <summary>
    /// SqlSugar 通用仓储层
    /// </summary>
    /// <typeparam name="T">实体</typeparam>
    public class BaseRepository<T>
        where T : class, new()
    {
        protected  ISqlSugarClient _db=>dbs.CopyNew();
        ISqlSugarClient dbs;

        public BaseRepository(ISqlSugarClient db)
        {
            dbs = db;
        }
    }
}
