using Caliburn.Micro;
using Humanizer;
using PodcatcherDotNet.Helpers;
using PodcatcherDotNet.Messages;
using PodcatcherDotNet.Models;
using PodcatcherDotNet.Properties;
using System;
using System.Diagnostics;
using System.Linq;

namespace PodcatcherDotNet.ViewModels {
    public class ShellViewModel : Conductor<Screen>.Collection.OneActive, IHandle<StatusMessage>, IHandle<DownloadPodcastMessage>, IHandle<ActivateScreenMessage>, IHandle<IsReadChangedMessage> {
        private IEventAggregator _eventAggregator;
        private Lazy<FeedListViewModel> _feedList = new Lazy<FeedListViewModel>();
        private Lazy<DownloadSectionViewModel> _downloadSection = new Lazy<DownloadSectionViewModel>();
        private string _statusMessage;
        private bool _isOptionsDropDownOpen;
        private bool _isOptionsEnabled;
        private bool _isDownloadsEnabled;
        private bool _isFeedsEnabled;
        private Screen _previouslyActive;

        public FeedListViewModel FeedList {
            get { return _feedList.Value; }
        }

        public DownloadSectionViewModel DownloadSection {
            get { return _downloadSection.Value; }
        }

        public string StatusMessage {
            get { return _statusMessage; }
            set {
                if (_statusMessage != value) {
                    _statusMessage = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool IsOptionsDropDownOpen {
            get { return _isOptionsDropDownOpen; }
            set {
                if (_isOptionsDropDownOpen != value) {
                    _isOptionsDropDownOpen = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool IsOptionsEnabled {
            get { return _isOptionsEnabled; }
            set {
                if (_isOptionsEnabled != value) {
                    _isOptionsEnabled = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool IsDownloadsEnabled {
            get { return _isDownloadsEnabled; }
            set {
                if (_isDownloadsEnabled != value) {
                    _isDownloadsEnabled = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool IsFeedsEnabled {
            get { return _isFeedsEnabled; }
            set {
                if (_isFeedsEnabled != value) {
                    _isFeedsEnabled = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public int TotalUnread {
            get {
                if (_feedList.IsValueCreated) {
                    return _feedList.Value.TotalUnread;
                }
                return 0;
            }
        }

        public ShellViewModel() {
            DisplayName = "Podcatcher.NET";

            _eventAggregator = IoC.Get<IEventAggregator>();
            _eventAggregator.Subscribe(this);

            SetDefaultFolders();

            ShowFeedList(); // Show main view.
        }

        private static void SetDefaultFolders() {
            if (String.IsNullOrEmpty(Settings.Default.DownloadFolder)) {
                // Try to get Downloads folder, if not then set My Documents.
                Settings.Default.DownloadFolder = WindowsHelper.GetDownloadsFolder();

                if (String.IsNullOrEmpty(Settings.Default.DownloadFolder)) {
                    Settings.Default.DownloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
            }

            if (String.IsNullOrEmpty(Settings.Default.DownloadAsFolder)) {
                Settings.Default.DownloadAsFolder = Settings.Default.DownloadFolder;
            }
        }

        protected override void ChangeActiveItem(Screen newItem, bool closePrevious) {
            base.ChangeActiveItem(newItem, closePrevious);

            IsOptionsEnabled = !(newItem is OptionsViewModel);
            IsDownloadsEnabled = !(newItem is DownloadSectionViewModel);
            IsFeedsEnabled = !(newItem is FeedListViewModel);
        }

        protected override void OnDeactivate(bool close) {
            base.OnDeactivate(close);

            if (close) {
                if (_downloadSection.IsValueCreated) {
                    _downloadSection.Value.ClosingCleanup();
                }

                Settings.Default.Save();
            }
        }

        public override void CanClose(Action<bool> callback) {
            base.CanClose(callback);

            // Ask user to veto close if there are active downloads.
            callback(CheckActiveDownloads());
        }

        private bool CheckActiveDownloads() {
            int count = DownloadSection.ActiveDownloadsCount;

            if (count > 0) {
                return DialogHelper.ShowYesNo(
                    "Active Downloads",
                    "Exiting will cancel {0}, do you want to continue?",
                    "download".ToQuantity(count));
            }

            return true;
        }

        public void Handle(StatusMessage message) {
            StatusMessage = message.Message;
        }

        public void Handle(DownloadPodcastMessage message) {
            if (Settings.Default.AutoGoToDownloadScreen) {
                ShowDownloadSection(); // Show the downloads screen, if false will download in background.
                _downloadSection.Value.SelectDownloadList();
            }
        }

        public void Handle(IsReadChangedMessage message) {
            NotifyOfPropertyChange("TotalUnread");
        }

        public void MarkAllAsRead() {
            IsOptionsDropDownOpen = false;

            _feedList.Value.MarkAllFeedsAsRead();
        }

        public void ImportOpml() {
            IsOptionsDropDownOpen = false;

            string filename;
            if (DialogHelper.ShowOpenOpml(out filename)) {
                try {
                    var feeds = FeedImporter.ImportOpml(filename).ToList();

                    _feedList.Value.AddFeeds(feeds);

                    StatusMessage = String.Format("OPML import completed ({0})", "feed".ToQuantity(feeds.Count));
                }
                catch (Exception ex) {
                    DialogHelper.ShowError("Import OPML Error", ex.ToString());
                }
            }
        }
        public void ExportOpml() {
            IsOptionsDropDownOpen = false;

            string filename;
            if (DialogHelper.ShowSaveOpml(out filename, String.Format("{0}.opml", WindowsHelper.GetAssemblyName()))) {
                try {
                    FeedImporter.ExportOpml(filename, _feedList.Value.Feeds, WindowsHelper.GetAssemblyName());
                    StatusMessage = "OPML export completed";
                }
                catch (Exception ex) {
                    DialogHelper.ShowError("Export OPML Error", ex.ToString());
                }
            }
        }

        public void LoadBackup() {
            string filename;
            if (DialogHelper.ShowLoadBackup(out filename)) {
                try {
                    FeedSerializer.LoadBackup(filename);

                    _feedList.Value.LoadFeeds();
                    _downloadSection.Value.LoadDownloads();

                    StatusMessage = "Backup successfully loaded";
                }
                catch (Exception ex) {
                    DialogHelper.ShowError("Load Backup Error", ex.ToString());
                }
            }
        }

        public void SaveBackup() {
            string filename;
            if (DialogHelper.ShowSaveBackup(out filename)) {
                try {
                    FeedSerializer.SaveBackup(filename);
                    StatusMessage = "Backup successfully saved";
                }
                catch (Exception ex) {
                    DialogHelper.ShowError("Save Backup Error", ex.ToString());
                }
            }
        }

        public void TrayMouseDoubleClick() {
            _eventAggregator.PublishOnUIThread(new ActivateShellViewMessage());
        }

        public void TrayBalloonTipClicked() {
            _eventAggregator.PublishOnUIThread(new ActivateShellViewMessage());
        }

        public void ShowFeedList() {
            ActivateScreen(_feedList.Value);
        }

        public void ShowOptions() {
            ActivateScreen(new OptionsViewModel());
        }

        public void ShowRemoveItems() {
            IsOptionsDropDownOpen = false;

            ActivateScreen(new RemoveItemsViewModel());
        }

        public void ShowDownloadSection() {
            ActivateScreen(_downloadSection.Value);
        }

        public void ShowEditCategories() {
            IsOptionsDropDownOpen = false;

            ActivateScreen(new EditCategoriesViewModel(_feedList.Value.Categories));
        }

        public void Handle(ActivateScreenMessage message) {
            ActivateScreen(message.Screen);
        }

        public void ActivateScreen(Screen screen) {
            _previouslyActive = null;

            // Remember which main UI screen was previously selected for when we close a dialog view.
            if (ActiveItem == _feedList.Value || ActiveItem == _downloadSection.Value) {
                _previouslyActive = ActiveItem;
            }

            // If not one of the main screens then close it.
            if (ActiveItem != null && ActiveItem != _feedList.Value && ActiveItem != _downloadSection.Value) {
                ActiveItem.TryClose();
            }

            ActivateItem(screen);
        }

        public override void DeactivateItem(Screen item, bool close) {
            // Reselect previously active main screen. Stops Caliburn from just chosing the screen itself and getting it wrong.
            if (_previouslyActive != null && _previouslyActive != ActiveItem) {
                ActiveItem = _previouslyActive;
            }

            base.DeactivateItem(item, close);
        }
    }
}