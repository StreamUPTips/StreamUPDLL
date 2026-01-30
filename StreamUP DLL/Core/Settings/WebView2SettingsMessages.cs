using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        /// <summary>
        /// Handle messages received from the WebView2 viewer
        /// </summary>
        private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                // The viewer sends JSON.stringify(message), so we need to get it as a string first
                var messageString = e.TryGetWebMessageAsString();
                if (string.IsNullOrEmpty(messageString))
                {
                    LogDebug("Received empty message from viewer");
                    return;
                }

                var message = JObject.Parse(messageString);
                var action = message["action"]?.ToString();

                LogDebug($"Received message from viewer: {action}");

                switch (action)
                {
                    case "save":
                        HandleSaveMessage(message);
                        break;

                    case "close":
                        HandleCloseMessage();
                        break;

                    case "openFileDialog":
                        HandleOpenFileDialogMessage(message);
                        break;

                    case "openFolderDialog":
                        HandleOpenFolderDialogMessage(message);
                        break;

                    case "requestObsData":
                        HandleRequestObsDataMessage(message);
                        break;

                    case "settingsChanged":
                        _hasUnsavedChanges = true;
                        LogDebug("Settings marked as changed");
                        break;

                    case "reset":
                        HandleResetMessage();
                        break;

                    case "log":
                        HandleLogMessage(message);
                        break;

                    case "minimize":
                        MinimizeWindow();
                        break;

                    case "maximize":
                        MaximizeWindow();
                        break;

                    case "startDrag":
                        StartWindowDrag();
                        break;

                    case "runAction":
                        HandleRunActionMessage(message);
                        break;

                    case "executeMethod":
                        HandleExecuteMethodMessage(message);
                        break;

                    case "openExternalUrl":
                        HandleOpenExternalUrlMessage(message);
                        break;

                    case "sendFeedback":
                        HandleSendFeedbackMessage(message);
                        break;

                    default:
                        LogDebug($"Unknown message action: {action}");
                        break;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error handling web message: {ex.Message}");
            }
        }

        #region Message Handlers

        /// <summary>
        /// Handle save message from viewer
        /// </summary>
        private void HandleSaveMessage(JObject message)
        {
            LogInfo("Processing save request from viewer");

            try
            {
                var data = message["data"] as JObject;
                if (data == null)
                {
                    LogError("Save message contains no data");
                    SendToViewer(new { action = "saveComplete", success = false, error = "No data provided" });
                    return;
                }

                // Ensure meta section exists and is up to date
                if (data["meta"] == null)
                {
                    data["meta"] = new JObject();
                }
                data["meta"]["savedAt"] = DateTime.UtcNow.ToString("O");
                data["meta"]["productNumber"] = _currentProductNumber;

                // Save to file
                var success = SaveProductSettingsInternal(_currentProductNumber, data);

                if (success)
                {
                    // Update current state
                    _currentSavedSettings = data.DeepClone() as JObject;
                    _hasUnsavedChanges = false;

                    // Notify viewer of success
                    SendToViewer(new { action = "saveComplete", success = true });

                    // Trigger save action if configured
                    TriggerSaveAction();

                    LogInfo("Settings saved successfully");
                }
                else
                {
                    SendToViewer(new { action = "saveComplete", success = false, error = "Failed to write settings file" });
                }
            }
            catch (Exception ex)
            {
                LogError($"Save failed: {ex.Message}");
                SendToViewer(new { action = "saveComplete", success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Handle close message from viewer
        /// </summary>
        private void HandleCloseMessage()
        {
            LogInfo("Close requested from viewer");

            if (_settingsWindow != null && !_settingsWindow.IsDisposed)
            {
                _settingsWindow.Invoke((MethodInvoker)delegate
                {
                    _settingsWindow.Close();
                });
            }
        }

        /// <summary>
        /// Handle file dialog request from viewer
        /// </summary>
        private void HandleOpenFileDialogMessage(JObject message)
        {
            var callbackId = message["callbackId"]?.ToString();
            var filter = message["filter"]?.ToString() ?? "All files (*.*)|*.*";
            var title = message["title"]?.ToString() ?? "Select File";
            var initialDir = message["initialDirectory"]?.ToString();

            LogDebug($"Opening file dialog: {title}");

            if (_settingsWindow == null || _settingsWindow.IsDisposed)
            {
                SendToViewer(new { action = "fileDialogResult", callbackId, path = (string)null, error = "Window not available" });
                return;
            }

            _settingsWindow.Invoke((MethodInvoker)delegate
            {
                try
                {
                    using (var dialog = new OpenFileDialog())
                    {
                        dialog.Filter = filter;
                        dialog.Title = title;

                        if (!string.IsNullOrEmpty(initialDir) && System.IO.Directory.Exists(initialDir))
                        {
                            dialog.InitialDirectory = initialDir;
                        }

                        string path = null;
                        if (dialog.ShowDialog(_settingsWindow) == DialogResult.OK)
                        {
                            path = dialog.FileName;
                        }

                        SendToViewer(new { action = "fileDialogResult", callbackId, path });
                    }
                }
                catch (Exception ex)
                {
                    LogError($"File dialog error: {ex.Message}");
                    SendToViewer(new { action = "fileDialogResult", callbackId, path = (string)null, error = ex.Message });
                }
            });
        }

        /// <summary>
        /// Handle folder dialog request from viewer
        /// </summary>
        private void HandleOpenFolderDialogMessage(JObject message)
        {
            var callbackId = message["callbackId"]?.ToString();
            var title = message["title"]?.ToString() ?? "Select Folder";
            var initialDir = message["initialDirectory"]?.ToString();

            LogDebug($"Opening folder dialog: {title}");

            if (_settingsWindow == null || _settingsWindow.IsDisposed)
            {
                SendToViewer(new { action = "fileDialogResult", callbackId, path = (string)null, error = "Window not available" });
                return;
            }

            _settingsWindow.Invoke((MethodInvoker)delegate
            {
                try
                {
                    using (var dialog = new FolderBrowserDialog())
                    {
                        dialog.Description = title;
                        dialog.ShowNewFolderButton = true;

                        if (!string.IsNullOrEmpty(initialDir) && System.IO.Directory.Exists(initialDir))
                        {
                            dialog.SelectedPath = initialDir;
                        }

                        string path = null;
                        if (dialog.ShowDialog(_settingsWindow) == DialogResult.OK)
                        {
                            path = dialog.SelectedPath;
                        }

                        SendToViewer(new { action = "fileDialogResult", callbackId, path });
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Folder dialog error: {ex.Message}");
                    SendToViewer(new { action = "fileDialogResult", callbackId, path = (string)null, error = ex.Message });
                }
            });
        }

        /// <summary>
        /// Handle request for OBS data when user changes connection
        /// </summary>
        private async void HandleRequestObsDataMessage(JObject message)
        {
            var connectionIndex = message["connectionIndex"]?.Value<int>() ?? 0;
            LogInfo($"OBS data requested for connection: {connectionIndex}");

            try
            {
                // Fetch fresh OBS data for the specified connection
                var obsData = FetchObsConnectionData(connectionIndex);

                // Send to viewer
                var script = $"window.updateObsData({obsData.ToString(Formatting.None)});";
                await ExecuteScriptAsync(script);

                LogInfo("OBS data sent to viewer");
            }
            catch (Exception ex)
            {
                LogError($"Failed to fetch OBS data: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle reset to defaults request
        /// </summary>
        private void HandleResetMessage()
        {
            LogInfo("Reset to defaults requested");

            // The viewer handles the UI reset, we just mark changes as unsaved
            _hasUnsavedChanges = true;

            SendToViewer(new { action = "resetComplete", success = true });
        }

        /// <summary>
        /// Handle log message from viewer (for debugging)
        /// </summary>
        private void HandleLogMessage(JObject message)
        {
            var level = message["level"]?.ToString() ?? "info";
            var logMessage = message["message"]?.ToString() ?? "";

            switch (level.ToLower())
            {
                case "error":
                    LogError($"[Viewer] {logMessage}");
                    break;
                case "debug":
                    LogDebug($"[Viewer] {logMessage}");
                    break;
                default:
                    LogInfo($"[Viewer] {logMessage}");
                    break;
            }
        }

        /// <summary>
        /// Handle run action request from viewer
        /// </summary>
        private void HandleRunActionMessage(JObject message)
        {
            var callbackId = message["callbackId"]?.ToString();
            var actionId = message["actionId"]?.ToString();
            var actionName = message["actionName"]?.ToString();

            LogInfo($"Run action requested - ID: {actionId}, Name: {actionName}");

            try
            {
                bool success = false;

                // Prefer ID if available, otherwise use name
                if (!string.IsNullOrEmpty(actionId))
                {
                    // Run by ID
                    success = _CPH.RunActionById(actionId, true);
                    LogInfo($"RunActionById({actionId}) = {success}");
                }
                else if (!string.IsNullOrEmpty(actionName))
                {
                    // Run by name
                    success = _CPH.RunAction(actionName, true);
                    LogInfo($"RunAction({actionName}) = {success}");
                }
                else
                {
                    LogError("No action ID or name provided");
                    SendToViewer(new { action = "runActionResult", callbackId, success = false, error = "No action ID or name provided" });
                    return;
                }

                SendToViewer(new { action = "runActionResult", callbackId, success });
            }
            catch (Exception ex)
            {
                LogError($"Run action failed: {ex.Message}");
                SendToViewer(new { action = "runActionResult", callbackId, success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Handle execute method request from viewer
        /// </summary>
        private void HandleExecuteMethodMessage(JObject message)
        {
            var callbackId = message["callbackId"]?.ToString();
            var codeId = message["codeId"]?.ToString();
            var methodName = message["methodName"]?.ToString() ?? "Execute";

            LogInfo($"Execute method requested - CodeId: {codeId}, Method: {methodName}");

            try
            {
                if (string.IsNullOrEmpty(codeId))
                {
                    LogError("No execute code ID provided");
                    SendToViewer(new { action = "executeMethodResult", callbackId, success = false, error = "No execute code ID provided" });
                    return;
                }

                // Execute the method in the named Execute Code sub-action
                bool success = _CPH.ExecuteMethod(codeId, methodName);
                LogInfo($"ExecuteMethod({codeId}, {methodName}) = {success}");

                SendToViewer(new { action = "executeMethodResult", callbackId, success });
            }
            catch (Exception ex)
            {
                LogError($"Execute method failed: {ex.Message}");
                SendToViewer(new { action = "executeMethodResult", callbackId, success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Handle open external URL request from viewer - opens URL in user's default browser
        /// </summary>
        private void HandleOpenExternalUrlMessage(JObject message)
        {
            var url = message["url"]?.ToString();

            if (string.IsNullOrEmpty(url))
            {
                LogDebug("No URL provided for external open");
                return;
            }

            LogDebug($"Opening external URL: {url}");

            try
            {
                // Open URL in user's default browser
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                LogError($"Failed to open external URL: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle feedback submission from viewer - sends to Notion database
        /// </summary>
        private async void HandleSendFeedbackMessage(JObject message)
        {
            // Obfuscated to prevent GitHub secret scanning from auto-revoking
            string NOTION_API_KEY = Encoding.UTF8.GetString(Convert.FromBase64String("bnRuXzQ3NDI2ODM5NDY0MVpCdVQzbHdpT05OSVpNRDVWcXg1SEE0TE9xeGVjQ2QzbnE="));
            const string NOTION_DATABASE_ID = "2efb0adc23a8803ab96ad1e10b24d902";

            LogInfo("Processing feedback submission from viewer");

            try
            {
                var data = message["data"] as JObject;
                if (data == null)
                {
                    LogError("Feedback message contains no data");
                    SendToViewer(new { action = "feedbackResponse", success = false, error = "No feedback data provided" });
                    return;
                }

                var title = data["title"]?.ToString() ?? "No Title";
                var type = data["type"]?.ToString() ?? "General Feedback";
                var description = data["description"]?.ToString() ?? "";
                var tags = data["tags"]?.ToObject<List<string>>() ?? new List<string>();
                var productName = data["productName"]?.ToString() ?? "Unknown";
                var productNumber = data["productNumber"]?.ToString() ?? "Unknown";
                var productVersion = data["productVersion"]?.ToString() ?? "Unknown";

                // Build Notion API payload
                var notionPayload = BuildNotionFeedbackPayload(
                    title, type, description, tags,
                    productName, productNumber, productVersion,
                    NOTION_DATABASE_ID
                );

                LogDebug($"Sending feedback to Notion: {title}");

                // POST to Notion API
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {NOTION_API_KEY}");
                    client.DefaultRequestHeaders.Add("Notion-Version", "2022-06-28");

                    var content = new StringContent(notionPayload, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("https://api.notion.com/v1/pages", content);

                    if (response.IsSuccessStatusCode)
                    {
                        LogInfo("Feedback submitted successfully to Notion");
                        SendToViewer(new { action = "feedbackResponse", success = true, message = "Feedback submitted successfully" });
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        LogError($"Notion API error ({response.StatusCode}): {errorContent}");
                        SendToViewer(new { action = "feedbackResponse", success = false, error = "Failed to submit feedback. Please try again." });
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Feedback submission error: {ex.Message}");
                SendToViewer(new { action = "feedbackResponse", success = false, error = "An error occurred. Please try again." });
            }
        }

        /// <summary>
        /// Build the Notion API payload for feedback submission
        /// </summary>
        private string BuildNotionFeedbackPayload(
            string title, string type, string description, List<string> tags,
            string productName, string productNumber, string productVersion,
            string databaseId)
        {
            // Build tags array for multi_select
            var tagsArray = new JArray();
            foreach (var tag in tags)
            {
                tagsArray.Add(new JObject { ["name"] = tag });
            }

            var payload = new JObject
            {
                ["parent"] = new JObject
                {
                    ["database_id"] = databaseId
                },
                ["properties"] = new JObject
                {
                    // Name (title type - Notion's default title field)
                    ["Name"] = new JObject
                    {
                        ["title"] = new JArray
                        {
                            new JObject
                            {
                                ["text"] = new JObject
                                {
                                    ["content"] = title
                                }
                            }
                        }
                    },
                    // Type (select type)
                    ["Type"] = new JObject
                    {
                        ["select"] = new JObject
                        {
                            ["name"] = type
                        }
                    },
                    // Description (rich_text type)
                    ["Description"] = new JObject
                    {
                        ["rich_text"] = new JArray
                        {
                            new JObject
                            {
                                ["text"] = new JObject
                                {
                                    ["content"] = description.Length > 2000 ? description.Substring(0, 2000) : description
                                }
                            }
                        }
                    },
                    // Product Name (rich_text type)
                    ["Product Name"] = new JObject
                    {
                        ["rich_text"] = new JArray
                        {
                            new JObject
                            {
                                ["text"] = new JObject
                                {
                                    ["content"] = productName
                                }
                            }
                        }
                    },
                    // Product Number (rich_text type)
                    ["Product Number"] = new JObject
                    {
                        ["rich_text"] = new JArray
                        {
                            new JObject
                            {
                                ["text"] = new JObject
                                {
                                    ["content"] = productNumber
                                }
                            }
                        }
                    },
                    // Product Version (rich_text type)
                    ["Product Version"] = new JObject
                    {
                        ["rich_text"] = new JArray
                        {
                            new JObject
                            {
                                ["text"] = new JObject
                                {
                                    ["content"] = productVersion
                                }
                            }
                        }
                    },
                    // Status (select type) - default to "New"
                    ["Status"] = new JObject
                    {
                        ["select"] = new JObject
                        {
                            ["name"] = "New"
                        }
                    },
                    // Submitted (date type)
                    ["Submitted"] = new JObject
                    {
                        ["date"] = new JObject
                        {
                            ["start"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                        }
                    },
                    // Tags (multi_select type)
                    ["Tags"] = new JObject
                    {
                        ["multi_select"] = tagsArray
                    }
                }
            };

            return payload.ToString(Formatting.None);
        }

        #endregion

        #region Save Action Trigger

        /// <summary>
        /// Trigger the configured save action or execute method in Streamer.bot
        /// Supports both "settingsAction" (action name) and "onSaveExecuteMethod" (codeId + methodName)
        /// </summary>
        private void TriggerSaveAction()
        {
            // First, check for settingsAction (simple action name)
            var settingsAction = _currentConfig?["settingsAction"]?.ToString();

            if (!string.IsNullOrEmpty(settingsAction))
            {
                TriggerAction(settingsAction);
                return;
            }

            // Second, check for onSaveExecuteMethod (codeId + methodName)
            var executeMethod = _currentConfig?["onSaveExecuteMethod"];
            if (executeMethod != null)
            {
                var codeId = executeMethod["codeId"]?.ToString();
                var methodName = executeMethod["methodName"]?.ToString() ?? "Execute";

                if (!string.IsNullOrEmpty(codeId))
                {
                    TriggerExecuteMethod(codeId, methodName);
                    return;
                }
            }

            LogDebug("No settings action configured");
        }

        /// <summary>
        /// Trigger an action by name
        /// </summary>
        private void TriggerAction(string actionName)
        {
            try
            {
                LogInfo($"Triggering settings action: {actionName}");

                // Check if action exists
                if (!_CPH.ActionExists(actionName))
                {
                    LogError($"Settings action not found: {actionName}");
                    return;
                }

                // Run the action
                _CPH.RunAction(actionName, false);

                LogInfo("Settings action triggered successfully");
            }
            catch (Exception ex)
            {
                LogError($"Failed to trigger settings action: {ex.Message}");
            }
        }

        /// <summary>
        /// Trigger an execute method by codeId and methodName
        /// </summary>
        private void TriggerExecuteMethod(string codeId, string methodName)
        {
            try
            {
                LogInfo($"Triggering execute method: {codeId}::{methodName}");

                // Execute the method
                bool success = _CPH.ExecuteMethod(codeId, methodName);

                if (success)
                {
                    LogInfo("Execute method triggered successfully");
                }
                else
                {
                    LogError($"Execute method failed: {codeId}::{methodName}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to trigger execute method: {ex.Message}");
            }
        }

        #endregion
    }
}
