using System.Collections.Generic;
using System.Linq;

namespace StreamUP
{
    /// <summary>
    /// Manages prompt state to prevent spamming users with duplicate dialogs.
    /// State is session-scoped and clears when Streamer.bot restarts.
    /// </summary>
    public static class PromptManager
    {
        // Session-scoped tracking - cleared when Streamer.bot restarts
        private static readonly HashSet<string> _promptedIssues = new HashSet<string>();

        // Lock for thread safety
        private static readonly object _lock = new object();

        /// <summary>
        /// Check if we've already prompted the user about a specific issue
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="issueKey">Issue type (e.g., "settings_missing", "obs_disconnected")</param>
        /// <returns>True if already prompted this session</returns>
        public static bool HasBeenPrompted(string productNumber, string issueKey)
        {
            lock (_lock)
            {
                var key = $"{productNumber}:{issueKey}";
                var result = _promptedIssues.Contains(key);
                System.Diagnostics.Debug.WriteLine($"[StreamUP PromptManager] HasBeenPrompted({key}) = {result}. All prompts: [{string.Join(", ", _promptedIssues)}]");
                return result;
            }
        }

        /// <summary>
        /// Mark that we've prompted the user about a specific issue
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="issueKey">Issue type</param>
        public static void MarkAsPrompted(string productNumber, string issueKey)
        {
            lock (_lock)
            {
                _promptedIssues.Add($"{productNumber}:{issueKey}");
            }
        }

        /// <summary>
        /// Clear all prompts for a specific product (e.g., when settings are saved)
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        public static void ClearPrompts(string productNumber)
        {
            lock (_lock)
            {
                var toRemove = _promptedIssues
                    .Where(x => x.StartsWith($"{productNumber}:"))
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"[StreamUP PromptManager] ClearPrompts({productNumber}) - Found {toRemove.Count} items to remove: [{string.Join(", ", toRemove)}]");
                System.Diagnostics.Debug.WriteLine($"[StreamUP PromptManager] Current prompts before clear: [{string.Join(", ", _promptedIssues)}]");

                foreach (var item in toRemove)
                {
                    _promptedIssues.Remove(item);
                }

                System.Diagnostics.Debug.WriteLine($"[StreamUP PromptManager] Prompts after clear: [{string.Join(", ", _promptedIssues)}]");
            }
        }

        /// <summary>
        /// Clear a specific prompt for a product (e.g., when the issue is resolved)
        /// </summary>
        /// <param name="productNumber">Product identifier</param>
        /// <param name="issueKey">Issue type to clear</param>
        public static void ClearPrompt(string productNumber, string issueKey)
        {
            lock (_lock)
            {
                _promptedIssues.Remove($"{productNumber}:{issueKey}");
            }
        }

        /// <summary>
        /// Clear all prompts for all products
        /// </summary>
        public static void ClearAllPrompts()
        {
            lock (_lock)
            {
                _promptedIssues.Clear();
            }
        }

        // Issue key constants for consistency
        public static class IssueKeys
        {
            public const string SettingsMissing = "settings_missing";
            public const string LibraryOutdated = "library_outdated";
            public const string ObsDisconnected = "obs_disconnected";
            public const string ObsSceneMissing = "obs_scene_missing";
            public const string ObsSceneOutdated = "obs_scene_outdated";
        }
    }
}
