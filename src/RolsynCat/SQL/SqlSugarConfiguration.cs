using SqlSugar;
namespace RoslynCat.SQL
{
    public static class SqlSugarConfiguration
    {
        public static ISqlSugarClient Configure() {
            var db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = new GetConfig().ConnectionString,
                DbType = DbType.SqlServer,
                IsAutoCloseConnection = true
            });

            // 配置SqlSugar的日志记录器
            db.Aop.OnLogExecuting = (sql,pars) =>
            {
                Console.WriteLine(sql + "\r\n" + db.Utilities.SerializeObject(pars.ToDictionary(p => p.ParameterName,p => p.Value)));
            };
            return db;
        }
    }
}
