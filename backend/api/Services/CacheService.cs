using api.Helpers;
using api.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace api.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _defaultCacheDuration;

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
            _defaultCacheDuration = TimeSpan.FromMinutes(5);
        }

        public T? Get<T>(string key)
        {
            return _cache.Get<T>(key);
        }
        public void Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiration ?? _defaultCacheDuration);

            if (typeof(T).ToString().Contains("Collection") || typeof(T).ToString().Contains("List") || typeof(T).ToString().Contains("Array"))
            {
                cacheEntryOptions.SetSize(50);
            }
            else if (typeof(T).ToString().Contains("Product"))
            {
                cacheEntryOptions.SetSize(20);
            }
            else if (typeof(T).ToString().Contains("Brand") || typeof(T).ToString().Contains("ProductLine"))
            {
                cacheEntryOptions.SetSize(10);
            }
            else
            {
                cacheEntryOptions.SetSize(5);
            }

            _cache.Set(key, value, cacheEntryOptions);
        }

        public bool TryGetValue<T>(string key, out T? value)
        {
            return _cache.TryGetValue<T>(key, out value);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void RemoveRange(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                _cache.Remove(key);
            }
        }

        // Brand Cache Management
        public void RemoveBrandCache(int id)
        {
            var keys = CacheKeyManager.GetBrandKeys(id);
            RemoveRange(keys);

            RemoveAllBrandsCache();
        }

        public void RemoveAllBrandsCache()
        {
            var keys = CacheKeyManager.GetAllBrandKeys();
            RemoveRange(keys);
        }

        public void RemoveProductCache(int id)
        {
            var keys = CacheKeyManager.GetProductKeys(id);
            RemoveRange(keys);
        }

        public void RemoveAllProductsCache()
        {
            var keys = CacheKeyManager.GetAllProductKeys();
            RemoveRange(keys);
        }

        public void RemoveProductLineCache(int id)
        {
            var keys = CacheKeyManager.GetProductLineKeys(id);
            RemoveRange(keys);
        }

        public void RemoveAllProductLinesCache()
        {
            var keys = CacheKeyManager.GetAllProductLineKeys();
            RemoveRange(keys);
        }

        public void RemoveTagCache(int id)
        {
            var keys = CacheKeyManager.GetTagKeys(id);
            RemoveRange(keys);
        }
        public void RemoveAllTagsCache()
        {
            var keys = CacheKeyManager.GetAllTagKeys();
            RemoveRange(keys);
        }

        public void RemoveCommentCache(int id)
        {
            var keys = CacheKeyManager.GetCommentKeys(id);
            RemoveRange(keys);
        }

        public void RemoveAllCommentsCache()
        {
            var keys = CacheKeyManager.GetAllCommentKeys();
            RemoveRange(keys);
        }

        public void RemoveCommentsByProductCache(int productId)
        {
            var allKeys = CacheKeyManager.GetAllCommentKeys();
            var productCommentKeys = allKeys.Where(k => k.Contains($"comments-product-{productId}") || k.Contains($"product-{productId}-rating"));
            RemoveRange(productCommentKeys);
        }
    }
}
