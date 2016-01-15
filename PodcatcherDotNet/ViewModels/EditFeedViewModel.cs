using Caliburn.Micro;
using PodcatcherDotNet.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PodcatcherDotNet.ViewModels {
    public class EditFeedViewModel : Screen {
        private IEventAggregator _eventAggregator;
        private FeedViewModel _feed;
        private string _title;
        private string _description;
        private string _url;
        private bool _isPreserved;
        private string _category;
        private ObservableCollection<string> _categories;
        private bool _canSave;

        public string Title {
            get { return _title; }
            set {
                if (_title != value) {
                    _title = value;
                    NotifyOfPropertyChange();
                    CanSave = IsUpdated(value, _feed.Title);
                }
            }
        }

        public string Description {
            get { return _description; }
            set {
                if (_description != value) {
                    _description = value;
                    NotifyOfPropertyChange();
                    CanSave = IsUpdated(value, _feed.Description, true);
                }
            }
        }

        public string Url {
            get { return _url; }
            set {
                if (_url != value) {
                    _url = value;
                    NotifyOfPropertyChange();
                    CanSave = IsUpdated(value, _feed.Url);
                }
            }
        }

        public bool CanSave {
            get { return _canSave; }
            set {
                if (_canSave != value) {
                    _canSave = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool IsPreserved {
            get { return _isPreserved; }
            set {
                if (_isPreserved != value) {
                    _isPreserved = value;
                    NotifyOfPropertyChange();
                    CanSave = IsUpdated(value, _feed.IsPreserved);
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
                    CanSave = IsUpdated(value, _feed.Category, true);
                }
            }
        }

        public EditFeedViewModel(FeedViewModel feed, IEnumerable<string> categories) {
            _eventAggregator = IoC.Get<IEventAggregator>();
            _feed = feed;

            Title = feed.Title;
            Description = feed.Description;
            Url = feed.Url;
            IsPreserved = feed.IsPreserved;

            if (feed.HasCategory) {
                Category = feed.Category;
            }

            CanSave = false;
            Categories = new ObservableCollection<string>(categories);
        }

        public void Activate() {
            _eventAggregator.PublishOnUIThread(new ActivateScreenMessage(this));
        }

        private static bool IsUpdated(string value, string original, bool allowEmpty = false) {
            if (allowEmpty || !String.IsNullOrWhiteSpace(value)) {
                return value != original;
            }
            return false;
        }

        private static bool IsUpdated(bool value, bool original) {
            return value != original;
        }

        public void Save() {
            _feed.BeginFeedEdit();
            _feed.Title = Title;
            _feed.Description = Description;
            _feed.Url = Url;
            _feed.IsPreserved = IsPreserved;
            _feed.Category = Category.Trim();
            _feed.CommitFeedEdit();

            TryClose(true);
        }
    }
}
