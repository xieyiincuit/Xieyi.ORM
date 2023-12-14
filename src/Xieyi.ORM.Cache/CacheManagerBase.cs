using Xieyi.ORM.Cache.Options;
using Xieyi.ORM.Core.DbContext;

namespace Xieyi.ORM.Cache
{
    internal abstract class CacheManagerBase
    {
        protected CacheManagerBase(DbContext context, CacheOptions cacheOptions)
        {
            DbContext = context;
            CacheOptions = cacheOptions;
            CacheManager = new CacheManager(cacheOptions);
        }

        protected DbContext DbContext { get; set; }
        protected CacheOptions CacheOptions { get; private set; }
        internal CacheManager CacheManager { get; set; }
    }
}