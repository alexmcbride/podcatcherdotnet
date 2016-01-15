using Caliburn.Micro;
using PodcatcherDotNet.Messages;
using System;

namespace PodcatcherDotNet.ViewModels {
    public class RemoveFeedViewModel : Screen {
        private FeedViewModel _feed;
        private IEventAggregator _eventAggregator;

        public string Title {
            get { return _feed.Title; }
        }

        public int Unread {
            get { return _feed.Unread; }
        }

        public int ItemCount {
            get { return _feed.Items.Count; }
        }

        public string Updated {
            get { return _feed.UpdatedPretty; }
        }

        public Action<FeedViewModel> Action { get; set; }

        public RemoveFeedViewModel(FeedViewModel feed) {
            _eventAggregator = IoC.Get<IEventAggregator>();
            _feed = feed;
            NotifyOfPropertyChange("Title");
            NotifyOfPropertyChange("Unread");
            NotifyOfPropertyChange("ItemCount");
            NotifyOfPropertyChange("Updated");
        }

        public void Activate() {
            _eventAggregator.PublishOnUIThread(new ActivateScreenMessage(this));
        }

        public void Remove() {
            Action(_feed);
            TryClose(true);
        }
    }
}
