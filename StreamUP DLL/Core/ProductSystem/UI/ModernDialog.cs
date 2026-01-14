using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
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
    /// <summary>
    /// Modern dialog system using WebView2 with consistent styling
    /// Matches StreamUP Settings Viewer design system
    /// </summary>
    public static class ModernDialog
    {
        // Static dialog window - only one dialog allowed at a time
        private static BorderlessForm _dialogWindow;
        private static WebView2 _dialogWebView;
        private static ModernDialogResult _dialogResult;
        private static ManualResetEventSlim _dialogComplete;

        // Theme storage key (same as Settings Viewer)
        private const string THEME_STORAGE_KEY = "streamup_viewer_theme";

        // StreamUP logo as base64 (64x64 resized)
        private const string STREAMUP_LOGO_BASE64 = "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";

        // Win32 API import for rounded corners
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            ref int attrValue,
            int attrSize
        );

        // Window corner preference for Windows 11
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2;

        #region Public Methods - Without Product Name

        public static ModernDialogResult ShowError(string title, string message)
        {
            return ShowDialog(DialogType.Error, title, message, null);
        }

        public static ModernDialogResult ShowWarning(string title, string message)
        {
            return ShowDialog(DialogType.Warning, title, message, null);
        }

        public static ModernDialogResult ShowPrompt(string title, string message, string yesText = "Yes", string noText = "No")
        {
            return ShowDialog(DialogType.Prompt, title, message, null, yesText, noText);
        }

        public static ModernDialogResult ShowInfo(string title, string message)
        {
            return ShowDialog(DialogType.Info, title, message, null);
        }

        public static ModernDialogResult ShowSuccess(string title, string message)
        {
            return ShowDialog(DialogType.Success, title, message, null);
        }

        #endregion

        #region Public Methods - With Product Name

        public static ModernDialogResult ShowError(string title, string message, string productName)
        {
            return ShowDialog(DialogType.Error, title, message, productName);
        }

        public static ModernDialogResult ShowWarning(string title, string message, string productName)
        {
            return ShowDialog(DialogType.Warning, title, message, productName);
        }

        public static ModernDialogResult ShowPrompt(string title, string message, string productName, string yesText, string noText)
        {
            return ShowDialog(DialogType.Prompt, title, message, productName, yesText, noText);
        }

        public static ModernDialogResult ShowInfo(string title, string message, string productName)
        {
            return ShowDialog(DialogType.Info, title, message, productName);
        }

        public static ModernDialogResult ShowSuccess(string title, string message, string productName)
        {
            return ShowDialog(DialogType.Success, title, message, productName);
        }

        #endregion

        private static ModernDialogResult ShowDialog(DialogType type, string title, string message, string productName, string yesText = "OK", string noText = "Cancel")
        {
            // Prevent multiple dialogs
            if (_dialogWindow != null && !_dialogWindow.IsDisposed)
            {
                try
                {
                    _dialogWindow.Invoke((MethodInvoker)delegate
                    {
                        _dialogWindow.Activate();
                        _dialogWindow.BringToFront();
                    });
                }
                catch { }

                return new ModernDialogResult { Declined = true };
            }

            _dialogResult = new ModernDialogResult();
            _dialogComplete = new ManualResetEventSlim(false);

            Thread thread = new Thread(() =>
            {
                try
                {
                    CreateDialogWindow(type, title, message, productName, yesText, noText);
                    Application.Run(_dialogWindow);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[StreamUP Dialog Error] {ex.Message}");
                    _dialogResult.Declined = true;
                    _dialogComplete.Set();
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            _dialogComplete.Wait(TimeSpan.FromMinutes(5));

            return _dialogResult;
        }

        private static void CreateDialogWindow(DialogType type, string title, string message, string productName, string yesText, string noText)
        {
            // Calculate generous size - we'll resize after content loads
            int lineCount = message.Split('\n').Length;
            int charCount = message.Length;
            bool hasProductName = !string.IsNullOrEmpty(productName);
            bool isPrompt = type == DialogType.Prompt;

            // Be more generous with height calculation
            int contentHeight = Math.Max(60, 40 + (lineCount * 22) + (charCount / 40 * 5));
            int estimatedHeight = 56 + (hasProductName ? 32 : 0) + contentHeight + 52;
            estimatedHeight = Math.Max(180, Math.Min(450, estimatedHeight));

            int estimatedWidth = isPrompt ? 420 : 400;

            // Use BorderlessForm for consistency with settings window
            _dialogWindow = new BorderlessForm
            {
                Text = "StreamUP",
                Width = estimatedWidth,
                Height = estimatedHeight,
                MinimumSize = new Size(320, 150),
                MaximumSize = new Size(500, 500),
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(15, 23, 42),
                ShowInTaskbar = true,
                TopMost = true
            };

            _dialogWebView = new WebView2
            {
                Dock = DockStyle.Fill,
                DefaultBackgroundColor = Color.FromArgb(15, 23, 42)
            };
            _dialogWindow.Controls.Add(_dialogWebView);

            // Apply rounded corners for Windows 11 (matches settings window styling)
            ApplyRoundedCorners(_dialogWindow.Handle);

            _dialogWindow.Shown += async (s, e) =>
            {
                try
                {
                    await InitializeDialogWebView(type, title, message, productName, yesText, noText);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[StreamUP Dialog Error] {ex.Message}");
                    _dialogResult.Declined = true;
                    _dialogWindow.Close();
                }
            };

            _dialogWindow.FormClosed += (s, e) =>
            {
                _dialogComplete?.Set();
                _dialogWebView?.Dispose();
                _dialogWebView = null;
                _dialogWindow = null;
            };
        }

        private static async Task InitializeDialogWebView(DialogType type, string title, string message, string productName, string yesText, string noText)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var cachePath = Path.Combine(appData, "StreamUP", "DialogCache");
            Directory.CreateDirectory(cachePath);

            var env = await CoreWebView2Environment.CreateAsync(userDataFolder: cachePath);
            await _dialogWebView.EnsureCoreWebView2Async(env);

            _dialogWebView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            _dialogWebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            _dialogWebView.CoreWebView2.Settings.IsZoomControlEnabled = false;
            _dialogWebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;

            _dialogWebView.CoreWebView2.WebMessageReceived += OnDialogMessageReceived;

            string html = GenerateDialogHtml(type, title, message, productName, yesText, noText);
            _dialogWebView.CoreWebView2.NavigateToString(html);
        }

        private static void OnDialogMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                // Get message as string (viewer sends JSON.stringify)
                var messageString = e.TryGetWebMessageAsString();
                if (string.IsNullOrEmpty(messageString))
                {
                    System.Diagnostics.Debug.WriteLine("[StreamUP Dialog] Received empty message");
                    return;
                }

                var json = JObject.Parse(messageString);
                var action = json["action"]?.ToString();

                switch (action)
                {
                    case "accept":
                    case "yes":
                        _dialogResult.Accepted = true;
                        _dialogWindow?.Invoke((MethodInvoker)delegate { _dialogWindow?.Close(); });
                        break;

                    case "decline":
                    case "no":
                    case "close":
                        _dialogResult.Declined = true;
                        _dialogWindow?.Invoke((MethodInvoker)delegate { _dialogWindow?.Close(); });
                        break;

                    case "resize":
                        int height = json["height"]?.Value<int>() ?? 200;
                        _dialogWindow?.Invoke((MethodInvoker)delegate
                        {
                            if (_dialogWindow != null)
                            {
                                int newHeight = Math.Max(150, Math.Min(500, height + 20));
                                _dialogWindow.Height = newHeight;
                            }
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StreamUP Dialog Message Error] {ex.Message}");
            }
        }

        // Note: StartWindowDrag() removed - DialogForm now handles dragging via WM_NCHITTEST

        /// <summary>
        /// Apply rounded corners for Windows 11 (matches settings window styling)
        /// </summary>
        private static void ApplyRoundedCorners(IntPtr handle)
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StreamUP Dialog] Could not apply rounded corners: {ex.Message}");
            }
        }

        private static string GenerateDialogHtml(DialogType type, string title, string message, string productName, string yesText, string noText)
        {
            string accentColorDark, accentColorLight, iconSvg;

            switch (type)
            {
                case DialogType.Error:
                    accentColorDark = "#ef4444";
                    accentColorLight = "#dc2626";
                    iconSvg = GetErrorIcon();
                    break;
                case DialogType.Warning:
                    accentColorDark = "#f59e0b";
                    accentColorLight = "#d97706";
                    iconSvg = GetWarningIcon();
                    break;
                case DialogType.Success:
                    accentColorDark = "#22c55e";
                    accentColorLight = "#16a34a";
                    iconSvg = GetSuccessIcon();
                    break;
                case DialogType.Prompt:
                case DialogType.Info:
                default:
                    accentColorDark = "#6af4ff";
                    accentColorLight = "#0891b2";
                    iconSvg = GetInfoIcon();
                    break;
            }

            string productSection = "";
            if (!string.IsNullOrEmpty(productName))
            {
                productSection = $@"
                <div class=""product-bar"">
                    <span class=""product-label"">Product:</span>
                    <span class=""product-name"">{EscapeHtml(productName)}</span>
                </div>";
            }

            string buttons;
            if (type == DialogType.Prompt)
            {
                buttons = $@"
                    <button onclick=""decline()"" class=""btn-secondary"">{EscapeHtml(noText)}</button>
                    <button onclick=""accept()"" class=""btn-primary"">{EscapeHtml(yesText)}</button>
                ";
            }
            else
            {
                buttons = $@"<button onclick=""accept()"" class=""btn-primary"">OK</button>";
            }

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <style>
        :root {{
            --accent-dark: {accentColorDark};
            --accent-light: {accentColorLight};
        }}

        * {{ box-sizing: border-box; margin: 0; padding: 0; }}

        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            overflow: hidden;
            line-height: 1.5;
            background-color: #0f172a;
            color: #f1f5f9;
        }}

        .container {{
            display: flex;
            flex-direction: column;
            height: 100vh;
        }}

        /* Header */
        .header {{
            display: flex;
            align-items: center;
            gap: 10px;
            padding: 12px 14px;
            background-color: #0f172a;
            border-bottom: 1px solid #334155;
            user-select: none;
            flex-shrink: 0;
        }}

        .logo {{
            width: 28px;
            height: 28px;
            flex-shrink: 0;
        }}

        .logo img {{
            width: 100%;
            height: 100%;
            object-fit: contain;
        }}

        .icon {{
            width: 20px;
            height: 20px;
            flex-shrink: 0;
            color: var(--accent-dark);
        }}

        .title {{
            flex: 1;
            font-size: 14px;
            font-weight: 600;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }}

        /* Close button with visible X - excluded from drag via DialogForm */
        .close-btn {{
            width: 28px;
            height: 28px;
            display: flex;
            align-items: center;
            justify-content: center;
            border: none;
            background: #334155;
            color: #f1f5f9;
            cursor: pointer;
            border-radius: 6px;
            transition: all 0.15s;
            flex-shrink: 0;
            font-size: 18px;
            font-weight: 400;
            line-height: 1;
            font-family: Arial, sans-serif;
        }}

        .close-btn:hover {{
            background-color: #ef4444;
            color: white;
        }}

        /* Product bar */
        .product-bar {{
            display: flex;
            align-items: center;
            gap: 6px;
            padding: 8px 14px;
            background-color: rgba(30, 41, 59, 0.5);
            border-bottom: 1px solid rgba(51, 65, 85, 0.5);
            font-size: 12px;
            flex-shrink: 0;
        }}

        .product-label {{ color: #64748b; }}
        .product-name {{ color: #cbd5e1; font-weight: 500; }}

        /* Content */
        .content {{
            flex: 1;
            padding: 16px 14px;
            overflow-y: auto;
            font-size: 13px;
            color: #cbd5e1;
            line-height: 1.6;
        }}

        /* Footer */
        .footer {{
            display: flex;
            justify-content: flex-end;
            gap: 8px;
            padding: 12px 14px;
            background-color: #0f172a;
            border-top: 1px solid #334155;
            flex-shrink: 0;
        }}

        /* Buttons */
        button {{
            font-size: 13px;
            font-weight: 500;
            padding: 8px 16px;
            border-radius: 6px;
            border: none;
            cursor: pointer;
            transition: all 0.15s;
        }}

        .btn-primary {{
            background-color: var(--accent-dark);
            color: #0f172a;
        }}

        .btn-primary:hover {{
            filter: brightness(1.1);
        }}

        .btn-secondary {{
            background-color: #334155;
            color: #f1f5f9;
            border: 1px solid #475569;
        }}

        .btn-secondary:hover {{
            background-color: #475569;
        }}

        /* Scrollbar */
        ::-webkit-scrollbar {{ width: 6px; }}
        ::-webkit-scrollbar-track {{ background: transparent; }}
        ::-webkit-scrollbar-thumb {{ background: #475569; border-radius: 3px; }}
        ::-webkit-scrollbar-thumb:hover {{ background: #64748b; }}

        /* Light mode */
        body.light {{
            background-color: #f8fafc;
            color: #0f172a;
        }}

        body.light .header {{
            background-color: #ffffff;
            border-bottom-color: #e2e8f0;
        }}

        body.light .footer {{
            background-color: #ffffff;
            border-top-color: #e2e8f0;
        }}

        body.light .product-bar {{
            background-color: #f1f5f9;
            border-bottom-color: #e2e8f0;
        }}

        body.light .product-name {{ color: #334155; }}
        body.light .content {{ color: #475569; }}
        body.light .title {{ color: #0f172a; }}

        body.light .icon {{ color: var(--accent-light); }}

        body.light .btn-primary {{
            background-color: var(--accent-light);
            color: #ffffff;
        }}

        body.light .btn-secondary {{
            background-color: #e2e8f0;
            color: #0f172a;
            border-color: #cbd5e1;
        }}

        body.light .btn-secondary:hover {{
            background-color: #cbd5e1;
        }}

        body.light .close-btn {{
            background: #e2e8f0;
            color: #334155;
        }}

        body.light .close-btn:hover {{
            background-color: #ef4444;
            color: white;
        }}

        body.light ::-webkit-scrollbar-thumb {{ background: #cbd5e1; }}
    </style>
</head>
<body>
    <div class=""container"" id=""container"">
        <div class=""header"" id=""header"">
            <div class=""logo"">
                <img src=""data:image/png;base64,{STREAMUP_LOGO_BASE64}"" alt=""StreamUP"">
            </div>
            <div class=""icon"">{iconSvg}</div>
            <div class=""title"">{EscapeHtml(title)}</div>
            <button class=""close-btn"" onclick=""decline()"" title=""Close"">&#215;</button>
        </div>

        {productSection}

        <div class=""content"" id=""content"">{EscapeHtml(message)}</div>

        <div class=""footer"">{buttons}</div>
    </div>

    <script>
        // Theme detection
        const theme = localStorage.getItem('{THEME_STORAGE_KEY}') || 'dark';
        if (theme === 'light') document.body.classList.add('light');

        // Send message to C# (must use JSON.stringify to match Settings Viewer pattern)
        function sendMessage(msg) {{
            window.chrome.webview.postMessage(JSON.stringify(msg));
        }}

        // Actions
        function accept() {{
            sendMessage({{ action: 'accept' }});
        }}
        function decline() {{
            sendMessage({{ action: 'decline' }});
        }}

        // Keyboard shortcuts
        document.addEventListener('keydown', function(e) {{
            if (e.key === 'Escape') decline();
            if (e.key === 'Enter') accept();
        }});

        // Auto-resize after content loads
        window.addEventListener('load', function() {{
            setTimeout(function() {{
                const container = document.getElementById('container');
                const height = container.scrollHeight;
                sendMessage({{
                    action: 'resize',
                    height: height
                }});
            }}, 100);
        }});
    </script>
</body>
</html>
";
        }

        private static string EscapeHtml(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;")
                .Replace("\n", "<br>");
        }

        private static string GetErrorIcon()
        {
            return @"<svg fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2""
                      d=""M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z""/>
            </svg>";
        }

        private static string GetWarningIcon()
        {
            return @"<svg fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2""
                      d=""M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z""/>
            </svg>";
        }

        private static string GetSuccessIcon()
        {
            return @"<svg fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2""
                      d=""M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z""/>
            </svg>";
        }

        private static string GetInfoIcon()
        {
            return @"<svg fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2""
                      d=""M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z""/>
            </svg>";
        }
    }

}
