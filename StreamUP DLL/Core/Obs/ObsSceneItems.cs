using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // ============================================================
        // OBS WEBSOCKET 5 - SCENE ITEMS
        // Easy-to-use methods for managing sources within scenes
        // ============================================================

        #region Visibility

        /// <summary>
        /// Sets the visibility of a source in a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="visible">True to show, false to hide</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetSourceVisibility(string sceneName, string sourceName, bool visible, int connection = 0)
        {
            int sceneItemId = ObsGetSceneItemId(sceneName, sourceName, connection);
            if (sceneItemId < 0) return false;
            return ObsSendRequestNoResponse("SetSceneItemEnabled", new { sceneName, sceneItemId, sceneItemEnabled = visible }, connection);
        }

        /// <summary>
        /// Shows a source in a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsShowSource(string sceneName, string sourceName, int connection = 0)
            => ObsSetSourceVisibility(sceneName, sourceName, true, connection);

        /// <summary>
        /// Hides a source in a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsHideSource(string sceneName, string sourceName, int connection = 0)
            => ObsSetSourceVisibility(sceneName, sourceName, false, connection);

        /// <summary>
        /// Gets the visibility state of a source in a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if visible, false if hidden or not found</returns>
        public bool ObsIsSourceVisible(string sceneName, string sourceName, int connection = 0)
        {
            int sceneItemId = ObsGetSceneItemId(sceneName, sourceName, connection);
            if (sceneItemId < 0) return false;
            var response = ObsSendRequest("GetSceneItemEnabled", new { sceneName, sceneItemId }, connection);
            return response?["sceneItemEnabled"]?.Value<bool>() ?? false;
        }

        /// <summary>
        /// Sets visibility using scene item ID directly (faster if you already have the ID).
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sceneItemId">Scene item ID</param>
        /// <param name="visible">True to show, false to hide</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetSceneItemEnabled(string sceneName, int sceneItemId, bool visible, int connection = 0)
            => ObsSendRequestNoResponse("SetSceneItemEnabled", new { sceneName, sceneItemId, sceneItemEnabled = visible }, connection);

        /// <summary>
        /// Toggles the visibility of a source in a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>New visibility state (true if now visible), or null if failed</returns>
        public bool? ObsToggleSourceVisibility(string sceneName, string sourceName, int connection = 0)
        {
            bool currentState = ObsIsSourceVisible(sceneName, sourceName, connection);
            if (ObsSetSourceVisibility(sceneName, sourceName, !currentState, connection))
                return !currentState;
            return null;
        }

        #endregion

        #region Transform

        /// <summary>
        /// Gets the transform data for a source in a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Transform data as JObject, or null if not found</returns>
        public JObject ObsGetSourceTransform(string sceneName, string sourceName, int connection = 0)
        {
            int sceneItemId = ObsGetSceneItemId(sceneName, sourceName, connection);
            if (sceneItemId < 0) return null;
            var response = ObsSendRequest("GetSceneItemTransform", new { sceneName, sceneItemId }, connection);
            return response?["sceneItemTransform"] as JObject;
        }

        /// <summary>
        /// Gets the transform data for a scene item (alias for ObsGetSourceTransform).
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Transform data as JObject, or null if not found</returns>
        public JObject ObsGetSceneItemTransform(string sceneName, string sourceName, int connection = 0)
            => ObsGetSourceTransform(sceneName, sourceName, connection);

        /// <summary>
        /// Sets the transform data for a source in a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="transform">Transform object with properties to set</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetSourceTransform(string sceneName, string sourceName, object transform, int connection = 0)
        {
            int sceneItemId = ObsGetSceneItemId(sceneName, sourceName, connection);
            if (sceneItemId < 0) return false;
            return ObsSendRequestNoResponse("SetSceneItemTransform", new { sceneName, sceneItemId, sceneItemTransform = transform }, connection);
        }

        /// <summary>
        /// Sets the position of a source in a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetSourcePosition(string sceneName, string sourceName, double x, double y, int connection = 0)
            => ObsSetSourceTransform(sceneName, sourceName, new { positionX = x, positionY = y }, connection);

        /// <summary>
        /// Sets the size of a source in a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetSourceSize(string sceneName, string sourceName, double width, double height, int connection = 0)
            => ObsSetSourceTransform(sceneName, sourceName, new { width, height }, connection);

        /// <summary>
        /// Sets the scale of a source in a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="scaleX">X scale factor (1.0 = 100%)</param>
        /// <param name="scaleY">Y scale factor (1.0 = 100%)</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetSourceScale(string sceneName, string sourceName, double scaleX, double scaleY, int connection = 0)
            => ObsSetSourceTransform(sceneName, sourceName, new { scaleX, scaleY }, connection);

        /// <summary>
        /// Sets the rotation of a source in a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="rotation">Rotation angle in degrees</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetSourceRotation(string sceneName, string sourceName, double rotation, int connection = 0)
            => ObsSetSourceTransform(sceneName, sourceName, new { rotation }, connection);

        /// <summary>
        /// Sets the crop of a source in a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="top">Top crop in pixels</param>
        /// <param name="bottom">Bottom crop in pixels</param>
        /// <param name="left">Left crop in pixels</param>
        /// <param name="right">Right crop in pixels</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetSourceCrop(string sceneName, string sourceName, int top, int bottom, int left, int right, int connection = 0)
            => ObsSetSourceTransform(sceneName, sourceName, new { cropTop = top, cropBottom = bottom, cropLeft = left, cropRight = right }, connection);

        #endregion

        #region Locking

        /// <summary>
        /// Sets the locked state of a source in a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="locked">True to lock, false to unlock</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetSourceLocked(string sceneName, string sourceName, bool locked, int connection = 0)
        {
            int sceneItemId = ObsGetSceneItemId(sceneName, sourceName, connection);
            if (sceneItemId < 0) return false;
            return ObsSendRequestNoResponse("SetSceneItemLocked", new { sceneName, sceneItemId, sceneItemLocked = locked }, connection);
        }

        /// <summary>
        /// Gets whether a source is locked in a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if locked, false if unlocked or not found</returns>
        public bool ObsIsSourceLocked(string sceneName, string sourceName, int connection = 0)
        {
            int sceneItemId = ObsGetSceneItemId(sceneName, sourceName, connection);
            if (sceneItemId < 0) return false;
            var response = ObsSendRequest("GetSceneItemLocked", new { sceneName, sceneItemId }, connection);
            return response?["sceneItemLocked"]?.Value<bool>() ?? false;
        }

        #endregion

        #region Index/Order

        /// <summary>
        /// Gets the index (z-order) of a source in a scene. 0 is bottom.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Scene item index, or -1 if not found</returns>
        public int ObsGetSourceIndex(string sceneName, string sourceName, int connection = 0)
        {
            int sceneItemId = ObsGetSceneItemId(sceneName, sourceName, connection);
            if (sceneItemId < 0) return -1;
            var response = ObsSendRequest("GetSceneItemIndex", new { sceneName, sceneItemId }, connection);
            return response?["sceneItemIndex"]?.Value<int>() ?? -1;
        }

        /// <summary>
        /// Sets the index (z-order) of a source in a scene. 0 is bottom.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="index">New index position</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetSourceIndex(string sceneName, string sourceName, int index, int connection = 0)
        {
            int sceneItemId = ObsGetSceneItemId(sceneName, sourceName, connection);
            if (sceneItemId < 0) return false;
            return ObsSendRequestNoResponse("SetSceneItemIndex", new { sceneName, sceneItemId, sceneItemIndex = index }, connection);
        }

        #endregion

        #region Blend Mode

        /// <summary>
        /// Gets the blend mode of a source in a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Blend mode string, or null if not found</returns>
        public string ObsGetSourceBlendMode(string sceneName, string sourceName, int connection = 0)
        {
            int sceneItemId = ObsGetSceneItemId(sceneName, sourceName, connection);
            if (sceneItemId < 0) return null;
            var response = ObsSendRequest("GetSceneItemBlendMode", new { sceneName, sceneItemId }, connection);
            return response?["sceneItemBlendMode"]?.Value<string>();
        }

        /// <summary>
        /// Sets the blend mode of a source in a scene.
        /// Blend modes: OBS_BLEND_NORMAL, OBS_BLEND_ADDITIVE, OBS_BLEND_SUBTRACT,
        /// OBS_BLEND_SCREEN, OBS_BLEND_MULTIPLY, OBS_BLEND_LIGHTEN, OBS_BLEND_DARKEN
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source</param>
        /// <param name="blendMode">Blend mode constant</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsSetSourceBlendMode(string sceneName, string sourceName, string blendMode, int connection = 0)
        {
            int sceneItemId = ObsGetSceneItemId(sceneName, sourceName, connection);
            if (sceneItemId < 0) return false;
            return ObsSendRequestNoResponse("SetSceneItemBlendMode", new { sceneName, sceneItemId, sceneItemBlendMode = blendMode }, connection);
        }

        #endregion

        #region Scene Item Management

        /// <summary>
        /// Gets a list of all scene items in a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Array of scene items, or null if failed</returns>
        public JArray ObsGetSceneItemList(string sceneName, int connection = 0)
        {
            var response = ObsSendRequest("GetSceneItemList", new { sceneName }, connection);
            return response?["sceneItems"] as JArray;
        }

        /// <summary>
        /// Creates a new scene item (adds an existing source to a scene).
        /// </summary>
        /// <param name="sceneName">Name of the scene to add the source to</param>
        /// <param name="sourceName">Name of the source to add</param>
        /// <param name="enabled">Whether the item should be visible initially</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Scene item ID of the created item, or -1 if failed</returns>
        public int ObsCreateSceneItem(string sceneName, string sourceName, bool enabled = true, int connection = 0)
        {
            var response = ObsSendRequest("CreateSceneItem", new { sceneName, sourceName, sceneItemEnabled = enabled }, connection);
            return response?["sceneItemId"]?.Value<int>() ?? -1;
        }

        /// <summary>
        /// Removes a scene item from a scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene</param>
        /// <param name="sourceName">Name of the source to remove</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>True if successful</returns>
        public bool ObsRemoveSceneItem(string sceneName, string sourceName, int connection = 0)
        {
            int sceneItemId = ObsGetSceneItemId(sceneName, sourceName, connection);
            if (sceneItemId < 0) return false;
            return ObsSendRequestNoResponse("RemoveSceneItem", new { sceneName, sceneItemId }, connection);
        }

        /// <summary>
        /// Duplicates a scene item to the same or different scene.
        /// </summary>
        /// <param name="sceneName">Name of the source scene</param>
        /// <param name="sourceName">Name of the source to duplicate</param>
        /// <param name="destinationSceneName">Name of destination scene (null for same scene)</param>
        /// <param name="connection">OBS connection index (0-4)</param>
        /// <returns>Scene item ID of the duplicate, or -1 if failed</returns>
        public int ObsDuplicateSceneItem(string sceneName, string sourceName, string destinationSceneName = null, int connection = 0)
        {
            int sceneItemId = ObsGetSceneItemId(sceneName, sourceName, connection);
            if (sceneItemId < 0) return -1;
            var response = ObsSendRequest("DuplicateSceneItem", new { sceneName, sceneItemId, destinationSceneName }, connection);
            return response?["sceneItemId"]?.Value<int>() ?? -1;
        }

        #endregion
    }
}
