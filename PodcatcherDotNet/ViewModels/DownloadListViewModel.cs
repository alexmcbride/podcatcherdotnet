using Caliburn.Micro;
using Humanizer;
using PodcatcherDotNet.Helpers;
using PodcatcherDotNet.Messages;
using PodcatcherDotNet.Models;
using PodcatcherDotNet.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PodcatcherDotNet.ViewModels {
    public class DownloadListViewModel : Conductor<DownloadViewModel>.Collection.AllActive, IHandle<DownloadFinishedMessage>, IHandle<ClearDownloadMessage>, IHandle<DownloadCancelledMessage> {
        private static readonly TimeSpan SearchDelay = TimeSpan.FromMilliseconds(500);

        private IEventAggregator _eventAggregator;
        private string _searchText;

        public ICollectionView DownloadsView { get; private set; }

        public IEnumerable<DownloadViewModel> ActiveDownloads {
            get { return Items.Where(d => d.IsActive); }
        }

        public bool HasActiveDownloads {
            get { return ActiveDownloads.Any(); }
        }

        public IEnumerable<DownloadViewModel> FinishedDownloads {
            get { return Items.Where(d => d.State == DownloadState.Finished); }
        }

        public bool HasFinishedDownloads {
            get { return FinishedDownloads.Any(); }
        }

        public bool CanCancelAll {
            get { return HasActiveDownloads; }
        }

        public bool CanClearAll {
            get { return HasFinishedDownloads; }
        }

        public string SearchText {
            get { return _searchText; }
            set {
                if (_searchText != value) {
                    _searchText = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public DownloadListViewModel() {
            _eventAggregator = IoC.Get<IEventAggregator>();
            _eventAggregator.Subscribe(this);

            LoadDownloads();
            DownloadsView = CollectionViewSource.GetDefaultView(Items);
            DownloadsView.SortDescriptions.Add(new SortDescription("StartTime", ListSortDirection.Descending));
        }

        public DownloadViewModel GetDownloadFromList(ItemViewModel item) {
            return FinishedDownloads.FirstOrDefault(i => i.FeedTitle == item.FeedTitle && i.Title == item.Title);
        }

        public void DownloadPodcast(ItemViewModel item, string filename) {
            var download = GetDownloadFromList(item);

            if (download == null) {
                DownloadPodcastInternal(item, filename);
            }
            else {
                var result = DialogHelper.ShowYesNo(
                    "Downloaded Before",
                    "You downloaded '{0}' on {1}, do you want to download it again?",
                    item.Title,
                    download.DownloadedDate.ToShortDateString());

                if (result) {
                    DownloadPodcastInternal(item, filename);
                }
            }
        }

        private void DownloadPodcastInternal(ItemViewModel item, string filename) {
            var download = new DownloadViewModel(item, filename);
            download.DownloadProgressChanged += download_DownloadProgressChanged;
            Items.Add(download);

            NotifyOfPropertyChange("ActiveDownloads");
            NotifyOfPropertyChange("CanCancelAll");
        }

        public void Handle(DownloadFinishedMessage message) {
            NotifyOfPropertyChange("ActiveDownloads");
            NotifyOfPropertyChange("CanCancelAll");
            NotifyOfPropertyChange("CanClearAll");

            _eventAggregator.PublishOnUIThread(new UpdateTaskbarProgressMessage(false));
        }

        public void Handle(ClearDownloadMessage message) {
            RemoveDownload(message.Download);

            NotifyOfPropertyChange("ActiveDownloads");
            NotifyOfPropertyChange("CanClearAll");
        }

        public void Handle(DownloadCancelledMessage message) {
            NotifyOfPropertyChange("ActiveDownloads");
            NotifyOfPropertyChange("CanCancelAll");

            _eventAggregator.PublishOnUIThread(new UpdateTaskbarProgressMessage(false));
        }

        // We update the taskbar progress bar.
        private void download_DownloadProgressChanged(object sender, EventArgs e) {
            // Sometimes event gets fired after download status has been set to finishing.
            if (HasActiveDownloads) {
                var progress = ActiveDownloads.Min(d => d.PercentComplete); // Get whichever download has the longest to run.

                _eventAggregator.PublishOnUIThread(new UpdateTaskbarProgressMessage(true, progress));
            }
        }

        private void RemoveDownload(DownloadViewModel download) {
            download.DownloadProgressChanged -= download_DownloadProgressChanged;

            Items.Remove(download);
        }

        public void LoadDownloads() {
            try {
                var downloads = FeedSerializer
                    .LoadDownloads()
                    .Select(p => new DownloadViewModel(p));

                Items.Clear();
                Items.AddRange(downloads);
            }
            catch (Exception ex) {
                DialogHelper.ShowError("Load Downloads Error", ex.ToString());
            }
        }

        public void SaveDownloads() {
            try {
                var downloads = FinishedDownloads.Select(i => i.Download);

                FeedSerializer.SaveDownloads(downloads); // Save to disk.
            }
            catch (Exception ex) {
                DialogHelper.ShowError("Save Downloads Error", ex.ToString());
            }
        }

        public void CancelAll() {
            foreach (var download in ActiveDownloads) {
                download.Cancel();
            }
        }

        public void ClearAll() {
            var downloads = FinishedDownloads.ToList();

            var result = DialogHelper.ShowYesNo(
                "Clear Downloads",
                "Are you sure you want to clear {0}?",
                "download".ToQuantity(downloads.Count));

            if (result) {
                foreach (var download in downloads) {
                    RemoveDownload(download);
                }

                NotifyOfPropertyChange("CanClearAll");
            }
        }

        public void OpenDownloadFolder() {
            try {
                WindowsHelper.OpenFolder(Settings.Default.DownloadFolder);
            }
            catch (Exception ex) {
                DialogHelper.ShowError("Open Folder Error", ex.ToString());
            }
        }

        public async void SearchTextChanged() {
            var temp = SearchText;

            await Task.Delay(SearchDelay);

            if (temp == SearchText) {
                SearchDownloadList();
            }
        }

        private void SearchDownloadList() {
            DownloadsView.Filter = obj => {
                if (String.IsNullOrEmpty(SearchText)) {
                    return true;
                }

                var download = (DownloadViewModel)obj;

                if (SearchText[0] == '-') {
                    return !DoesDownloadContainSearch(download, SearchText.Substring(1));
                }

                return DoesDownloadContainSearch(download, SearchText);
            };
        }

        private static bool DoesDownloadContainSearch(DownloadViewModel download, string searchText) {
            return download.Title.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) > -1 || download.FeedTitle.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) > -1;
        }
    }
}
