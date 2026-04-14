using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Infrastructure.Storage
{
    using SqlSugar;
    using System.Linq.Expressions;

    /// <summary>
    /// SqlSugar 通用仓储层
    /// </summary>
    /// <typeparam name="T">实体</typeparam>
        public class BaseRepository<T> where T : class, new()
        {
            protected readonly ISqlSugarClient _db;

            public BaseRepository(ISqlSugarClient db)
            {
                _db = db; 
            }

  
        }
}
