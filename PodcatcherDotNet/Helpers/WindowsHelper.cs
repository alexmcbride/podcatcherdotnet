using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PodcatcherDotNet.Helpers {
    public static class WindowsHelper {
        private const string ShellFoldersRegistryKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders";
        private static readonly string DownloadFolderGuid = "{374DE290-123F-4565-9164-39C4925E467B}";

        public static string GetAssemblyName() {
            return new AssemblyName(typeof(WindowsHelper).Assembly.FullName).Name;
        }

        public static string GetDownloadsFolder() {
            // Get default Downloads folder under Windows 7 and 8.
            return Registry.GetValue(ShellFoldersRegistryKey, DownloadFolderGuid, null) as String;
        }

        public static bool IsValidUrl(string url) {
            return Uri.IsWellFormedUriString(url, UriKind.Absolute) || File.Exists(url);
        }

        public static void OpenFolder(string filename) {
            OpenFolder(filename, false);
        }

        public static void OpenFolder(string filename, bool select) {
            // Opens folder in Windows Explorer and optionally selects a specific file.
            Process.Start("explorer.exe", select ? "/select, " + filename : filename);
        }

        public static void OpenBrowser(string url, bool useCustomBrowser, string browserExe) {
            if (useCustomBrowser && File.Exists(browserExe)) {
                Process.Start(browserExe, url);
            }
            else {
                Process.Start(url);
            }
        }

        public static void OpenMusicPlayer(string mp3FileName) {
            Process.Start(mp3FileName);
        }
    }
}
