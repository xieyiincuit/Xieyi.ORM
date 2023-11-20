using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using Xieyi.ORM.Core.Attributes;
using Xieyi.ORM.Core.ConnectionManagement;
using Xieyi.ORM.Core.Helper;
using Xieyi.ORM.Core.QueryEngine;
using Xieyi.ORM.Core.SqlDataAccess;
using Xieyi.ORM.Core.SqlStatementManagement;

// ReSharper disable VirtualMemberCallInConstructor

namespace Xieyi.ORM.Core.DbContext
{
    /// <summary>
    /// 抽象SqlDbContext，在此之上可实现MySql或MSSQL的DbContext
    /// </summary>
    public abstract class SqlDbContext : DbContext, IExecuteSql, ICacheable
    {
        protected SqlDbContext(string connectionString_Write, params string[] connectionStrings_Read) : base(connectionString_Write, connectionStrings_Read)
        {
            DbConnectionSettingInit(); //初始化访问器
            ConnectionManager.SetAndGetConnectionString(OperationType.Write); //初始化连接字符串
            DbConnection = CreateDbConnection(ConnectionManager.CurrentConnectionString); //初始化连接器
            DbCommand = CreateDbCommand(); //初始化命令执行器
            DbDataAdapter = CreateDbDataAdapter(); //初始化集合访问器
            CommandTextGenerator = CreateCommandTextGenerator(); //初始化SQL生成器
            QueryExecutor = new QueryExecutor(this);
        }

        /// <summary>
        /// 查询执行器
        /// </summary>
        internal QueryExecutor QueryExecutor { get; private set; }
        
        internal override string GetQueryCacheKey()
        {
            //如果有条件，则sql的key要拼接对应的参数值
            if (Parameters != null && Parameters.Any())
            {
                return MD5Helper.GetMd5Hash($"{SqlStatement}_{string.Join("|", Parameters.Values)}");
            }
            return MD5Helper.GetMd5Hash(SqlStatement);
        }

        #region 数据库属性、参数、查询、命令、连接管理

        /// <summary>
        /// 表名
        /// </summary>
        public string TableName
        {
            get => CollectionName;
            internal set => CollectionName = value;
        }

        /// <summary>
        /// 根据实体获取表名
        /// </summary>
        public string GetTableName<TEntity>() where TEntity : class => TableAttribute.GetName(typeof(TEntity));

        /// <summary>
        /// Sql语句，获取或赋值命令行对象的CommandText参数
        /// </summary>
        public string SqlStatement
        {
            get => this.DbCommand?.CommandText;
            internal set
            {
                if (this.DbCommand == null)
                    throw new NullReferenceException("DbCommand is null,please initialize connection first!");
                this.DbCommand.CommandText = value;
            }
        }

        /// <summary>
        /// 初始化访问器
        /// </summary>
        internal void DbConnectionSettingInit()
        {
            //设置SqlCommand对象的属性值
            DbCommand.CommandTimeout = TimeSpan.FromSeconds(60).Seconds;
        }

        /// <summary>
        /// 参数化查询参数
        /// </summary>
        public IDictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// 初始化查询参数
        /// </summary>
        internal abstract void ParameterInitializes();

        /// <summary>
        /// 数据库连接管理器
        /// </summary>
        protected DbConnection DbConnection { get; private set; }

        /// <summary>
        /// 创建连接管理器
        /// </summary>
        protected abstract DbConnection CreateDbConnection(string connectionString);

        /// <summary>
        /// 命令管理器
        /// </summary>
        internal DbCommand DbCommand { get; private set; }

        /// <summary>
        /// 创建命令管理器
        /// </summary>
        protected abstract DbCommand CreateDbCommand();

        /// <summary>
        /// 结果集访问器
        /// </summary>
        internal DbDataAdapter DbDataAdapter { get; private set; }

        /// <summary>
        /// 创建结果集访问器
        /// </summary>
        protected abstract DbDataAdapter CreateDbDataAdapter();

        /// <summary>
        /// 命令生成器
        /// </summary>
        internal CommandTextGeneratorBase CommandTextGenerator { get; private set; }

        /// <summary>
        /// 创建SQL生成器
        /// </summary>
        protected abstract CommandTextGeneratorBase CreateCommandTextGenerator();

        /// <summary>
        /// 连接状态检查，如果关闭，则打开连接
        /// </summary>
        internal void CheckConnectionStatus()
        {
            if (DbConnection.State != ConnectionState.Open)
                DbConnection.Open();
        }

        /// <summary>
        /// 切换数据库连接
        /// </summary>
        /// <param name="operationType"></param>
        private void SwitchConnection(OperationType operationType)
        {
            //获取下次执行的链接字符串
            var connectionString = this.ConnectionManager.SetAndGetConnectionString(operationType);
            //如果下次设置的链接字符串和当前的一致，则无需切换
            if (connectionString == this.DbConnection.ConnectionString)
                return;

            //关闭旧有链接
            if (this.DbConnection.State != ConnectionState.Closed)
                this.DbConnection.Close();

            this.DbConnection.ConnectionString = connectionString;
        }

        /// <summary>
        /// 是否Sql语句或者存储过程,如果是SQL语句或者存储过程，则查询时不走缓存，且跳过LINQ解析的代码
        /// </summary>
        internal bool IsSqlStatementOrStoredProcedure { get; set; } = false;

        #endregion

        #region 强类型执行API

        public override void Add<TEntity>(TEntity entity)
        {
            DataValidatorSafeExecute(validator => validator.Verify(entity));

            DbCommand.CommandType = CommandType.Text;
            this.CommandTextGenerator.Add(entity);
            this.SwitchConnection(OperationType.Write);

            this.QueryExecutor.ExecuteNonQuery();
            this.DbCacheManagerSafeExecute(m => m.Add(entity));
        }

        public override async Task AddAsync<TEntity>(TEntity entity)
        {
            DataValidatorSafeExecute(validator => validator.Verify(entity));

            DbCommand.CommandType = CommandType.Text;
            this.CommandTextGenerator.Add(entity);
            this.SwitchConnection(OperationType.Write);

            await this.QueryExecutor.ExecuteNonQueryAsync();
            this.DbCacheManagerSafeExecute(m => m.Add(entity));
        }

        public void Add<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            DbCommand.CommandType = CommandType.Text;
            this.SwitchConnection(OperationType.Write);

            foreach (var entity in entities)
            {
                DataValidatorSafeExecute(v => v.Verify(entity));
                this.CommandTextGenerator.Add(entity);
                this.QueryExecutor.ExecuteNonQuery();
            }

            this.DbCacheManagerSafeExecute(m => m.Add(entities));
        }

        public async Task AddAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            DbCommand.CommandType = CommandType.Text;
            this.SwitchConnection(OperationType.Write);

            foreach (var entity in entities)
            {
                DataValidatorSafeExecute(v => v.Verify(entity));
                this.CommandTextGenerator.Add(entity);
                await this.QueryExecutor.ExecuteNonQueryAsync();
            }

            this.DbCacheManagerSafeExecute(m => m.Add(entities));
        }

        public override void Delete<TEntity>(Expression<Func<TEntity, bool>> filter)
        {
            DbCommand.CommandType = CommandType.Text;
            this.CommandTextGenerator.Delete(filter);
            this.SwitchConnection(OperationType.Write);

            this.QueryExecutor.ExecuteNonQuery();
            this.DbCacheManagerSafeExecute(m => m.Delete(filter));
        }

        public override async Task DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> filter)
        {
            DbCommand.CommandType = CommandType.Text;
            this.CommandTextGenerator.Delete(filter);
            this.SwitchConnection(OperationType.Write);

            await this.QueryExecutor.ExecuteNonQueryAsync();
            this.DbCacheManagerSafeExecute(m => m.Delete(filter));
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            DbCommand.CommandType = CommandType.Text;
            this.CommandTextGenerator.Delete(entity);
            this.SwitchConnection(OperationType.Write);

            this.QueryExecutor.ExecuteNonQuery();
            this.DbCacheManagerSafeExecute(m => m.Delete(entity));
        }

        public async Task DeleteAsync<TEntity>(TEntity entity) where TEntity : class
        {
            DbCommand.CommandType = CommandType.Text;
            this.CommandTextGenerator.Delete(entity);
            this.SwitchConnection(OperationType.Write);

            await this.QueryExecutor.ExecuteNonQueryAsync();
            this.DbCacheManagerSafeExecute(m => m.Delete(entity));
        }

        public override void Update<TEntity>(Expression<Func<TEntity, bool>> filter, TEntity entity)
        {
            DbCommand.CommandType = CommandType.Text;
            this.CommandTextGenerator.Update(entity, filter);
            this.SwitchConnection(OperationType.Write);

            this.QueryExecutor.ExecuteNonQuery();
            this.DbCacheManagerSafeExecute(m => m.Update(entity, filter));
        }

        public override async Task UpdateAsync<TEntity>(Expression<Func<TEntity, bool>> filter, TEntity entity)
        {
            DbCommand.CommandType = CommandType.Text;
            this.CommandTextGenerator.Update(entity, filter);
            this.SwitchConnection(OperationType.Write);

            await this.QueryExecutor.ExecuteNonQueryAsync();
            this.DbCacheManagerSafeExecute(m => m.Update(entity, filter));
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class
        {
            DataValidatorSafeExecute(v => v.Verify(entity));

            DbCommand.CommandType = CommandType.Text;
            this.CommandTextGenerator.Update(entity, out Expression<Func<TEntity, bool>> filter);
            this.SwitchConnection(OperationType.Write);

            this.QueryExecutor.ExecuteNonQuery();
            this.DbCacheManagerSafeExecute(m => m.Update(entity, filter));
        }

        public async Task UpdateAsync<TEntity>(TEntity entity) where TEntity : class
        {
            DataValidatorSafeExecute(v => v.Verify(entity));

            DbCommand.CommandType = CommandType.Text;
            this.CommandTextGenerator.Update(entity, out Expression<Func<TEntity, bool>> filter);
            this.SwitchConnection(OperationType.Write);

            await this.QueryExecutor.ExecuteNonQueryAsync();
            this.DbCacheManagerSafeExecute(m => m.Update(entity, filter));
        }

        #endregion

        #region 强类型Linq查询API

        /// <summary>
        /// 强类型Linq查询器
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public ILinqQueryable<TEntity> Queryable<TEntity>() where TEntity : class
        {
            this.DbCommand.CommandType = CommandType.Text;
            //重置命令生成器，防止上次查询参数被重用
            this.CommandTextGenerator = this.CreateCommandTextGenerator();
            this.SwitchConnection(OperationType.Read);
            return new SqlQueryable<TEntity>(this);
        }

        /// <summary>
        /// 强类型Sql查询器
        /// </summary>
        /// <returns></returns>
        public IQueryable<TEntity> Queryable<TEntity>(string sqlStatement, IDictionary<string, object> @params = null) where TEntity : class
        {
            this.DbCommand.CommandType = CommandType.Text;
            this.SqlStatement = sqlStatement;
            this.Parameters = @params;
            this.SwitchConnection(OperationType.Read);
            this.IsSqlStatementOrStoredProcedure = true;
            return new SqlQueryable<TEntity>(this);
        }

        /// <summary>
        /// 强类型存储过程查询器
        /// </summary>
        /// <returns></returns>
        public IQueryable<TEntity> StoredProcedureQueryable<TEntity>(string storedProcedureName, IDictionary<string, object> @params = null) where TEntity : class
        {
            this.DbCommand.CommandType = CommandType.StoredProcedure;
            this.SqlStatement = storedProcedureName;
            this.Parameters = @params;
            this.SwitchConnection(OperationType.Read);
            this.IsSqlStatementOrStoredProcedure = true;
            return new SqlQueryable<TEntity>(this);
        }

        #endregion

        #region Sql or Procedure 执行

        public int ExecuteSql(string sql, IDictionary<string, object> parameters = null)
        {
            DbCommand.CommandType = CommandType.Text;
            this.SqlStatement = sql;
            this.Parameters = parameters;
            this.SwitchConnection(OperationType.Write);
            this.IsSqlStatementOrStoredProcedure = true;
            return QueryExecutor.ExecuteNonQuery();
        }

        public async Task<int> ExecuteSqlAsync(string sql, IDictionary<string, object> parameters = null)
        {
            DbCommand.CommandType = CommandType.Text;
            this.SqlStatement = sql;
            this.Parameters = parameters;
            this.SwitchConnection(OperationType.Write);
            this.IsSqlStatementOrStoredProcedure = true;
            return await QueryExecutor.ExecuteNonQueryAsync();
        }

        public int ExecuteProcedure(string procedureName, IDictionary<string, object> parameters = null)
        {
            this.SqlStatement = procedureName;
            this.Parameters = parameters;
            DbCommand.CommandType = CommandType.StoredProcedure;
            this.SwitchConnection(OperationType.Write);
            this.IsSqlStatementOrStoredProcedure = true;
            return QueryExecutor.ExecuteNonQuery();
        }

        public async Task<int> ExecuteProcedureAsync(string procedureName, IDictionary<string, object> parameters = null)
        {
            this.SqlStatement = procedureName;
            this.Parameters = parameters;
            DbCommand.CommandType = CommandType.StoredProcedure;
            this.SwitchConnection(OperationType.Write);
            this.IsSqlStatementOrStoredProcedure = true;
            return await QueryExecutor.ExecuteNonQueryAsync();
        }

        #endregion

        #region 事务执行 Transation

        public void ExecuteWithTransaction(Action action)
        {
            try
            {
                this.DbCommand.Transaction = this.DbConnection.BeginTransaction();
                action();
                this.DbCommand.Transaction.Commit();
            }
            catch (Exception ex)
            {
                this.DbCommand.Transaction.Rollback();
                throw new Exception("Xieyi.ORM ExecuteWithTransaction Error", ex);
            }
        }

        #endregion
        
        public override void Dispose()
        {
            if (this.DbConnection.State == ConnectionState.Open)
                this.DbConnection.Close();

            this.DbDataAdapter?.Dispose();
            this.DbCommand?.Dispose();
            this.DbConnection?.Dispose();
        }
    }
}