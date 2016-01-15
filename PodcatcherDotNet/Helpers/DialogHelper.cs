using Caliburn.Micro;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.IO;
using System.Windows;

namespace PodcatcherDotNet.Helpers {
    public static class DialogHelper {
        private static Window Owner {
            get { return App.Current.MainWindow; }
        }

        public static bool ShowOpenOpml(out string filename) {
            var dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.Filter = "OPML File (*.opml)|*.opml|All Files (*)|*";
            dlg.Multiselect = false;
            dlg.Title = "Import OPML File";

            if (dlg.ShowDialog(Owner) ?? false) {
                filename = dlg.FileName;
                return true;
            }

            filename = null;
            return false;
        }

        public static bool ShowSaveOpml(out string filename, string initialFileName) {
            var dlg = new SaveFileDialog();
            dlg.AddExtension = true;
            dlg.Filter = "OPML File (*.opml)|*.opml";
            dlg.OverwritePrompt = true;
            dlg.Title = "Save OPML File";
            dlg.FileName = initialFileName;

            if (dlg.ShowDialog(Owner) ?? false) {
                filename = dlg.FileName;
                return true;
            }

            filename = null;
            return false;
        }

        public static bool ShowBrowseDownloadFolder(out string selectedPath, string initialPath) {
            var dlg = new VistaFolderBrowserDialog();
            dlg.Description = "Locate podcast download folder";
            dlg.ShowNewFolderButton = true;
            dlg.UseDescriptionForTitle = true;
            dlg.SelectedPath = initialPath;

            if (dlg.ShowDialog(Owner) ?? false) {
                selectedPath = dlg.SelectedPath;
                return true;
            }

            selectedPath = null;
            return false;
        }

        public static bool ShowBrowseBrowserExe(out string browserExe, string initialFileName) {
            var dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.FileName = Path.GetFileName(initialFileName);
            dlg.Filter = "Browser Executable (*.exe)|*.exe";
            dlg.Title = "Locate Browser Exectuable File";

            if (dlg.ShowDialog(Owner) ?? false) {
                browserExe = dlg.FileName;
                return true;
            }

            browserExe = null;
            return false;
        }

        public static bool ShowSavePodcast(out string filename, string folder, string name) {
            var dlg = new SaveFileDialog();
            dlg.AddExtension = true;
            dlg.CheckPathExists = true;
            dlg.Filter = "MP3 File (*.mp3)|*.mp3";
            dlg.OverwritePrompt = true;
            dlg.Title = "Save Podcast";
            dlg.InitialDirectory = Directory.Exists(folder) ? folder : null;
            dlg.FileName = name;

            if (dlg.ShowDialog(Owner) ?? false) {
                filename = dlg.FileName;
                return true;
            }

            filename = null;
            return false;
        }

        public static bool ShowAddFiles(out string[] filenames) {
            var dlg = new OpenFileDialog();
            dlg.Title = "Add Files";
            dlg.Filter = "MP3 Files (*.mp3)|*.mp3";
            dlg.CheckFileExists = true;
            dlg.Multiselect = true;

            if (dlg.ShowDialog(Owner) ?? false) {
                filenames = dlg.FileNames;
                return true;
            }

            filenames = new string[0];
            return false;
        }

        public static bool ShowSaveBackup(out string filename) {
            var dlg = new SaveFileDialog();
            dlg.Title = "Save Backup";
            dlg.CheckPathExists = true;
            dlg.Filter = "Podcatcher.NET Backup File (*.pbf)|*.pbf";
            dlg.OverwritePrompt = true;
            dlg.FileName = "PodcatcherDotNet Backup File.pbf";

            if (dlg.ShowDialog(Owner) ?? false) {
                filename = dlg.FileName;
                return true;
            }

            filename = null;
            return false;
        }

        public static bool ShowLoadBackup(out string filename) {
            var dlg = new OpenFileDialog();
            dlg.Title = "Load Backup";
            dlg.AddExtension = true;
            dlg.CheckFileExists = true;
            dlg.Multiselect = false;
            dlg.Filter = "Podcatcher.NET Backup File (*.pbf)|*.pbf|All Files (*)|*";
            

            if (dlg.ShowDialog(Owner) ?? false) {
                filename = dlg.FileName;
                return true;
            }

            filename = null;
            return false;
        }

        public static void ShowInfo(string title, string message, params object[] args) {
            Execute.OnUIThread(() => MessageBox.Show(Owner, String.Format(message, args), title, MessageBoxButton.OK, MessageBoxImage.Information));
        }

        public static void ShowError(string title, string message, params object[] args) {
            Execute.OnUIThread(() => MessageBox.Show(Owner, String.Format(message, args), title, MessageBoxButton.OK, MessageBoxImage.Error));
        }

        public static bool ShowYesNo(string title, string message, params object[] args) {
            return MessageBox.Show(Owner, String.Format(message, args), title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}
