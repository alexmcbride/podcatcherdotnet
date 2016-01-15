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
using System.Net;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PodcatcherDotNet.ViewModels {
    [DebuggerDisplay("Title = {Title}, Category = {Category}")]
    public class FeedViewModel : Conductor<ItemViewModel>.Collection.OneActive, IEditableObject {
        private static readonly TimeSpan FilterDelay = TimeSpan.FromMilliseconds(500);

        private IEventAggregator _eventAggregator;
        private ICollectionView _itemsView;
        private bool _isUpdating;
        private string _filterTerm;
        private bool _isFilterAll;
        private bool _isFilterUnread;
        private bool _isFilterPodcasts;
        private bool _isFilterToday;
        private bool _isFilterYesterday;
        private bool _isFilterThisWeek;

        public Feed Feed { get; private set; }

        public string Title {
            get { return Feed.Title; }
            set {
                if (Feed.Title != value) {
                    Feed.Title = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public string Description {
            get { return Feed.Description; }
            set {
                if (Feed.Description != value) {
                    Feed.Description = value;
                    NotifyOfPropertyChange();
                    NotifyOfPropertyChange("HasDescription");
                }
            }
        }

        public bool HasDescription {
            get { return !String.IsNullOrEmpty(Description); }
        }

        public DateTime Updated {
            get { return Feed.Updated; }
        }

        public string UpdatedPretty {
            get { return Feed.Updated.Humanize(utcDate: false); }
        }

        public bool WasUpdatedToday {
            get { return Items.Any(i => i.WasPublishedToday); }
        }

        public bool WasUpdatedYesterday {
            get { return Items.Any(i => i.WasPublishedYesterday); }
        }

        public bool WasUpdatedThisWeek {
            get { return Items.Any(i => i.WasPublishedThisWeek); }
        }

        public bool HasUnread {
            get { return Items.Any(i => !i.IsRead); }
        }

        public int Unread {
            get { return Items.Count(i => !i.IsRead); }
        }

        public bool HasPodcasts {
            get { return Items.Any(i => i.IsPodcast); }
        }

        public bool IsUpdating {
            get { return _isUpdating; }
            set {
                if (_isUpdating != value) {
                    _isUpdating = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public string Url {
            get { return Feed.Url; }
            set {
                if (Feed.Url != value) {
                    Feed.Url = value;
                    NotifyOfPropertyChange();
                }
            }
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

        public ICollectionView ItemsView {
            get { return _itemsView; }
            set {
                if (_itemsView != value) {
                    _itemsView = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool IsPreserved {
            get { return Feed.IsPreserved; }
            set {
                if (Feed.IsPreserved != value) {
                    Feed.IsPreserved = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public string Category {
            get {
                if (String.IsNullOrEmpty(Feed.Category)) {
                    return "Unassigned";
                }
                return Feed.Category;
            }

            set {
                if (Feed.Category != value) {
                    Feed.Category = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool HasCategory {
            get { return !String.IsNullOrEmpty(Feed.Category); }
        }

        public FeedViewModel(Feed feed, IEventAggregator eventAggregator) {
            Feed = feed;

            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            // Bind to ItemsView for sorting and filtering.
            ItemsView = CollectionViewSource.GetDefaultView(Items);
            ItemsView.SortDescriptions.Add(new SortDescription("Published", ListSortDirection.Descending));

            Items.AddRange(feed.Items.Select(i => new ItemViewModel(i, this, _eventAggregator)));

            IsFilterAll = true;
        }

        protected override void ChangeActiveItem(ItemViewModel newItem, bool closePrevious) {
            base.ChangeActiveItem(newItem, closePrevious);

            // When an active item is changed we mark it as read.
            if (newItem != null) {
                MarkAsRead(newItem, read: true);
            }
        }

        public void Remove() {
            var vm = new RemoveFeedViewModel(this);
            vm.Action = ((FeedListViewModel)Parent).RemoveFeed;
            vm.Activate();
        }

        public void EditFeed() {
            var vm = new EditFeedViewModel(this, ((FeedListViewModel)Parent).Categories);
            vm.Activate();
        }

        public void MarkAllAsRead() {
            MarkAllAsRead(read: true);
        }

        public void MarkAllAsUnread() {
            MarkAllAsRead(read: false);
        }

        public void MarkAllAsRead(bool read) {
            bool changed = false;

            foreach (var item in Items) {
                if (item.IsRead != read) {
                    item.IsRead = read;
                    changed = true;
                }
            }

            if (changed) {
                NotifyOfPropertyChange("HasUnread");
                NotifyOfPropertyChange("Unread");

                _eventAggregator.PublishOnUIThread(new IsReadChangedMessage());
            }
        }

        public void MarkAsRead(ItemViewModel item, bool read) {
            if (item.IsRead != read) {
                item.IsRead = read;

                NotifyOfPropertyChange("HasUnread");
                NotifyOfPropertyChange("Unread");

                _eventAggregator.PublishOnUIThread(new IsReadChangedMessage());
            }
        }

        // Updates the feed and adds any new items that are downloaded from the syndication feed.
        public async Task<int> Update() {
            IsUpdating = true;
            int result = 0;

            try {
                result = await UpdateInternal();
            }
            catch (Exception ex) {
                //DialogHelper.ShowError("Feed Error", "Feed '{0}' error: {1}", Title, ex.Message);
                LogHelper.WriteLine("Feed Error for '{0}': {1}", Title, ex.ToString());
            }
            finally {
                IsUpdating = false;
            }

            return result;
        }

        private async Task<int> UpdateInternal() {
            try {
                var items = await FeedFactory.UpdateFeedAsync(Feed, Settings.Default.DownloadTimeout);

                if (items.Any()) {
                    AddItems(items);
                }

                return items.Count;
            }
            catch (WebException ex) {
                if (ex.Status == WebExceptionStatus.Timeout) {
                    Debug.WriteLine("timeout '{0}'", Title);
                }
                else throw;
            }

            return 0;
        }

        public void AddItems(IEnumerable<Item> items) {
            AddItems(items.Select(i => new ItemViewModel(i, this, _eventAggregator)));
        }

        public void AddItems(IEnumerable<ItemViewModel> items) {
            BeginFeedEdit();

            Items.AddRange(items);

            NotifyOfPropertyChange("Updated");
            NotifyOfPropertyChange("HasUnread");
            NotifyOfPropertyChange("Unread");
            _eventAggregator.PublishOnUIThread(new IsReadChangedMessage());

            CommitFeedEdit();
        }

        public void RemoveItems(IEnumerable<ItemViewModel> items) {
            BeginFeedEdit();

            Items.RemoveRange(items);

            NotifyOfPropertyChange("Updated");
            NotifyOfPropertyChange("HasUnread");
            NotifyOfPropertyChange("Unread");
            _eventAggregator.PublishOnUIThread(new IsReadChangedMessage());

            CommitFeedEdit();

            // Remember to remove the items from the actual feed and not just the ViewModel!
            Feed.RemoveItems(items.Select(i => i.Item));
        }

        public void RemoveItem(ItemViewModel item) {
            BeginFeedEdit();

            Items.Remove(item);

            NotifyOfPropertyChange("Updated");
            NotifyOfPropertyChange("HasUnread");
            NotifyOfPropertyChange("Unread");
            _eventAggregator.PublishOnUIThread(new IsReadChangedMessage());

            CommitFeedEdit();

            Feed.Items.Remove(item.Item);
        }

        public void UpdateCategory(string category) {
            BeginFeedEdit();

            Category = category.Trim();

            CommitFeedEdit();
        }

        public void BeginFeedEdit() {
            // Tell feed we are going to update. This uses IEditableCollectionView so that sorting, 
            // grouping and filtering are correctly updated once we have finished editing the feed.
            _eventAggregator.PublishOnUIThread(new BeginFeedEditMessage(this));
        }

        public void CommitFeedEdit() {
            // Tell feed we have finished updating and that sorting and filtering can be redone.
            _eventAggregator.PublishOnUIThread(new CommitFeedEditMessage(this));
        }

        private bool CheckValidSiteUrl(string url) {
            if (!WindowsHelper.IsValidUrl(url)) {
                DialogHelper.ShowError("Invalid URL", "The site URL '{0}' is not valid.", url);
                return false;
            }
            return true;
        }

        public void VisitSite() {
            if (CheckValidSiteUrl(Feed.SiteUrl)) {
                var vm = new BrowserViewModel();
                vm.Activate(Feed.SiteUrl);
            }
        }

        public void VisitSiteExternal() {
            try {
                if (CheckValidSiteUrl(Feed.SiteUrl)) {
                    WindowsHelper.OpenBrowser(Feed.SiteUrl, Settings.Default.UseCustomBrowser, Settings.Default.CustomBrowserExe);
                }
            }
            catch (Exception ex) {
                DialogHelper.ShowError("Browser Error", ex.ToString());
            }
        }

        protected override void OnDeactivate(bool close) {
            base.OnDeactivate(close);

            if (close) {
                _eventAggregator.PublishOnUIThread(new IsReadChangedMessage());
            }
        }

        public void FilterAll() {
            IsFilterAll = true;
            IsFilterUnread = IsFilterPodcasts = IsFilterToday = IsFilterThisWeek = IsFilterYesterday = false;
            FilterItems();
        }

        public void FilterUnread() {
            IsFilterUnread = true;
            IsFilterAll = IsFilterPodcasts = IsFilterToday = IsFilterThisWeek = IsFilterYesterday = false;
            FilterItems();
        }

        public void FilterPodcasts() {
            IsFilterPodcasts = true;
            IsFilterAll = IsFilterUnread = IsFilterToday = IsFilterThisWeek = IsFilterYesterday = false;
            FilterItems();
        }

        public void FilterToday() {
            IsFilterToday = true;
            IsFilterAll = IsFilterUnread = IsFilterPodcasts = IsFilterThisWeek = IsFilterYesterday = false;
            FilterItems();
        }

        public void FilterYesterday() {
            IsFilterYesterday = true;
            IsFilterAll = IsFilterUnread = IsFilterPodcasts = IsFilterThisWeek = IsFilterToday = false;
            FilterItems();
        }

        public void FilterThisWeek() {
            IsFilterThisWeek = true;
            IsFilterAll = IsFilterUnread = IsFilterPodcasts = IsFilterToday = IsFilterYesterday = false;
            FilterItems();
        }

        // Called whenever filter text changes.
        public async void FilterTermChanged() {
            var term = FilterTerm;

            // Wait to see if anything else gets typed.
            await Task.Delay(FilterDelay);

            // Nothing else, so perform filter.
            if (term == FilterTerm) {
                FilterItems();
            }
        }

        public void FilterItems() {
            var item = ActiveItem;

            ItemsView.Filter = FilterItem;

            ActiveItem = item; // reselect previously selected item.
        }

        private bool FilterItem(object obj) {
            var item = (ItemViewModel)obj;

            if (IsFilterAll) {
                return FilterItemTitle(item);
            }

            if (IsFilterUnread && !item.IsRead) {
                return FilterItemTitle(item);
            }

            if (IsFilterPodcasts && item.IsPodcast) {
                return FilterItemTitle(item);
            }

            if (IsFilterToday && item.WasPublishedToday) {
                return FilterItemTitle(item);
            }

            if (IsFilterYesterday && item.WasPublishedYesterday) {
                return FilterItemTitle(item);
            }

            if (IsFilterThisWeek && item.WasPublishedThisWeek) {
                return FilterItemTitle(item);
            }

            return false;
        }

        private bool FilterItemTitle(ItemViewModel item) {
            if (String.IsNullOrEmpty(FilterTerm)) {
                return true;
            }

            if (FilterTerm[0] == '-') {
                return item.Title.IndexOf(FilterTerm.Substring(1), StringComparison.OrdinalIgnoreCase) == -1;
            }

            return item.Title.IndexOf(FilterTerm, StringComparison.OrdinalIgnoreCase) > -1;
        }

        #region IEditableObject
        public void BeginEdit() { }
        public void CancelEdit() { }
        public void EndEdit() { }
        #endregion
    }
}
