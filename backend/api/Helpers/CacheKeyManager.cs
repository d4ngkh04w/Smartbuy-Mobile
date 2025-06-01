using System.Collections.Concurrent;
using api.Queries;

namespace api.Helpers
{
    public static class CacheKeyManager
    {
        private static readonly ConcurrentDictionary<string, HashSet<string>> _brandKeys = new();
        private static readonly ConcurrentDictionary<string, HashSet<string>> _productKeys = new();
        private static readonly ConcurrentDictionary<string, HashSet<string>> _productLineKeys = new();
        private static readonly ConcurrentDictionary<string, HashSet<string>> _tagKeys = new();
        private static readonly ConcurrentDictionary<string, HashSet<string>> _commentKeys = new();

        #region Brand Cache Keys

        public static string GetBrandKey(int id) => RegisterBrandKey($"brand-{id}");

        public static string GetAllBrandsKey() => RegisterBrandKey("brands-all");

        public static string GetBrandsKey(string sortBy, bool? isActive) =>
            RegisterBrandKey($"brands-{isActive}-{sortBy}");

        public static string GetBrandsKey(BrandQuery query) =>
            RegisterBrandKey($"brands-{query.IncludeProductLines}-{query.IncludeProducts}-{query.IsActive}-{query.SortBy?.ToLower() ?? "name"}-{query.IsDescending}");

        public static HashSet<string> GetAllBrandKeys()
        {
            HashSet<string> allKeys = new();
            foreach (var entry in _brandKeys)
            {
                allKeys.UnionWith(entry.Value);
            }
            return allKeys;
        }

        public static HashSet<string> GetBrandKeys(int id)
        {
            string key = id.ToString();
            return _brandKeys.TryGetValue(key, out var keys) ? keys : new HashSet<string>();
        }

        private static string RegisterBrandKey(string cacheKey)
        {
            string entityId = GetEntityIdFromKey(cacheKey, "brand-");
            if (!string.IsNullOrEmpty(entityId))
            {
                _brandKeys.AddOrUpdate(entityId,
                    new HashSet<string> { cacheKey },
                    (_, existingKeys) =>
                    {
                        existingKeys.Add(cacheKey);
                        return existingKeys;
                    });
            }
            else
            {
                _brandKeys.AddOrUpdate("all",
                    new HashSet<string> { cacheKey },
                    (_, existingKeys) =>
                    {
                        existingKeys.Add(cacheKey);
                        return existingKeys;
                    });
            }

            return cacheKey;
        }
        #endregion

        #region Product Cache Keys

        public static string GetProductKey(int id) => RegisterProductKey($"product-{id}");

        public static string GetAllProductsKey() => RegisterProductKey("products-all");

        public static string GetPagedProductsKey(ProductQuery query) =>
            RegisterProductKey($"products-paged-{query.Page}-{query.PageSize}-{query.Search}-{query.SortBy}-{query.BrandName}-{query.MinPrice}-{query.MaxPrice}-{query.IsActive}");

        public static HashSet<string> GetAllProductKeys()
        {
            HashSet<string> allKeys = new();
            foreach (var entry in _productKeys)
            {
                allKeys.UnionWith(entry.Value);
            }
            return allKeys;
        }

        public static HashSet<string> GetProductKeys(int id)
        {
            string key = id.ToString();
            return _productKeys.TryGetValue(key, out var keys) ? keys : new HashSet<string>();
        }

        private static string RegisterProductKey(string cacheKey)
        {
            string entityId = GetEntityIdFromKey(cacheKey, "product-");
            if (!string.IsNullOrEmpty(entityId))
            {
                _productKeys.AddOrUpdate(entityId,
                    new HashSet<string> { cacheKey },
                    (_, existingKeys) =>
                    {
                        existingKeys.Add(cacheKey);
                        return existingKeys;
                    });
            }
            else
            {
                _productKeys.AddOrUpdate("all",
                    new HashSet<string> { cacheKey },
                    (_, existingKeys) =>
                    {
                        existingKeys.Add(cacheKey);
                        return existingKeys;
                    });
            }

            return cacheKey;
        }
        #endregion

        #region ProductLine Cache Keys

        public static string GetProductLineKey(int id, ProductLineQuery? query) =>
            RegisterProductLineKey($"productline-{id}-{query?.IncludeProducts}-{query?.IsActive}");

        public static string GetAllProductLinesKey(ProductLineQuery query) =>
            RegisterProductLineKey($"productlines-{query.IncludeProducts}-{query.IsActive}-{query.BrandId}-{query.SortBy?.ToLower() ?? "name"}-{query.IsDescending}");

        public static HashSet<string> GetAllProductLineKeys()
        {
            HashSet<string> allKeys = new();
            foreach (var entry in _productLineKeys)
            {
                allKeys.UnionWith(entry.Value);
            }
            return allKeys;
        }

        public static HashSet<string> GetProductLineKeys(int id)
        {
            string key = id.ToString();
            return _productLineKeys.TryGetValue(key, out var keys) ? keys : new HashSet<string>();
        }

        private static string RegisterProductLineKey(string cacheKey)
        {
            string entityId = GetEntityIdFromKey(cacheKey, "productline-");
            if (!string.IsNullOrEmpty(entityId))
            {
                _productLineKeys.AddOrUpdate(entityId,
                    new HashSet<string> { cacheKey },
                    (_, existingKeys) =>
                    {
                        existingKeys.Add(cacheKey);
                        return existingKeys;
                    });
            }
            else
            {
                _productLineKeys.AddOrUpdate("all",
                    new HashSet<string> { cacheKey },
                    (_, existingKeys) =>
                    {
                        existingKeys.Add(cacheKey);
                        return existingKeys;
                    });
            }

            return cacheKey;
        }
        #endregion

        #region Tag Cache Keys

        public static string GetTagKey(int id) => RegisterTagKey($"tag-{id}");

        public static string GetAllTagsKey() => RegisterTagKey("all-tags");

        public static HashSet<string> GetAllTagKeys()
        {
            HashSet<string> allKeys = new();
            foreach (var entry in _tagKeys)
            {
                allKeys.UnionWith(entry.Value);
            }
            return allKeys;
        }

        public static HashSet<string> GetTagKeys(int id)
        {
            string key = id.ToString();
            return _tagKeys.TryGetValue(key, out var keys) ? keys : new HashSet<string>();
        }

        private static string RegisterTagKey(string cacheKey)
        {
            string entityId = GetEntityIdFromKey(cacheKey, "tag-");
            if (!string.IsNullOrEmpty(entityId))
            {
                _tagKeys.AddOrUpdate(entityId,
                    new HashSet<string> { cacheKey },
                    (_, existingKeys) =>
                    {
                        existingKeys.Add(cacheKey);
                        return existingKeys;
                    });
            }
            else
            {
                _tagKeys.AddOrUpdate("all",
                    new HashSet<string> { cacheKey },
                    (_, existingKeys) =>
                    {
                        existingKeys.Add(cacheKey);
                        return existingKeys;
                    });
            }

            return cacheKey;
        }
        #endregion

        #region Comment Cache Keys

        public static string GetCommentKey(int id) => RegisterCommentKey($"comment-{id}");

        public static string GetAllCommentsKey() => RegisterCommentKey("comments-all");

        public static string GetCommentsByProductIdKey(int productId, int page, int pageSize) =>
            RegisterCommentKey($"comments-product-{productId}-page-{page}-size-{pageSize}");

        public static string GetProductAverageRatingKey(int productId) =>
            RegisterCommentKey($"product-{productId}-rating");

        public static string GetProductRatingStatsKey(int productId) =>
            RegisterCommentKey($"product-{productId}-rating-stats");

        public static HashSet<string> GetAllCommentKeys()
        {
            HashSet<string> allKeys = new();
            foreach (var entry in _commentKeys)
            {
                allKeys.UnionWith(entry.Value);
            }
            return allKeys;
        }

        public static HashSet<string> GetCommentKeys(int id)
        {
            string key = id.ToString();
            return _commentKeys.TryGetValue(key, out var keys) ? keys : new HashSet<string>();
        }

        private static string RegisterCommentKey(string cacheKey)
        {
            string entityId = GetEntityIdFromKey(cacheKey, "comment-");
            if (!string.IsNullOrEmpty(entityId))
            {
                _commentKeys.AddOrUpdate(entityId,
                    new HashSet<string> { cacheKey },
                    (_, existingKeys) =>
                    {
                        existingKeys.Add(cacheKey);
                        return existingKeys;
                    });
            }
            else
            {
                _commentKeys.AddOrUpdate("all",
                    new HashSet<string> { cacheKey },
                    (_, existingKeys) =>
                    {
                        existingKeys.Add(cacheKey);
                        return existingKeys;
                    });
            }

            return cacheKey;
        }
        #endregion

        private static string GetEntityIdFromKey(string key, string prefix)
        {
            if (key.StartsWith(prefix))
            {
                string idPart = key.Substring(prefix.Length);
                int dashIndex = idPart.IndexOf('-');
                if (dashIndex > 0)
                {
                    idPart = idPart.Substring(0, dashIndex);
                }

                return idPart;
            }
            return string.Empty;
        }
    }
}
