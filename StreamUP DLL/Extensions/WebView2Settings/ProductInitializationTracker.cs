using System;
using System.Collections.Generic;

namespace StreamUP
{
    /// <summary>
    /// Static tracker for product initialization prompts and OBS warnings.
    /// Prevents spam by tracking which prompts have been shown this session.
    /// Resets only on Streamer.bot restart.
    /// </summary>
    public static class ProductInitializationTracker
    {
        private static readonly object _lock = new object();
        private static readonly HashSet<string> _promptedProducts = new HashSet<string>();
        private static readonly HashSet<string> _obsWarnedProducts = new HashSet<string>();
        private static readonly HashSet<string> _versionWarnedProducts = new HashSet<string>();

        /// <summary>
        /// Check if initialization prompt should be shown for a product.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <returns>True if should show prompt, false if already shown this session</returns>
        public static bool ShouldShowInitializationPrompt(string productNumber)
        {
            if (string.IsNullOrEmpty(productNumber))
                return false;

            lock (_lock)
            {
                return !_promptedProducts.Contains(productNumber);
            }
        }

        /// <summary>
        /// Mark a product as having shown the initialization prompt this session.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        public static void MarkInitializationPromptShown(string productNumber)
        {
            if (string.IsNullOrEmpty(productNumber))
                return;

            lock (_lock)
            {
                _promptedProducts.Add(productNumber);
            }
        }

        /// <summary>
        /// Check if OBS warning should be shown for a product.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <returns>True if should show warning, false if already shown this session</returns>
        public static bool ShouldShowObsWarning(string productNumber)
        {
            if (string.IsNullOrEmpty(productNumber))
                return false;

            lock (_lock)
            {
                return !_obsWarnedProducts.Contains(productNumber);
            }
        }

        /// <summary>
        /// Mark a product as having shown the OBS warning this session.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        public static void MarkObsWarningShown(string productNumber)
        {
            if (string.IsNullOrEmpty(productNumber))
                return;

            lock (_lock)
            {
                _obsWarnedProducts.Add(productNumber);
            }
        }

        /// <summary>
        /// Get count of products that have shown initialization prompts.
        /// </summary>
        /// <returns>Number of prompted products</returns>
        public static int GetPromptedProductCount()
        {
            lock (_lock)
            {
                return _promptedProducts.Count;
            }
        }

        /// <summary>
        /// Get count of products that have shown OBS warnings.
        /// </summary>
        /// <returns>Number of warned products</returns>
        public static int GetObsWarnedProductCount()
        {
            lock (_lock)
            {
                return _obsWarnedProducts.Count;
            }
        }

        /// <summary>
        /// Check if version warning should be shown for a product.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <returns>True if should show warning, false if already shown this session</returns>
        public static bool ShouldShowVersionWarning(string productNumber)
        {
            if (string.IsNullOrEmpty(productNumber))
                return false;

            lock (_lock)
            {
                return !_versionWarnedProducts.Contains(productNumber);
            }
        }

        /// <summary>
        /// Mark a product as having shown the version warning this session.
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        public static void MarkVersionWarningShown(string productNumber)
        {
            if (string.IsNullOrEmpty(productNumber))
                return;

            lock (_lock)
            {
                _versionWarnedProducts.Add(productNumber);
            }
        }

        /// <summary>
        /// Clear all tracking data. Use only for testing.
        /// </summary>
        internal static void ClearAllTracking()
        {
            lock (_lock)
            {
                _promptedProducts.Clear();
                _obsWarnedProducts.Clear();
                _versionWarnedProducts.Clear();
            }
        }
    }
}
