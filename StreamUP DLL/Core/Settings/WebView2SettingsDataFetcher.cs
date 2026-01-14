using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // Maximum number of OBS connections Streamer.bot supports
        private const int MAX_OBS_CONNECTIONS = 5;

        /// <summary>
        /// Fetch all live data from Streamer.bot and OBS
        /// </summary>
        private JObject FetchAllLiveData()
        {
            LogInfo("Fetching live data from Streamer.bot and OBS");

            var liveData = new JObject
            {
                ["streamerBot"] = FetchStreamerBotData(),
                ["obs"] = FetchAllObsData()
            };

            return liveData;
        }

        #region Streamer.bot Data

        /// <summary>
        /// Fetch data from Streamer.bot (actions, commands, groups, rewards)
        /// </summary>
        private JObject FetchStreamerBotData()
        {
            LogInfo("Fetching Streamer.bot data");

            var data = new JObject();

            try
            {
                // Fetch actions
                data["actions"] = FetchActions();

                // Fetch commands
                data["commands"] = FetchCommands();

                // Fetch user groups
                data["userGroups"] = FetchUserGroups();

                // Fetch Twitch channel point rewards
                data["channelPointRewards"] = FetchChannelPointRewards();

                LogInfo("Streamer.bot data fetched successfully");
            }
            catch (Exception ex)
            {
                LogError($"Error fetching Streamer.bot data: {ex.Message}");
            }

            return data;
        }

        /// <summary>
        /// Fetch all actions from Streamer.bot
        /// </summary>
        private JArray FetchActions()
        {
            var actions = new JArray();

            try
            {
                var actionList = _CPH.GetActions();
                if (actionList != null)
                {
                    foreach (var action in actionList)
                    {
                        actions.Add(new JObject
                        {
                            ["id"] = action.Id.ToString(),
                            ["name"] = action.Name,
                            ["enabled"] = action.Enabled,
                            ["group"] = action.Group ?? ""
                        });
                    }
                }

                LogDebug($"Fetched {actions.Count} actions");
            }
            catch (Exception ex)
            {
                LogError($"Error fetching actions: {ex.Message}");
            }

            return actions;
        }

        /// <summary>
        /// Fetch all commands from Streamer.bot
        /// </summary>
        private JArray FetchCommands()
        {
            var commands = new JArray();

            try
            {
                var commandList = _CPH.GetCommands();
                if (commandList != null)
                {
                    foreach (var cmd in commandList)
                    {
                        var commandTriggers = new JArray();
                        if (cmd.Commands != null)
                        {
                            foreach (var trigger in cmd.Commands)
                            {
                                commandTriggers.Add(trigger);
                            }
                        }

                        commands.Add(new JObject
                        {
                            ["id"] = cmd.Id.ToString(),
                            ["name"] = cmd.Name,
                            ["enabled"] = cmd.Enabled,
                            ["group"] = cmd.Group ?? "",
                            ["commands"] = commandTriggers
                        });
                    }
                }

                LogDebug($"Fetched {commands.Count} commands");
            }
            catch (Exception ex)
            {
                LogError($"Error fetching commands: {ex.Message}");
            }

            return commands;
        }

        /// <summary>
        /// Fetch all user groups from Streamer.bot
        /// </summary>
        private JArray FetchUserGroups()
        {
            var groups = new JArray();

            try
            {
                var groupList = _CPH.GetGroups();
                if (groupList != null)
                {
                    foreach (var group in groupList)
                    {
                        groups.Add(group);
                    }
                }

                LogDebug($"Fetched {groups.Count} user groups");
            }
            catch (Exception ex)
            {
                LogError($"Error fetching user groups: {ex.Message}");
            }

            return groups;
        }

        /// <summary>
        /// Fetch all Twitch channel point rewards
        /// </summary>
        private JArray FetchChannelPointRewards()
        {
            var rewards = new JArray();

            try
            {
                var rewardList = _CPH.TwitchGetRewards();
                if (rewardList != null)
                {
                    foreach (var reward in rewardList)
                    {
                        rewards.Add(new JObject
                        {
                            ["id"] = reward.Id,
                            ["title"] = reward.Title,
                            ["cost"] = reward.Cost,
                            ["enabled"] = reward.Enabled,
                            ["paused"] = reward.Paused,
                            ["group"] = reward.Group ?? "",
                            ["backgroundColor"] = reward.BackgroundColor,
                            ["inputRequired"] = reward.InputRequired
                        });
                    }
                }

                LogDebug($"Fetched {rewards.Count} channel point rewards");
            }
            catch (Exception ex)
            {
                LogError($"Error fetching channel point rewards: {ex.Message}");
            }

            return rewards;
        }

        #endregion

        #region OBS Data

        /// <summary>
        /// Fetch data from all OBS connections
        /// </summary>
        private JObject FetchAllObsData()
        {
            LogInfo("Fetching OBS data");

            var data = new JObject();

            try
            {
                // Get connection status for all connections
                var connections = new JArray();
                var connectionsData = new JArray();

                for (int i = 0; i < MAX_OBS_CONNECTIONS; i++)
                {
                    bool isConnected = false;
                    try
                    {
                        isConnected = _CPH.ObsIsConnected(i);
                    }
                    catch (Exception ex)
                    {
                        LogDebug($"Could not check OBS connection {i}: {ex.Message}");
                    }

                    connections.Add(new JObject
                    {
                        ["index"] = i,
                        ["name"] = $"OBS Connection {i}",
                        ["connected"] = isConnected
                    });

                    // Fetch detailed data for connected instances
                    if (isConnected)
                    {
                        connectionsData.Add(FetchObsConnectionData(i));
                    }
                    else
                    {
                        connectionsData.Add(new JObject
                        {
                            ["index"] = i,
                            ["connected"] = false,
                            ["scenes"] = new JArray(),
                            ["sources"] = new JArray(),
                            ["filters"] = new JArray()
                        });
                    }
                }

                data["connections"] = connections;
                data["connectionsData"] = connectionsData;

                LogInfo("OBS data fetched successfully");
            }
            catch (Exception ex)
            {
                LogError($"Error fetching OBS data: {ex.Message}");
            }

            return data;
        }

        /// <summary>
        /// Fetch data for a specific OBS connection
        /// </summary>
        private JObject FetchObsConnectionData(int connectionIndex)
        {
            LogDebug($"Fetching OBS data for connection {connectionIndex}");

            var data = new JObject
            {
                ["index"] = connectionIndex,
                ["connected"] = false,
                ["scenes"] = new JArray(),
                ["sources"] = new JArray(),
                ["filters"] = new JArray()
            };

            try
            {
                if (!_CPH.ObsIsConnected(connectionIndex))
                {
                    LogDebug($"OBS connection {connectionIndex} is not connected");
                    return data;
                }

                data["connected"] = true;

                // Fetch scenes
                data["scenes"] = FetchObsScenes(connectionIndex);

                // Fetch sources (from all scenes)
                data["sources"] = FetchObsSources(connectionIndex);

                // Fetch filters (from all sources)
                data["filters"] = FetchObsFilters(connectionIndex);

                LogDebug($"OBS connection {connectionIndex} data fetched");
            }
            catch (Exception ex)
            {
                LogError($"Error fetching OBS connection {connectionIndex} data: {ex.Message}");
            }

            return data;
        }

        /// <summary>
        /// Fetch all scenes from an OBS connection
        /// </summary>
        private JArray FetchObsScenes(int obsConnection)
        {
            var scenes = new JArray();

            try
            {
                if (GetObsSceneList(obsConnection, out JObject sceneList))
                {
                    if (sceneList["scenes"] is JArray sceneArray)
                    {
                        foreach (var scene in sceneArray)
                        {
                            scenes.Add(new JObject
                            {
                                ["name"] = scene["sceneName"]?.ToString() ?? "",
                                ["uuid"] = scene["sceneUuid"]?.ToString() ?? ""
                            });
                        }
                    }
                }

                LogDebug($"Fetched {scenes.Count} scenes from OBS {obsConnection}");
            }
            catch (Exception ex)
            {
                LogError($"Error fetching OBS scenes: {ex.Message}");
            }

            return scenes;
        }

        /// <summary>
        /// Fetch all unique sources from an OBS connection (across all scenes)
        /// </summary>
        private JArray FetchObsSources(int obsConnection)
        {
            var sources = new JArray();
            var seenSources = new HashSet<string>();

            try
            {
                // Get scene list first
                if (!GetObsSceneList(obsConnection, out JObject sceneList))
                {
                    return sources;
                }

                if (sceneList["scenes"] is JArray sceneArray)
                {
                    foreach (var scene in sceneArray)
                    {
                        var sceneName = scene["sceneName"]?.ToString();
                        if (string.IsNullOrEmpty(sceneName)) continue;

                        // Get scene items
                        if (GetObsSceneItemsArray(sceneName, OBSSceneType.Scene, obsConnection, out JArray sceneItems))
                        {
                            foreach (JObject item in sceneItems)
                            {
                                var sourceName = item["sourceName"]?.ToString();
                                if (string.IsNullOrEmpty(sourceName) || seenSources.Contains(sourceName))
                                {
                                    continue;
                                }

                                seenSources.Add(sourceName);

                                sources.Add(new JObject
                                {
                                    ["name"] = sourceName,
                                    ["type"] = item["inputKind"]?.ToString() ?? "",
                                    ["isGroup"] = item["isGroup"] != null && item["isGroup"].Type != JTokenType.Null && item["isGroup"].Value<bool>()
                                });
                            }
                        }
                    }
                }

                LogDebug($"Fetched {sources.Count} unique sources from OBS {obsConnection}");
            }
            catch (Exception ex)
            {
                LogError($"Error fetching OBS sources: {ex.Message}");
            }

            return sources;
        }

        /// <summary>
        /// Fetch all filters from an OBS connection (across all sources)
        /// </summary>
        private JArray FetchObsFilters(int obsConnection)
        {
            var filters = new JArray();
            var processedSources = new HashSet<string>();

            try
            {
                // Get scene list
                if (!GetObsSceneList(obsConnection, out JObject sceneList))
                {
                    return filters;
                }

                if (sceneList["scenes"] is JArray sceneArray)
                {
                    foreach (var scene in sceneArray)
                    {
                        var sceneName = scene["sceneName"]?.ToString();
                        if (string.IsNullOrEmpty(sceneName)) continue;

                        // Get scene items
                        if (GetObsSceneItemsArray(sceneName, OBSSceneType.Scene, obsConnection, out JArray sceneItems))
                        {
                            foreach (JObject item in sceneItems)
                            {
                                var sourceName = item["sourceName"]?.ToString();
                                if (string.IsNullOrEmpty(sourceName) || processedSources.Contains(sourceName))
                                {
                                    continue;
                                }

                                processedSources.Add(sourceName);

                                // Get filters for this source
                                if (GetObsSourceFilterList(sourceName, obsConnection, out JArray sourceFilters))
                                {
                                    foreach (JObject filter in sourceFilters)
                                    {
                                        filters.Add(new JObject
                                        {
                                            ["sourceName"] = sourceName,
                                            ["filterName"] = filter["filterName"]?.ToString() ?? "",
                                            ["filterKind"] = filter["filterKind"]?.ToString() ?? "",
                                            ["filterEnabled"] = filter["filterEnabled"]?.Value<bool>() ?? true
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                LogDebug($"Fetched {filters.Count} filters from OBS {obsConnection}");
            }
            catch (Exception ex)
            {
                LogError($"Error fetching OBS filters: {ex.Message}");
            }

            return filters;
        }

        #endregion
    }
}
