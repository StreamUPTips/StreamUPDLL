using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace StreamUP
{
    public partial class StreamUpLib
    {

        public static string GetFriendlyName()
        {
            try
            {
                if (IsWine())
                    return $"Wine ({GetWindowsVersion()})";

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return GetWindowsVersion();

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return GetLinuxDistro();

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return GetMacOSVersion();

                return "Unknown OS";
            }
            catch
            {
                return "Unknown OS";
            }
        }

        // ---------------- WINDOWS ----------------



        private static string GetWindowsVersion()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

                string productName = key?.GetValue("ProductName")?.ToString();
                string displayVersion = key?.GetValue("DisplayVersion")?.ToString();

                if (!string.IsNullOrEmpty(productName))
                {
                    return displayVersion != null
                        ? $"{productName} ({displayVersion})"
                        : productName;
                }
            }
            catch { }

            return "Windows";
        }

        private static string GetWindowsVersionNumber()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine
                    .OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

                var major = key?.GetValue("CurrentMajorVersionNumber");
                var minor = key?.GetValue("CurrentMinorVersionNumber");
                var build = key?.GetValue("CurrentBuild")?.ToString();

                if (major != null && minor != null && build != null)
                {
                    return $"{major}.{minor}.{build}";
                }

                // fallback (older systems)
                var version = key?.GetValue("CurrentVersion")?.ToString();
                if (version != null && build != null)
                {
                    return $"{version}.{build}";
                }
            }
            catch { }

            return "Unknown";
        }

        // ---------------- LINUX ----------------

        private static string GetLinuxDistro()
        {
            const string osRelease = "/etc/os-release";

            if (File.Exists(osRelease))
            {
                var lines = File.ReadAllLines(osRelease);

                var prettyName = lines
                    .FirstOrDefault(l => l.StartsWith("PRETTY_NAME="));

                if (prettyName != null)
                {
                    return prettyName
                        .Split('=')[1]
                        .Trim('"');
                }
            }

            return "Linux";
        }

        // ---------------- macOS ----------------

        private static string GetMacOSVersion()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "sw_vers",
                        Arguments = "-productVersion",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string version = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();

                return $"macOS {version}";
            }
            catch
            {
                return "macOS";
            }
        }

        // ---------------- WINE ----------------

        private static bool IsWine()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                   Environment.GetEnvironmentVariable("WINELOADERNOEXEC") != null;
        }
    }
}