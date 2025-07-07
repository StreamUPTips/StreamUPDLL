using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // --- Scene Items Requests (from protocol) as build methods ---
        /// <summary>
        /// Gets an array of all scene items in a scene.
        /// <paramref name="sceneName">Name of the scene to get the items from.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getsceneitemlist
        /// </summary>
        public JObject ObsRawGetSceneItemList(string sceneName) => BuildObsRequest("GetSceneItemList", new { sceneName });

        /// <summary>
        /// Gets an array of all scene items in a group.
        /// <paramref name="sceneName">Name of the group to get the items from.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getgroupsceneitemlist
        /// </summary>
        public JObject ObsRawGetGroupSceneItemList(string sceneName) => BuildObsRequest("GetGroupSceneItemList", new { sceneName });

        /// <summary>
        /// Gets the ID of a scene item.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sourceName">Name of the source (input or scene) the item refers to.</paramref>
        /// <paramref name="searchOffset">Optional offset for search (used for duplicate items).</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getsceneitemid
        /// </summary>
        public JObject ObsRawGetSceneItemId(string sceneName, string sourceName, int? searchOffset = null) => BuildObsRequest("GetSceneItemId", new { sceneName, sourceName, searchOffset });

        /// <summary>
        /// Creates a new scene item in a scene.
        /// <paramref name="sceneName">Name of the scene to add the item to.</paramref>
        /// <paramref name="sceneItem">Object containing the scene item data.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=createsceneitem
        /// </summary>
        public JObject ObsRawCreateSceneItem(string sceneName, object sceneItem) => BuildObsRequest("CreateSceneItem", new { sceneName, sceneItem });

        /// <summary>
        /// Removes a scene item from a scene.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to remove.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=removesceneitem
        /// </summary>
        public JObject ObsRawRemoveSceneItem(string sceneName, int sceneItemId) => BuildObsRequest("RemoveSceneItem", new { sceneName, sceneItemId });

        /// <summary>
        /// Duplicates a scene item to another scene.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to duplicate.</paramref>
        /// <paramref name="destinationSceneName">Optional name of the destination scene. If not specified, duplicates within the same scene.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=duplicatesceneitem
        /// </summary>
        public JObject ObsRawDuplicateSceneItem(string sceneName, int sceneItemId, string destinationSceneName = null) => BuildObsRequest("DuplicateSceneItem", new { sceneName, sceneItemId, destinationSceneName });

        /// <summary>
        /// Gets the transform data of a scene item.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to get the transform data for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=getsceneitemtransform
        /// </summary>
        public JObject ObsRawGetSceneItemTransform(string sceneName, int sceneItemId) => BuildObsRequest("GetSceneItemTransform", new { sceneName, sceneItemId });

        /// <summary>
        /// Sets the transform data of a scene item.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to set the transform data for.</paramref>
        /// <paramref name="sceneItemTransform">Object containing the new transform data.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=setsceneitemtransform
        /// </summary>
        public JObject ObsRawSetSceneItemTransform(string sceneName, int sceneItemId, object sceneItemTransform) => BuildObsRequest("SetSceneItemTransform", new { sceneName, sceneItemId, sceneItemTransform });

        /// <summary>
        /// Gets the enabled state of a scene item.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to get the enabled state of.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=getsceneitemenabled
        /// </summary>
        public JObject ObsRawGetSceneItemEnabled(string sceneName, int sceneItemId) => BuildObsRequest("GetSceneItemEnabled", new { sceneName, sceneItemId });

        /// <summary>
        /// Sets the enabled state of a scene item.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to set the enabled state for.</paramref>
        /// <paramref name="sceneItemEnabled">Whether the scene item should be enabled.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=setsceneitemenabled
        /// </summary>
        public JObject ObsRawSetSceneItemEnabled(string sceneName, int sceneItemId, bool sceneItemEnabled) => BuildObsRequest("SetSceneItemEnabled", new { sceneName, sceneItemId, sceneItemEnabled });

        /// <summary>
        /// Gets the locked state of a scene item.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to get the locked state of.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=getsceneitemlocked
        /// </summary>
        public JObject ObsRawGetSceneItemLocked(string sceneName, int sceneItemId) => BuildObsRequest("GetSceneItemLocked", new { sceneName, sceneItemId });

        /// <summary>
        /// Sets the locked state of a scene item.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to set the locked state for.</paramref>
        /// <paramref name="sceneItemLocked">Whether the scene item should be locked.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=setsceneitemlocked
        /// </summary>
        public JObject ObsRawSetSceneItemLocked(string sceneName, int sceneItemId, bool sceneItemLocked) => BuildObsRequest("SetSceneItemLocked", new { sceneName, sceneItemId, sceneItemLocked });

        /// <summary>
        /// Gets the index of a scene item in the scene item list.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to get the index of.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=getsceneitemindex
        /// </summary>
        public JObject ObsRawGetSceneItemIndex(string sceneName, int sceneItemId) => BuildObsRequest("GetSceneItemIndex", new { sceneName, sceneItemId });

        /// <summary>
        /// Sets the index of a scene item in the scene item list.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to set the index for.</paramref>
        /// <paramref name="sceneItemIndex">New index for the scene item.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=setsceneitemindex
        /// </summary>
        public JObject ObsRawSetSceneItemIndex(string sceneName, int sceneItemId, int sceneItemIndex) => BuildObsRequest("SetSceneItemIndex", new { sceneName, sceneItemId, sceneItemIndex });

        /// <summary>
        /// Gets the blend mode of a scene item.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to get the blend mode of.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=getsceneitemblendmode
        /// </summary>
        public JObject ObsRawGetSceneItemBlendMode(string sceneName, int sceneItemId) => BuildObsRequest("GetSceneItemBlendMode", new { sceneName, sceneItemId });

        /// <summary>
        /// Sets the blend mode of a scene item.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to set the blend mode for.</paramref>
        /// <paramref name="sceneItemBlendMode">New blend mode for the scene item.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md=setsceneitemblendmode
        /// </summary>
        public JObject ObsRawSetSceneItemBlendMode(string sceneName, int sceneItemId, string sceneItemBlendMode) => BuildObsRequest("SetSceneItemBlendMode", new { sceneName, sceneItemId, sceneItemBlendMode });

        /// <summary>
        /// Gets the source (input or scene) associated with a scene item.
        /// <paramref name="sceneName">Name of the scene the item is in.</paramref>
        /// <paramref name="sceneItemId">ID of the scene item to get the source for.</paramref>
        /// See: https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md#getsceneitemsource
        /// </summary>
        public JObject ObsRawGetSceneItemSource(string sceneName, int sceneItemId) => BuildObsRequest("GetSceneItemSource", new { sceneName, sceneItemId });
    }
}
