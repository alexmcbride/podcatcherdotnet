using Caliburn.Micro;
using PodcatcherDotNet.Helpers;
using PodcatcherDotNet.Messages;
using PodcatcherDotNet.Properties;
using System;

namespace PodcatcherDotNet.ViewModels {
    public class OptionsViewModel : Screen {
        private IEventAggregator _eventAggregator;
        private int _updateInterval;
        private bool _enableUpdates;
        private string _downloadFolder;
        private bool _replaceId3Tags;
        private bool _autoGoToDownloadScreen;
        private bool _displayNotifyIcon;
        private bool _canSave;
        private bool _useCustomBrowser;
        private string _browserExe;
        private bool _convertM4aToMp3;
        private bool _showAutoConvertOption;
private  bool _externalBrowserDefault;

        public bool CanSave {
            get { return _canSave; }
            set {
                if (_canSave != value) {
                    _canSave = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public int UpdateInterval {
            get { return _updateInterval; }
            set {
                if (_updateInterval != value) {
                    _updateInterval = value;
                    NotifyOfPropertyChange();
                    CanSave = value != (int)Settings.Default.UpdateInterval.TotalMilliseconds;
                }
            }
        }

        public bool EnableUpdates {
            get { return _enableUpdates; }
            set {
                if (_enableUpdates != value) {
                    _enableUpdates = value;
                    NotifyOfPropertyChange();
                    CanSave = value != Settings.Default.UpdatesEnabled;
                }
            }
        }

        public string DownloadFolder {
            get { return _downloadFolder; }
            set {
                if (_downloadFolder != value) {
                    _downloadFolder = value;
                    NotifyOfPropertyChange();
                    CanSave = !String.IsNullOrWhiteSpace(value) && value != Settings.Default.DownloadFolder;
                }
            }
        }

        public bool ReplaceId3Tags {
            get { return _replaceId3Tags; }
            set {
                if (_replaceId3Tags != value) {
                    _replaceId3Tags = value;
                    NotifyOfPropertyChange();
                    CanSave = value != Settings.Default.ReplaceId3Tags;
                }
            }
        }

        public bool AutoGoToDownloadScreen {
            get { return _autoGoToDownloadScreen; }
            set {
                if (_autoGoToDownloadScreen != value) {
                    _autoGoToDownloadScreen = value;
                    NotifyOfPropertyChange();
                    CanSave = value != Settings.Default.AutoGoToDownloadScreen;
                }
            }
        }

        public bool DisplayNotifyIcon {
            get { return _displayNotifyIcon; }
            set {
                if (_displayNotifyIcon != value) {
                    _displayNotifyIcon = value;
                    NotifyOfPropertyChange();
                    CanSave = value != Settings.Default.DisplayNotifyIcon;
                }
            }
        }

        public bool UseCustomBrowser {
            get { return _useCustomBrowser; }
            set {
                if (_useCustomBrowser != value) {
                    _useCustomBrowser = value;
                    NotifyOfPropertyChange();
                    CanSave = value != Settings.Default.UseCustomBrowser;
                }
            }
        }

        public string CustomBrowserExe {
            get { return _browserExe; }
            set {
                if (_browserExe != value) {
                    _browserExe = value;
                    NotifyOfPropertyChange();
                    CanSave = value != Settings.Default.CustomBrowserExe;
                }
            }
        }

        public bool ConvertM4aToMp3 {
            get { return _convertM4aToMp3; }
            set {
                if (_convertM4aToMp3 != value) {
                    _convertM4aToMp3 = value;
                    NotifyOfPropertyChange();
                    CanSave = value != Settings.Default.ConvertM4aToMp3;
                }
            }
        }

        public bool ShowAutoConvertOption {
            get { return _showAutoConvertOption; }
            set {
                if (_showAutoConvertOption != value) {
                    _showAutoConvertOption = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool ExternalBrowserDefault {
            get { return _externalBrowserDefault; }
            set {
                if (_externalBrowserDefault != value) {
                    _externalBrowserDefault = value;
                    NotifyOfPropertyChange();
                    CanSave = value != Settings.Default.ExternalBrowserDefault;
                }
            }
        }

        public OptionsViewModel() {
            _eventAggregator = IoC.Get<IEventAggregator>();

            LoadSettings();
        }

        private void LoadSettings() {
            UpdateInterval = Settings.Default.UpdateInterval.Minutes;
            EnableUpdates = Settings.Default.UpdatesEnabled;
            DownloadFolder = Settings.Default.DownloadFolder;
            ReplaceId3Tags = Settings.Default.ReplaceId3Tags;
            AutoGoToDownloadScreen = Settings.Default.AutoGoToDownloadScreen;
            DisplayNotifyIcon = Settings.Default.DisplayNotifyIcon;
            UseCustomBrowser = Settings.Default.UseCustomBrowser;
            CustomBrowserExe = Settings.Default.CustomBrowserExe;
            ConvertM4aToMp3 = Settings.Default.ConvertM4aToMp3;
            ShowAutoConvertOption = FfmpegHelper.IsFfmpegExeInDirectory();
            ExternalBrowserDefault = Settings.Default.ExternalBrowserDefault;
            CanSave = false;
        }

        public void DownloadFolderBrowse() {
            string selectedFolder;
            if (DialogHelper.ShowBrowseDownloadFolder(out selectedFolder, DownloadFolder)) {
                DownloadFolder = selectedFolder;
            }
        }

        public void CustomBrowserExeBrowse() {
            string browserExe;
            if (DialogHelper.ShowBrowseBrowserExe(out browserExe, CustomBrowserExe)) {
                CustomBrowserExe = browserExe;
            }
        }

        public void Save() {
            var interval = TimeSpan.FromMinutes(UpdateInterval);
            if (Settings.Default.UpdateInterval != interval || Settings.Default.UpdatesEnabled != EnableUpdates) {
                Settings.Default.UpdateInterval = interval;
                Settings.Default.UpdatesEnabled = EnableUpdates;

                // Notify FeedList to change the update timer interval.
                _eventAggregator.PublishOnUIThread(new InitializeTimerMessage());
            }

            Settings.Default.DownloadFolder = DownloadFolder;
            Settings.Default.ReplaceId3Tags = ReplaceId3Tags;
            Settings.Default.DisplayNotifyIcon = DisplayNotifyIcon;
            Settings.Default.AutoGoToDownloadScreen = AutoGoToDownloadScreen;
            Settings.Default.UseCustomBrowser = UseCustomBrowser;
            Settings.Default.CustomBrowserExe = CustomBrowserExe;
            Settings.Default.ConvertM4aToMp3 = ConvertM4aToMp3;
            Settings.Default.ExternalBrowserDefault = ExternalBrowserDefault;
            Settings.Default.Save();

            TryClose(true);
        }

        public void Cancel() {
            TryClose(false);
        }
    }
}
