using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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

        #endregion

        #region Save Action Trigger

        /// <summary>
        /// Trigger the configured save action in Streamer.bot
        /// </summary>
        private void TriggerSaveAction()
        {
            var settingsAction = _currentConfig?["settingsAction"]?.ToString();

            if (string.IsNullOrEmpty(settingsAction))
            {
                LogDebug("No settings action configured");
                return;
            }

            try
            {
                LogInfo($"Triggering settings action: {settingsAction}");

                // Check if action exists
                if (!_CPH.ActionExists(settingsAction))
                {
                    LogError($"Settings action not found: {settingsAction}");
                    return;
                }

                // Run the action
                _CPH.RunAction(settingsAction, false);

                LogInfo("Settings action triggered successfully");
            }
            catch (Exception ex)
            {
                LogError($"Failed to trigger settings action: {ex.Message}");
            }
        }

        #endregion
    }
}
