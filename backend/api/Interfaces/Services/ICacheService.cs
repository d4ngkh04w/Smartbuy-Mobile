namespace api.Interfaces.Services
{
    public interface ICacheService
    {
        T? Get<T>(string key);
        void Set<T>(string key, T value, TimeSpan? expiration = null);
        bool TryGetValue<T>(string key, out T? value);
        void Remove(string key);
        void RemoveRange(IEnumerable<string> keys);

        void RemoveBrandCache(int id);
        void RemoveAllBrandsCache();
        void RemoveProductCache(int id);
        void RemoveAllProductsCache();
        void RemoveProductLineCache(int id);
        void RemoveAllProductLinesCache();
        void RemoveTagCache(int id);
        void RemoveAllTagsCache();
        void RemoveCommentCache(int id);
        void RemoveAllCommentsCache();
        void RemoveCommentsByProductCache(int productId);
    }
}
