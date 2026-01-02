using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace StreamUP
{
    /// <summary>
    /// Static in-memory cache for product settings.
    /// Provides fast access to loaded product data without repeated file I/O.
    /// Caching is controlled per-load via optional parameters.
    /// </summary>
    public static class SettingsCacheV2
    {
        /// <summary>
        /// Cache storage: productNumber → full product data JObject
        /// </summary>
        private static Dictionary<string, JObject> _cache = new Dictionary<string, JObject>();

        /// <summary>
        /// Attempt to retrieve cached product data.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="data">Out parameter containing cached data if found</param>
        /// <returns>True if data was found in cache, false otherwise</returns>
        public static bool TryGetCachedData(string productNumber, out JObject data)
        {
            return _cache.TryGetValue(productNumber, out data);
        }

        /// <summary>
        /// Store product data in cache. Updates existing entry if product already cached.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="data">Product data to cache</param>
        public static void SetCachedData(string productNumber, JObject data)
        {
            _cache[productNumber] = data;
        }

        /// <summary>
        /// Clear cache for specific product.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        public static void ClearCache(string productNumber)
        {
            _cache.Remove(productNumber);
        }

        /// <summary>
        /// Clear entire cache. Use for application shutdown or cache reset.
        /// </summary>
        public static void ClearAllCache()
        {
            _cache.Clear();
        }

        /// <summary>
        /// Check if product data is currently cached.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <returns>True if product data is in cache</returns>
        public static bool IsCached(string productNumber)
        {
            return _cache.ContainsKey(productNumber);
        }

        /// <summary>
        /// Get number of products in cache. Useful for debugging and monitoring.
        /// </summary>
        /// <returns>Count of cached products</returns>
        public static int GetCacheCount()
        {
            return _cache.Count;
        }
    }
}
