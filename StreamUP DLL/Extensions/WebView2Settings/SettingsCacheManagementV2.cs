using System;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        /// <summary>
        /// Clear cache for a specific product. Use this to force reload from file on next access.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        public void InvalidateCacheV2(string productNumber)
        {
            try
            {
                if (!string.IsNullOrEmpty(productNumber))
                {
                    SettingsCacheV2.ClearCache(productNumber);
                    LogDebug($"Cleared cache for product {productNumber}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error clearing cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear entire cache. Use for application shutdown or full reset.
        /// </summary>
        public void InvalidateAllCacheV2()
        {
            try
            {
                SettingsCacheV2.ClearAllCache();
                LogInfo("Cleared all product cache");
            }
            catch (Exception ex)
            {
                LogError($"Error clearing all cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if a specific product is currently cached.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <returns>True if product is cached</returns>
        public bool IsCachedV2(string productNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(productNumber))
                    return false;

                return SettingsCacheV2.IsCached(productNumber);
            }
            catch (Exception ex)
            {
                LogError($"Error checking cache status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get count of products currently in cache.
        /// </summary>
        /// <returns>Number of cached products</returns>
        public int GetCachedProductCountV2()
        {
            try
            {
                return SettingsCacheV2.GetCacheCount();
            }
            catch (Exception ex)
            {
                LogError($"Error getting cache count: {ex.Message}");
                return 0;
            }
        }
    }
}
