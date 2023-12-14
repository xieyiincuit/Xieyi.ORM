using Newtonsoft.Json;
using Xieyi.ORM.Cache.Options;

namespace Xieyi.ORM.Cache
{
    internal class CacheManager
    {
        private readonly CacheOptions _cacheOptions;

        internal CacheManager(CacheOptions cacheOptions)
        {
            _cacheOptions = cacheOptions;
        }

        private static IRedisCache redisCache;

        private IRedisCache GetRedisCacheProvider()
        {
            if (redisCache != null) return redisCache;

            redisCache = string.IsNullOrEmpty(_cacheOptions.CacheMediaServer) ? new RedisCacheManager() : new RedisCacheManager(_cacheOptions.CacheMediaServer);

            if (redisCache == null)
                throw new TimeoutException("redis server connecting timeout");

            return redisCache;
        }

        public bool IsExist(string key)
        {
            return IsExist(key, out object obj);
        }

        public bool IsExist<TValue>(string key, out TValue value)
        {
            switch (_cacheOptions.CacheMediaType)
            {
                case CacheMediaType.Local:
                    return LocalCacheHelper.Exist(key, out value);
                case CacheMediaType.Redis:
                    var redisResult = GetRedisCacheProvider().Get(key);
                    if (!string.IsNullOrEmpty(redisResult))
                    {
                        value = JsonConvert.DeserializeObject<TValue>(redisResult);
                        return true;
                    }

                    value = default(TValue);
                    return false;
                default:
                    value = default(TValue);
                    return false;
            }
        }

        public void Put<T>(string key, T value, TimeSpan expiredTime)
        {
            switch (_cacheOptions.CacheMediaType)
            {
                case CacheMediaType.Local:
                    LocalCacheHelper.Put(key, value, expiredTime);
                    break;
                case CacheMediaType.Redis:
                    GetRedisCacheProvider().Set(key, JsonConvert.SerializeObject(value), expiredTime);
                    break;
                default:
                    break;
            }
        }

        public T Get<T>(string key)
        {
            switch (_cacheOptions.CacheMediaType)
            {
                case CacheMediaType.Local:
                    return LocalCacheHelper.Get<string, T>(key);
                case CacheMediaType.Redis:
                    var redisResult = GetRedisCacheProvider().Get(key);
                    return !string.IsNullOrEmpty(redisResult) ? JsonConvert.DeserializeObject<T>(redisResult) : default(T);
                default:
                    return default(T);
            }
        }

        public void Delete(string key)
        {
            switch (_cacheOptions.CacheMediaType)
            {
                case CacheMediaType.Local:
                    LocalCacheHelper.Delete<string>(key);
                    break;
                case CacheMediaType.Redis:
                    GetRedisCacheProvider().Delete(key);
                    break;
                default:
                    break;
            }
        }
    }
}