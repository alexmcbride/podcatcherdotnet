using Caliburn.Micro;
using Humanizer;
using PodcatcherDotNet.Helpers;
using PodcatcherDotNet.Messages;
using PodcatcherDotNet.Models;
using PodcatcherDotNet.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PodcatcherDotNet.ViewModels {
    [DebuggerDisplay("Title = {Title}")]
    public class ItemViewModel : Screen {
        private static readonly DateTime Today = DateTime.Today;
        private static readonly DateTime Yesterday = DateTime.Today.Subtract(TimeSpan.FromDays(1));
        private static readonly DateTime LastWeek = DateTime.Today.Subtract(TimeSpan.FromDays(7));

        private IEventAggregator _eventAggregator;

        public Item Item { get; private set; }

        // The parent feed.
        public FeedViewModel Feed { get; private set; }

        public string Title {
            get { return Item.Title; }
        }

        public string FeedTitle {
            get { return Feed.Title; }
        }

        public DateTime Published {
            get { return Item.Published; }
        }

        public string PublishedPretty {
            get { return Item.Published.Humanize(utcDate: false); }
        }

        public bool WasPublishedToday {
            get { return Published.Date == Today; }
        }

        public bool WasPublishedYesterday {
            get { return Published.Date == Yesterday; }
        }

        public bool WasPublishedThisWeek {
            get { return Published.Date > LastWeek; }
        }

        public string Summary {
            get { return Item.Summary; }
        }

        public bool HasSummary {
            get { return !String.IsNullOrEmpty(Item.Summary); }
        }

        public string Url {
            get { return Item.Url; }
        }

        public bool HasUrl {
            get { return !String.IsNullOrEmpty(Url); }
        }

        public string PodcastUrl {
            get { return Item.PodcastUrl; }
        }

        public bool IsPodcast {
            get { return !String.IsNullOrEmpty(PodcastUrl); }
        }

        public string PodcastFileName {
            get { return new Uri(PodcastUrl).Segments.LastOrDefault(); }
        }

        public bool IsRead {
            get { return Item.IsRead; }
            set {
                if (Item.IsRead != value) {
                    Item.IsRead = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public ItemViewModel(Item item, FeedViewModel feed, IEventAggregator eventAggregator) {
            Feed = feed;
            Item = item;

            _eventAggregator = eventAggregator;
        }

        public void ToggleMarkAsRead() {
            Feed.MarkAsRead(this, !IsRead);
        }

        public void MarkAsRead() {
            Feed.MarkAsRead(this, true);
        }

        public void MarkAsUnread() {
            Feed.MarkAsRead(this, false);
        }

        public void ViewInBrowser() {
            if (Settings.Default.ExternalBrowserDefault) {
                ViewInExternalBrowser();
            }
            else {
                ViewInInternalBrowser();
            }
        }

        public void ViewInInternalBrowser() {
            new BrowserViewModel().Activate(Url);
        }

        public void ViewInExternalBrowser() {
            try {
                WindowsHelper.OpenBrowser(Url, Settings.Default.UseCustomBrowser, Settings.Default.CustomBrowserExe);
            }
            catch (Exception ex) {
                DialogHelper.ShowError("Browser Error", ex.ToString());
            }
        }

        public void DownloadPodcast() {
            if (Directory.Exists(Settings.Default.DownloadFolder)) {
                DownloadPodcast(Path.Combine(Settings.Default.DownloadFolder, PodcastFileName));
            }
            else {
                DownloadPodcastAs();
            }
        }

        public void DownloadPodcastAs() {
            string filename;
            if (DialogHelper.ShowSavePodcast(out filename, Settings.Default.DownloadAsFolder, PodcastFileName)) {
                DownloadPodcast(filename);

                Settings.Default.DownloadAsFolder = Path.GetDirectoryName(filename);
            }
        }

        private void DownloadPodcast(string filename) {
            _eventAggregator.PublishOnUIThread(new DownloadPodcastMessage(this, filename));
        }

        public void Remove() {
            var vm = new RemoveItemViewModel(this);
            vm.Action = item => ((FeedViewModel)Parent).RemoveItem(this);
            vm.Activate();
        }
    }
}
