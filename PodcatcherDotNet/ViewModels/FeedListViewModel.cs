using Caliburn.Micro;
using Humanizer;
using PodcatcherDotNet.Helpers;
using PodcatcherDotNet.Messages;
using PodcatcherDotNet.Models;
using PodcatcherDotNet.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PodcatcherDotNet.ViewModels {
    public class FeedListViewModel :
        Conductor<FeedViewModel>.Collection.OneActive,
        IHandle<InitializeTimerMessage>,
        IHandle<CommitFeedEditMessage>,
        IHandle<BeginFeedEditMessage>,
        IHandle<RemoveOldItemsMessage>,
        IHandle<EditCategoryMessage>,
        IDisposable {

        private static readonly TimeSpan FilterDelay = TimeSpan.FromMilliseconds(500);

        private IEventAggregator _eventAggregator;
        private ICollectionView _itemsView;
        private IEditableCollectionView _editableFeedsView;
        private TimerHelper _timerHelper;
        private string _filterTerm;
        private bool _isFilterAll;
        private bool _isFilterUnread;
        private bool _isFilterPodcasts;
        private bool _isFilterToday;
        private bool _isFilterYesterday;
        private bool _isFilterThisWeek;
        private bool _canUpdateFeeds;

        public ICollectionView FeedsView {
            get { return _itemsView; }
            set {
                if (_itemsView != value) {
                    _itemsView = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public IEnumerable<string> Categories {
            get {
                return (from i in Items
                        where i.HasCategory
                        orderby i.Category
                        select i.Category).Distinct();
            }
        }

        public IEnumerable<Feed> Feeds {
            get { return Items.Select(i => i.Feed); }
        }

        public string FilterTerm {
            get { return _filterTerm; }
            set {
                if (_filterTerm != value) {
                    _filterTerm = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool IsFilterAll {
            get { return _isFilterAll; }
            set {
                if (_isFilterAll != value) {
                    _isFilterAll = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool IsFilterUnread {
            get { return _isFilterUnread; }
            set {
                if (_isFilterUnread != value) {
                    _isFilterUnread = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool IsFilterPodcasts {
            get { return _isFilterPodcasts; }
            set {
                if (_isFilterPodcasts != value) {
                    _isFilterPodcasts = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool IsFilterToday {
            get { return _isFilterToday; }
            set {
                if (_isFilterToday != value) {
                    _isFilterToday = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool IsFilterYesterday {
            get { return _isFilterYesterday; }
            set {
                if (_isFilterYesterday != value) {
                    _isFilterYesterday = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool IsFilterThisWeek {
            get { return _isFilterThisWeek; }
            set {
                if (_isFilterThisWeek != value) {
                    _isFilterThisWeek = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool CanUpdateFeeds {
            get { return _canUpdateFeeds; }
            set {
                if (_canUpdateFeeds != value) {
                    _canUpdateFeeds = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public int TotalUnread {
            get { return Items.Sum(i => i.Unread); }
        }

        public FeedListViewModel() {
            _eventAggregator = IoC.Get<IEventAggregator>();
            _eventAggregator.Subscribe(this);

            // Bind to CollectionView instead of actual items.
            FeedsView = CollectionViewSource.GetDefaultView(Items);
            FeedsView.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));
            FeedsView.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
            FeedsView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));//, null, StringComparison.OrdinalIgnoreCase));

            // Allows automatic update of filtering, sorting and grouping.
            _editableFeedsView = (IEditableCollectionView)FeedsView;

            LoadFeeds();
            InitializeTimer();

            IsFilterAll = CanUpdateFeeds = true;

            if (Settings.Default.UpdatesEnabled) {
                UpdateFeeds();
            }
            else if (Settings.Default.LastUpdated != DateTime.MinValue) {
                _eventAggregator.PublishOnUIThread(new StatusMessage(
                    "Last updated {0}",
                    Settings.Default.LastUpdated.ToString("dd/MM/yy - hh:mm tt")));
            }
        }

        public void ClearFeeds() {
            Items.Clear();
        }

        public void LoadFeeds() {
            try {
                var feeds = FeedSerializer.LoadFeeds(); // Load feeds from disk.

                Items.Clear();

                AddFeeds(feeds);
            }
            catch (Exception ex) {
                DialogHelper.ShowError("Load Feeds Error", ex.ToString());
            }
        }

        protected override void OnDeactivate(bool close) {
            if (close) {
                SaveFeeds();
            }

            base.OnDeactivate(close);
        }

        private void SaveFeeds() {
            try {
                FeedSerializer.SaveFeeds(Feeds); // Save feeds to disk.
            }
            catch (Exception ex) {
                DialogHelper.ShowError("Save Feeds Error", ex.ToString());
            }
        }

        public void Handle(BeginFeedEditMessage message) {
            // Tell the CollectionView we are going to edit a feed.
            _editableFeedsView.EditItem(message.Feed);
        }

        public void Handle(CommitFeedEditMessage message) {
            var selected = (message.Feed == ActiveItem);

            // Tell the CollectionView we have finished editing the feed and it 
            // can now redo any filtering, sorting or grouping operations.
            _editableFeedsView.CommitEdit();

            // Reselect feed.
            if (selected) {
                ActiveItem = message.Feed;
            }
        }

        // Update the category name of all the feeds in the specified category.
        public void Handle(EditCategoryMessage message) {
            var feeds = Items.Where(f => f.HasCategory && f.Category == message.OldName).ToList();

            foreach (var feed in feeds) {
                feed.UpdateCategory(message.NewName);
            }
        }

        public void Handle(RemoveOldItemsMessage message) {
            RemoveOldItems(message.Date, message.MinItems);
        }

        // The timer has been enabled/disabled or the interval has been changed, so the timer needs to be reinitialized.
        public void Handle(InitializeTimerMessage message) {
            if (Settings.Default.UpdatesEnabled) {
                _timerHelper.Initialize(Settings.Default.UpdateInterval);

                var dueTime = DateTime.Now.Add(Settings.Default.UpdateInterval);
                _eventAggregator.PublishOnUIThread(new StatusMessage(
                    "Update timer initialized, next update due at {0}",
                    dueTime.ToString("t")));
            }
            else {
                _timerHelper.Stop();

                _eventAggregator.PublishOnUIThread(new StatusMessage("Update timer stopped"));
            }
        }

        private void InitializeTimer() {
            _timerHelper = new TimerHelper();
            _timerHelper.Tick += _timerService_Tick;

            if (Settings.Default.UpdatesEnabled) {
                _timerHelper.Initialize(Settings.Default.UpdateInterval);
            }
        }

        // Update timer has elapsed and will now update all feeds.
        private void _timerService_Tick(object sender, EventArgs e) {
            UpdateFeeds();
        }

        // Update feeds using the threadpool so as not to block the UI.
        public void UpdateFeeds() {
            Task.Run(() => UpdateFeedsInternal());
        }

        public async void UpdateFeedsInternal() {
            try {
                CanUpdateFeeds = false;
                _eventAggregator.PublishOnUIThread(new StatusMessage("Updating..."));

                var tasks = Items.Select(i => i.Update());
                var results = await Task.WhenAll(tasks); // Each feed returns number of new items added.
                var total = results.Sum();

                Settings.Default.LastUpdated = DateTime.Now;
                var message = String.Format("Update finished at {0}", Settings.Default.LastUpdated.ToString("t"));
                if (total > 0) {
                    message += String.Format(" ({0} added)", "item".ToQuantity(total));
                    _eventAggregator.PublishOnUIThread(new NotifyBalloonMessage("Update Complete", message));
                }
                _eventAggregator.PublishOnUIThread(new StatusMessage(message));
            }
            finally {
                CanUpdateFeeds = true;
            }
        }

        public void AddFeed() {
            var vm = new AddFeedViewModel(Categories);
            vm.Action = AddFeed;
            vm.Activate();
        }

        public void AddFeed(Feed feed) {
            ActivateItem(new FeedViewModel(feed, _eventAggregator));

            _eventAggregator.PublishOnUIThread(new IsReadChangedMessage());
        }

        public void AddFeeds(IEnumerable<Feed> feeds) {
            Items.AddRange(feeds.Select(f => new FeedViewModel(f, _eventAggregator)));

            _eventAggregator.PublishOnUIThread(new IsReadChangedMessage());
        }

        public void RemoveFeed(FeedViewModel feed) {
            feed.TryClose();
        }

        public void MarkFeedAsRead() {
            ActiveItem.MarkAllAsRead(read: true);
        }

        public void MarkAllFeedsAsRead() {
            foreach (var feed in Items) {
                feed.MarkAllAsRead(read: true);
            }
        }

        private void RemoveOldItems(DateTime date, int minItems) {
            int itemCount = 0;
            int feedCount = 0;

            // Preserved feeds never get items removed.
            var feeds = Items.Where(i => !i.IsPreserved);

            // Remove any items from before the purge date but leave at least minItems.
            foreach (var feed in feeds) {
                var items = GetItemsToRemove(feed.Items, date, minItems).ToList();

                if (items.Any()) {
                    feed.RemoveItems(items);

                    itemCount += items.Count;
                    feedCount++;
                }
            }

            _eventAggregator.PublishOnUIThread(new StatusMessage(
                "Removed {0} from {1}",
                "item".ToQuantity(itemCount),
                "feed".ToQuantity(feedCount)));
        }

        private IEnumerable<ItemViewModel> GetItemsToRemove(IEnumerable<ItemViewModel> items, DateTime date, int minItems) {
            if (minItems > 0) {
                return items.OrderByDescending(i => i.Published).Skip(minItems).Where(i => i.Published < date);
            }

            return items.Where(i => i.Published < date);
        }

        public void FilterAll() {
            IsFilterAll = true;
            IsFilterUnread = IsFilterPodcasts = IsFilterToday = IsFilterThisWeek = IsFilterYesterday = false;
            FilterFeeds();
        }

        public void FilterUnread() {
            IsFilterUnread = true;
            IsFilterAll = IsFilterPodcasts = IsFilterToday = IsFilterThisWeek = IsFilterYesterday = false;
            FilterFeeds();
        }

        public void FilterPodcasts() {
            IsFilterPodcasts = true;
            IsFilterAll = IsFilterUnread = IsFilterToday = IsFilterThisWeek = IsFilterYesterday = false;
            FilterFeeds();
        }

        public void FilterToday() {
            IsFilterToday = true;
            IsFilterAll = IsFilterUnread = IsFilterPodcasts = IsFilterThisWeek = IsFilterYesterday = false;
            FilterFeeds();
        }

        public void FilterYesterday() {
            IsFilterYesterday = true;
            IsFilterAll = IsFilterUnread = IsFilterPodcasts = IsFilterThisWeek = IsFilterToday = false;
            FilterFeeds();
        }

        public void FilterThisWeek() {
            IsFilterThisWeek = true;
            IsFilterAll = IsFilterUnread = IsFilterPodcasts = IsFilterToday = IsFilterYesterday = false;
            FilterFeeds();
        }

        // Called each time the filter text changes.
        public async void FilterTermChanged() {
            var term = FilterTerm;

            // Wait to see if anything else is typed.
            await Task.Delay(FilterDelay);

            // If nothing else has been typed then filter the feeds.
            if (term == FilterTerm) {
                FilterFeeds();
            }
        }

        private void FilterFeeds() {
            var feed = ActiveItem;

            FeedsView.Filter = FilterFeed; // Actually filter the feeds.

            // Reselect previously selected item. 
            // Setting ActiveItem to null causes weird things to happen.
            if (feed != null) {
                ActiveItem = feed;
            }
        }

        private bool FilterFeed(object obj) {
            var feed = (FeedViewModel)obj;

            if (IsFilterAll) {
                return FilterFeedTitle(feed);
            }

            if (IsFilterUnread && feed.HasUnread) {
                return FilterFeedTitle(feed);
            }

            if (IsFilterPodcasts && feed.HasPodcasts) {
                return FilterFeedTitle(feed);
            }

            if (IsFilterToday && feed.WasUpdatedToday) {
                return FilterFeedTitle(feed);
            }

            if (IsFilterYesterday && feed.WasUpdatedYesterday) {
                return FilterFeedTitle(feed);
            }

            if (IsFilterThisWeek && feed.WasUpdatedThisWeek) {
                return FilterFeedTitle(feed);
            }

            return false;
        }

        private bool FilterFeedTitle(FeedViewModel feed) {
            if (String.IsNullOrEmpty(FilterTerm)) {
                return true;
            }

            if (FilterTerm[0] == '-') {
                return feed.Title.IndexOf(FilterTerm.Substring(1), StringComparison.OrdinalIgnoreCase) == -1;
            }

            return feed.Title.IndexOf(FilterTerm, StringComparison.OrdinalIgnoreCase) > -1;
        }

        public void Dispose() {
            if (_timerHelper != null) {
                _timerHelper.Dispose();
            }
        }
    }
}
