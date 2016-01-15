using Caliburn.Micro;
using PodcatcherDotNet.Helpers;
using PodcatcherDotNet.Messages;
using PodcatcherDotNet.Models;
using PodcatcherDotNet.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace PodcatcherDotNet.ViewModels {
    public class AddFeedViewModel : Screen {
        private IEventAggregator _eventAggregator;
        private Feed _feed;
        private string _url;
        private string _title;
        private string _category;
        private ObservableCollection<string> _categories;

        public string Url {
            get { return _url; }
            set {
                if (_url != value) {
                    _url = value;
                    NotifyOfPropertyChange();
                    NotifyOfPropertyChange("CanLoadFeed");
                    NotifyOfPropertyChange("CanAdd");
                }
            }
        }

        public string Title {
            get { return _title; }
            set {
                if (_title != value) {
                    _title = value;
                    NotifyOfPropertyChange();
                    NotifyOfPropertyChange("CanAdd");
                }
            }
        }

        public ObservableCollection<string> Categories {
            get { return _categories; }
            set {
                if (_categories != value) {
                    _categories = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public string Category {
            get { return _category; }
            set {
                if (_category != value) {
                    _category = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public Action<Feed> Action { get; set; }

        public bool CanLoadFeed {
            get { return WindowsHelper.IsValidUrl(Url); }
        }

        public bool CanAdd {
            get { return !String.IsNullOrWhiteSpace(Title) && WindowsHelper.IsValidUrl(Url); }
        }

        public AddFeedViewModel(IEnumerable<string> categories) {
            _eventAggregator = IoC.Get<IEventAggregator>();
            Categories = new ObservableCollection<string>(categories);

            string clipboard = Clipboard.GetText();
            if (!String.IsNullOrEmpty(clipboard) && WindowsHelper.IsValidUrl(clipboard)) {
                Url = clipboard;
            }
        }

        public void Activate() {
            _eventAggregator.PublishOnUIThread(new ActivateScreenMessage(this));
        }

        public void LoadFeed() {
#if !DEBUG
            try {
#endif
                _feed = FeedFactory.DownloadFeed(Url, Settings.Default.DownloadTimeout);

                Title = _feed.Title;
#if !DEBUG
            }
            catch (Exception ex) {
                DialogHelper.ShowError("Load Feed Error", ex.Message);
            }
#endif
        }

        public void Add() {
            try {
                if (_feed == null) {
                    _feed = FeedFactory.DownloadFeed(Url, Settings.Default.DownloadTimeout);
                }

                _feed.Title = Title;
                _feed.Category = Category.Trim();

                Action(_feed);

                TryClose(true);
            }
            catch (Exception ex) {
                DialogHelper.ShowError("Load Feed Error", ex.Message);
            }
        }
    }
}
