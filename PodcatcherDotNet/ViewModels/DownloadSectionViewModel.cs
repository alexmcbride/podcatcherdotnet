using Caliburn.Micro;
using PodcatcherDotNet.Messages;
using System;
using System.Linq;

namespace PodcatcherDotNet.ViewModels {
    public class DownloadSectionViewModel : Screen, IHandle<DownloadPodcastMessage>, IHandle<DownloadFinishedMessage>, IHandle<DownloadCancelledMessage> {
        private IEventAggregator _eventAggregator;

        private Lazy<DownloadListViewModel> _downloadList;
        private Lazy<DownloadFolderViewModel> _downloadFolder;
private  int _selectedTabIndex;

        public DownloadListViewModel DownloadList {
            get { return _downloadList.Value; }
        }

        public DownloadFolderViewModel DownloadFolder {
            get { return _downloadFolder.Value; }
        }

        public int ActiveDownloadsCount {
            get {
                if (_downloadList.IsValueCreated) {
                    return _downloadList.Value.ActiveDownloads.Count();
                }
                return 0;
            }
        }

        public int SelectedTabIndex {
            get { return _selectedTabIndex; }
            set {
                if (_selectedTabIndex != value) {
                    _selectedTabIndex = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public DownloadSectionViewModel() {
            _eventAggregator = IoC.Get<IEventAggregator>();
            _eventAggregator.Subscribe(this);

            _downloadList = new Lazy<DownloadListViewModel>();
            _downloadFolder = new Lazy<DownloadFolderViewModel>(() => new DownloadFolderViewModel(this));
        }

        // We handle adding downloads here because if the DownloadList has not been
        // initialized yet it won't be able to receive the event.
        public void Handle(DownloadPodcastMessage message) {
            DownloadList.DownloadPodcast(
                   message.Item,
                   message.FileName);

            NotifyOfPropertyChange("ActiveDownloadsCount");
        }

        public void Handle(DownloadFinishedMessage message) {
            NotifyOfPropertyChange("ActiveDownloadsCount");
        }

        public void Handle(DownloadCancelledMessage message) {
            NotifyOfPropertyChange("ActiveDownloadsCount");
        }

        public bool IsActiveDownload(string filename) {
            return DownloadList.ActiveDownloads.Any(d => d.FileName == filename);
        }

        public void TabSelectionChanged(int index) {
            const int DownloadFolderTabIndex = 1;

            // Only load the download folder files when its tab is selected.
            if (index == DownloadFolderTabIndex) {
                DownloadFolder.LoadDownloadFolder();
            }
        }

        public void LoadDownloads() {
            if (_downloadList.IsValueCreated) {
                _downloadList.Value.LoadDownloads();
            }
        }

        public void CancelAll() {
            DownloadList.CancelAll();
        }

        public void SaveDownloadHistory() {
            DownloadList.SaveDownloads();
        }

        public void ClosingCleanup() {
            CancelAll();
            SaveDownloadHistory();
        }

        public void SelectDownloadList() {
            SelectedTabIndex = 0;
        }
    }
}
