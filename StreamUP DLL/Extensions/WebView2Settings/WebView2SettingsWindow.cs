using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // Static window instance - only one settings window allowed at a time
        private static Form _settingsWindow;
        private static WebView2 _webView;
        private static bool _webViewInitialized = false;

        // Current session state
        private static bool _hasUnsavedChanges = false;
        private static JObject _currentConfig;
        private static JObject _currentSavedSettings;
        private static JObject _currentLiveData;
        private static string _currentProductNumber;
        private static StreamUpLib _currentInstance;

        // Window position/size persistence (static = persists while Streamer.bot runs)
        private static Point? _lastWindowLocation;
        private static Size? _lastWindowSize;
        private static FormWindowState _lastWindowState = FormWindowState.Normal;

        // Viewer URL - always use production
        private const string VIEWER_URL = "https://viewer.streamup.tips/";
        //private const string VIEWER_URL = "http://localhost:8080/";

        /// <summary>
        /// Creates and shows the WebView2 settings window
        /// </summary>
        private Task CreateAndShowSettingsWindow()
        {
            LogInfo("Creating WebView2 settings window");

            // Check if window already exists and is open
            if (_settingsWindow != null && !_settingsWindow.IsDisposed)
            {
                LogInfo("Settings window already open, bringing to front");
                _settingsWindow.Invoke(
                    (MethodInvoker)
                        delegate
                        {
                            _settingsWindow.WindowState = FormWindowState.Normal;
                            _settingsWindow.Activate();
                            _settingsWindow.BringToFront();
                        }
                );
                return Task.CompletedTask;
            }

            // Create window on STA thread (required for WebView2)
            var windowReady = new TaskCompletionSource<bool>();

            Thread thread = new Thread(() =>
            {
                try
                {
                    CreateSettingsWindowInternal(windowReady);
                    Application.Run(_settingsWindow);
                }
                catch (Exception ex)
                {
                    LogError($"Failed to create settings window: {ex.Message}");
                    windowReady.TrySetException(ex);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            return windowReady.Task;
        }

        /// <summary>
        /// Internal method to create the settings window (runs on STA thread)
        /// </summary>
        private void CreateSettingsWindowInternal(TaskCompletionSource<bool> windowReady)
        {
            // Calculate smart window size based on screen dimensions
            var screenSize = Screen.PrimaryScreen.WorkingArea;
            int targetWidth,
                targetHeight;

            if (_lastWindowSize.HasValue)
            {
                // Use last saved size
                targetWidth = _lastWindowSize.Value.Width;
                targetHeight = _lastWindowSize.Value.Height;
            }
            else
            {
                // Calculate size - narrower width to fit settings content, good height
                targetWidth = Math.Min(Math.Max((int)(screenSize.Width * 0.38), 700), 900);
                targetHeight = Math.Min(Math.Max((int)(screenSize.Height * 0.8), 600), 1000);
            }

            // Create the form - using Form with borderless style
            _settingsWindow = new Form
            {
                Text = "StreamUP Settings",
                Width = targetWidth,
                Height = targetHeight,
                MinimumSize = new Size(550, 400),
                StartPosition = _lastWindowLocation.HasValue
                    ? FormStartPosition.Manual
                    : FormStartPosition.CenterScreen,
                BackColor = Color.FromArgb(15, 23, 42), // Slate 900 to match loading screen
                FormBorderStyle = FormBorderStyle.None // Borderless for custom title bar
            };

            // Restore last position if available (ensure it's on screen)
            if (_lastWindowLocation.HasValue)
            {
                var loc = _lastWindowLocation.Value;
                // Make sure window is visible on screen
                if (
                    loc.X >= 0
                    && loc.Y >= 0
                    && loc.X < screenSize.Width - 100
                    && loc.Y < screenSize.Height - 100
                )
                {
                    _settingsWindow.Location = loc;
                }
            }

            // Restore last window state
            _settingsWindow.WindowState = _lastWindowState;

            // Set window icon for taskbar
            SetWindowIcon();

            // Apply rounded corners for Windows 11
            ApplyRoundedCorners(_settingsWindow.Handle);

            // Handle window events
            _settingsWindow.FormClosing += OnSettingsWindowClosing;
            _settingsWindow.Move += OnSettingsWindowMove;
            _settingsWindow.Resize += OnSettingsWindowResize;

            // Create WebView2 control
            _webView = new WebView2 { Dock = DockStyle.Fill };
            _settingsWindow.Controls.Add(_webView);

            // Initialize WebView2 and navigate when form is shown
            _settingsWindow.Shown += async (s, e) =>
            {
                try
                {
                    await InitializeWebView2();
                    await NavigateAndInjectData();
                    windowReady.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    LogError($"Failed to initialize settings: {ex.Message}");
                    windowReady.TrySetException(ex);
                    _settingsWindow.Close();
                }
            };
        }

        /// <summary>
        /// Initialize WebView2 control
        /// </summary>
        private async Task InitializeWebView2()
        {
            LogInfo("Initializing WebView2");

            try
            {
                // Set default background color to prevent white flash (StreamUP midnight: #1a1f35)
                _webView.DefaultBackgroundColor = Color.FromArgb(26, 31, 53);

                // Set up WebView2 environment with cache folder
                var dataPath = Path.Combine(GetStreamerBotFolder(), "StreamUP", "Data");
                Directory.CreateDirectory(dataPath);

                var cachePath = Path.Combine(dataPath, "WebView2Cache");

                var env = await CoreWebView2Environment.CreateAsync(userDataFolder: cachePath);

                await _webView.EnsureCoreWebView2Async(env);

                // Configure WebView2 settings
                _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                _webView.CoreWebView2.Settings.IsZoomControlEnabled = false;
                _webView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
                _webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;

                // Set up message handler for communication from viewer
                _webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

                _webViewInitialized = true;
                LogInfo("WebView2 initialized successfully");
            }
            catch (Exception ex)
            {
                LogError($"Failed to initialize WebView2: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Navigate to viewer and inject data
        /// </summary>
        private async Task NavigateAndInjectData()
        {
            LogInfo($"Navigating to viewer: {VIEWER_URL}");

            var navigationCompleted = new TaskCompletionSource<bool>();

            void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
            {
                _webView.CoreWebView2.NavigationCompleted -= OnNavigationCompleted;

                if (e.IsSuccess)
                {
                    navigationCompleted.TrySetResult(true);
                }
                else
                {
                    navigationCompleted.TrySetException(
                        new Exception($"Navigation failed: {e.WebErrorStatus}")
                    );
                }
            }

            _webView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
            _webView.CoreWebView2.Navigate(VIEWER_URL);

            // Wait for navigation to complete
            var success = await navigationCompleted.Task;

            if (!success)
            {
                LogError("Navigation to viewer failed");
                throw new Exception(
                    "Failed to load settings viewer. Please check your internet connection."
                );
            }

            // Build product data (includes OBS instance number from saved settings)
            var productData = new JObject
            {
                ["obsInstanceNumber"] =
                    _currentSavedSettings?["productData"]?["obsInstanceNumber"] ?? 0
            };

            // Inject all data into the viewer
            var configJson = _currentConfig?.ToString(Formatting.None) ?? "{}";
            var savedSettingsJson = _currentSavedSettings?.ToString(Formatting.None) ?? "null";
            var productDataJson = productData.ToString(Formatting.None);
            var liveDataJson = _currentLiveData?.ToString(Formatting.None) ?? "{}";

            var initScript =
                $@"
                window.STREAMUP_MODE = 'production';
                window.STREAMUP_CONFIG = {configJson};
                window.STREAMUP_SAVED_SETTINGS = {savedSettingsJson};
                window.STREAMUP_PRODUCT_DATA = {productDataJson};
                window.STREAMUP_LIVE_DATA = {liveDataJson};
                window.dispatchEvent(new Event('streamup-init'));
                console.log('[StreamUP] Data injected from DLL');
            ";

            await _webView.CoreWebView2.ExecuteScriptAsync(initScript);
            _hasUnsavedChanges = false;

            LogInfo("Data injected into viewer successfully");
        }

        #region Window Event Handlers

        private void OnSettingsWindowShown(object sender, EventArgs e)
        {
            LogDebug("Settings window shown");
        }

        private void OnSettingsWindowMove(object sender, EventArgs e)
        {
            if (_settingsWindow.WindowState == FormWindowState.Normal)
            {
                _lastWindowLocation = _settingsWindow.Location;
            }
        }

        private void OnSettingsWindowResize(object sender, EventArgs e)
        {
            if (_settingsWindow.WindowState == FormWindowState.Normal)
            {
                _lastWindowSize = _settingsWindow.Size;
            }
            _lastWindowState = _settingsWindow.WindowState;
        }

        private void OnSettingsWindowClosing(object sender, FormClosingEventArgs e)
        {
            LogDebug($"Settings window closing, unsaved changes: {_hasUnsavedChanges}");

            if (_hasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Are you sure you want to close?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2
                );

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Clean up
            _webViewInitialized = false;
            _hasUnsavedChanges = false;
            _currentConfig = null;
            _currentSavedSettings = null;
            _currentLiveData = null;
            _currentProductNumber = null;
            _currentInstance = null;

            LogInfo("Settings window closed");
        }

        #endregion

        #region Window Styling and Controls

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            ref int attrValue,
            int attrSize
        );

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam
        );

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        /// <summary>
        /// Start dragging the window (called from viewer via message)
        /// </summary>
        private void StartWindowDrag()
        {
            if (_settingsWindow != null && !_settingsWindow.IsDisposed)
            {
                _settingsWindow.Invoke(
                    (MethodInvoker)
                        delegate
                        {
                            ReleaseCapture();
                            SendMessage(
                                _settingsWindow.Handle,
                                WM_NCLBUTTONDOWN,
                                (IntPtr)HT_CAPTION,
                                IntPtr.Zero
                            );
                        }
                );
            }
        }

        /// <summary>
        /// Minimize the window
        /// </summary>
        private void MinimizeWindow()
        {
            if (_settingsWindow != null && !_settingsWindow.IsDisposed)
            {
                _settingsWindow.Invoke(
                    (MethodInvoker)
                        delegate
                        {
                            _settingsWindow.WindowState = FormWindowState.Minimized;
                        }
                );
            }
        }

        /// <summary>
        /// Maximize or restore the window
        /// </summary>
        private void MaximizeWindow()
        {
            if (_settingsWindow != null && !_settingsWindow.IsDisposed)
            {
                _settingsWindow.Invoke(
                    (MethodInvoker)
                        delegate
                        {
                            if (_settingsWindow.WindowState == FormWindowState.Maximized)
                            {
                                _settingsWindow.WindowState = FormWindowState.Normal;
                            }
                            else
                            {
                                _settingsWindow.WindowState = FormWindowState.Maximized;
                            }
                        }
                );
            }
        }

        /// <summary>
        /// Get current window state for viewer
        /// </summary>
        private string GetWindowState()
        {
            if (_settingsWindow == null || _settingsWindow.IsDisposed)
                return "normal";

            return _settingsWindow.WindowState == FormWindowState.Maximized
                ? "maximized"
                : "normal";
        }

        // Window corner preference for Windows 11
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2;

        /// <summary>
        /// Apply rounded corners for Windows 11
        /// </summary>
        private void ApplyRoundedCorners(IntPtr handle)
        {
            try
            {
                int preference = DWMWCP_ROUND;
                DwmSetWindowAttribute(
                    handle,
                    DWMWA_WINDOW_CORNER_PREFERENCE,
                    ref preference,
                    sizeof(int)
                );
                LogDebug("Rounded corners applied");
            }
            catch (Exception ex)
            {
                LogDebug($"Could not apply rounded corners: {ex.Message}");
            }
        }

        /// <summary>
        /// Set window icon from embedded resource or file
        /// </summary>
        private void SetWindowIcon()
        {
            try
            {
                // First, try to load from embedded resources (most reliable)
                var assembly = typeof(StreamUpLib).Assembly;
                using (
                    var stream = assembly.GetManifestResourceStream(
                        "StreamUP.Resources.StreamUp-icon.png"
                    )
                )
                {
                    if (stream != null)
                    {
                        using (var bitmap = new Bitmap(stream))
                        {
                            _settingsWindow.Icon = Icon.FromHandle(bitmap.GetHicon());
                        }
                        LogDebug("Window icon set from embedded resource");
                        return;
                    }
                }

                // Fallback: Try file paths
                var possiblePaths = new[]
                {
                    Path.Combine(
                        Path.GetDirectoryName(assembly.Location),
                        "Resources",
                        "StreamUp-icon.png"
                    ),
                    Path.Combine(Path.GetDirectoryName(assembly.Location), "StreamUp-icon.png"),
                    Path.Combine(
                        GetStreamerBotFolder(),
                        "StreamUP",
                        "Resources",
                        "StreamUp-icon.png"
                    ),
                };

                foreach (var iconPath in possiblePaths)
                {
                    if (File.Exists(iconPath))
                    {
                        using (var bitmap = new Bitmap(iconPath))
                        {
                            _settingsWindow.Icon = Icon.FromHandle(bitmap.GetHicon());
                        }
                        LogDebug($"Window icon set from: {iconPath}");
                        return;
                    }
                }

                LogDebug("No custom icon found, using default");
            }
            catch (Exception ex)
            {
                LogDebug($"Could not set window icon: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods for Viewer Communication

        /// <summary>
        /// Send a message to the viewer
        /// </summary>
        private void SendToViewer(object message)
        {
            if (_webView?.CoreWebView2 == null)
            {
                LogError("Cannot send message - WebView2 not initialized");
                return;
            }

            try
            {
                var json = JsonConvert.SerializeObject(message);
                _webView.CoreWebView2.PostWebMessageAsJson(json);
                LogDebug($"Sent message to viewer: {message.GetType().Name}");
            }
            catch (Exception ex)
            {
                LogError($"Failed to send message to viewer: {ex.Message}");
            }
        }

        /// <summary>
        /// Execute JavaScript in the viewer
        /// </summary>
        private async Task ExecuteScriptAsync(string script)
        {
            if (_webView?.CoreWebView2 == null)
            {
                LogError("Cannot execute script - WebView2 not initialized");
                return;
            }

            try
            {
                await _webView.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                LogError($"Failed to execute script: {ex.Message}");
            }
        }

        #endregion
    }
}
