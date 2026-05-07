using Autofac;
using SL.MLineDataPrecisionTracking.Infrastructure;
using SL.MLineDataPrecisionTracking.Models;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace SL.MLineDataPrecisionTracking.Core.Middleware
{
    public static class SqlSugerMiddleware
    {
        static string connString = ConfigurationManager
            .ConnectionStrings["DefaultConnection"]
            .ConnectionString;

        public static void AddSqlSugerMiddleware(this ContainerBuilder services)
        {
            services.Register<ISqlSugarClient>(s =>
            {
                SqlSugarScope db = new SqlSugarScope(
                    new ConnectionConfig()
                    {
                        ConnectionString = connString,
                        DbType = SqlSugar.DbType.PostgreSQL,
                        IsAutoCloseConnection = true,
                    }
                );

                // 调试SQL输出
                db.Aop.OnLogExecuting = (sql, pars) => { };

                if (
                    bool.TryParse(
                        ConfigurationManager.AppSettings["IsCreateTable"],
                        out bool CreateTable
                    )&& CreateTable
                )
                {
  
                    string targetNamespace = "SL.MLineDataPrecisionTracking.Models.Entities";
                    List<Type> types = new List<Type>();
                    foreach (var t in typeof(ModelsAssemblyMarker).Assembly.GetTypes())
                    {
                        if (
                            t.IsClass
                            && // 是类
                            !t.IsAbstract
                            && // 不是抽象类
                            !t.IsGenericType
                            && // 不是泛型类
                            !t.IsInterface
                            && // 不是接口
                            t.Namespace==targetNamespace
                        )
                        {
                            types.Add(t);
                        }
                    }
                    db.CodeFirst.SetStringDefaultLength(200).InitTables(types.ToArray());
                }

                return db;
            });
        }
    }
}
