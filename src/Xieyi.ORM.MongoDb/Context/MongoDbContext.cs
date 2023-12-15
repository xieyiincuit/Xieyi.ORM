using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Xieyi.ORM.Core.Attributes;
using Xieyi.ORM.Core.DbContext;

namespace Xieyi.ORM.MongoDb.Context;

public abstract class MongoDbContext<TDataBase> : NoSqlDbContext where TDataBase : class
{
    protected MongoDbContext(string connectionString) : base(connectionString)
    {
        SetContext();
        Client = new MongoClient(connectionString);
    }

    protected MongoDbContext(string host, int port) : base(string.Concat(host, ":", port))
    {
        SetContext();
        Client = new MongoClient(new MongoClientSettings { Server = new MongoServerAddress(host, port) });
    }

    protected MongoDbContext(IDictionary<string, int> host_port_dic) : base("default")
    {
        SetContext();

        if (host_port_dic == null || !host_port_dic.Any())
            throw new ArgumentException("host_port_dic must be provided", nameof(host_port_dic));

        Client = new MongoClient(new MongoClientSettings
        {
            Servers = host_port_dic.Select(t => new MongoServerAddress(t.Key, t.Value)).ToList()
        });
    }

    protected MongoDbContext(MongoClientSettings mongoClientSettings) : base("default")
    {
        SetContext();
        Client = new MongoClient(mongoClientSettings);
    }

    /// <summary>
    /// 上下文赋值
    /// </summary>
    private void SetContext()
    {
        DataBaseType = DataBaseType.MongoDB;
        DataBaseName = DataBaseAttribute.GetName(typeof(TDataBase));
    }

    #region MongoDb Server

    protected MongoClient Client { get; private set; }
    protected IMongoDatabase DataBase => Client.GetDatabase(DataBaseName);

    public IMongoCollection<TEntity> GetCollectionEntity<TEntity>() where TEntity : class
    {
        CollectionName = TableAttribute.GetName(typeof(TEntity));
        return DataBase.GetCollection<TEntity>(CollectionName);
    }

    /// <summary>
    /// 对外支持弱类型的接口
    /// </summary>
    /// <param name="collectionName"></param>
    /// <returns></returns>
    public IMongoCollection<BsonDocument> GetCollectionBson(string collectionName)
    {
        CollectionName = collectionName;
        return DataBase.GetCollection<BsonDocument>(collectionName);
    }

    #endregion

    #region 强类型 API

    public override void Add<TEntity>(TEntity entity)
    {
        DataValidatorSafeExecute(v => v.Verify(entity));
        GetCollectionEntity<TEntity>().InsertOne(entity);
        DbCacheManagerSafeExecute(m => m.Add(entity));
    }

    public override async Task AddAsync<TEntity>(TEntity entity)
    {
        DataValidatorSafeExecute(v => v.Verify(entity));
        await GetCollectionEntity<TEntity>().InsertOneAsync(entity);
        DbCacheManagerSafeExecute(m => m.Add(entity));
    }

    public void Add<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
    {
        DataValidatorSafeExecute(v => v.Verify(entities));
        GetCollectionEntity<TEntity>().InsertMany(entities);
        DbCacheManagerSafeExecute(m => m.Add(entities));
    }

    public async Task AddAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
    {
        DataValidatorSafeExecute(v => v.Verify(entities));
        await GetCollectionEntity<TEntity>().InsertManyAsync(entities);
        DbCacheManagerSafeExecute(m => m.Add(entities));
    }

    public override void Update<TEntity>(Expression<Func<TEntity, bool>> filter, TEntity entity)
    {
        DataValidatorSafeExecute(v => v.Verify(entity));
        GetCollectionEntity<TEntity>().ReplaceOne(filter, entity);
        DbCacheManagerSafeExecute(m => m.Update(entity, filter));
    }

    public override async Task UpdateAsync<TEntity>(Expression<Func<TEntity, bool>> filter, TEntity entity)
    {
        DataValidatorSafeExecute(v => v.Verify(entity));
        await GetCollectionEntity<TEntity>().ReplaceOneAsync(filter, entity);
        DbCacheManagerSafeExecute(m => m.Update(entity, filter));
    }

    public void DeleteOne<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class
    {
        GetCollectionEntity<TEntity>().DeleteOne(filter);
        DbCacheManagerSafeExecute(m => m.Delete(filter));
    }

    public async Task DeleteOneAsync<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class
    {
        await GetCollectionEntity<TEntity>().DeleteOneAsync(filter);
        DbCacheManagerSafeExecute(m => m.Delete(filter));
    }

    public override void Delete<TEntity>(Expression<Func<TEntity, bool>> filter)
    {
        GetCollectionEntity<TEntity>().DeleteMany(filter);
        DbCacheManagerSafeExecute(m => m.Delete(filter));
    }

    public override async Task DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> filter)
    {
        await GetCollectionEntity<TEntity>().DeleteManyAsync(filter);
        DbCacheManagerSafeExecute(m => m.Delete(filter));
    }

    public IMongoQueryable<TEntity> MongoQueryable<TEntity>() where TEntity : class
    {
        return GetCollectionEntity<TEntity>().AsQueryable();
    }

    #endregion

    /// <summary>
    /// 获取全集合数据
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    internal override List<TEntity> GetFullCollectionData<TEntity>()
    {
        //获取MongoDb全文档记录
        return GetCollectionEntity<TEntity>().Find(t => true).ToList();
    }
}