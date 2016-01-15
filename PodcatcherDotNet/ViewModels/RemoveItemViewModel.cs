using Caliburn.Micro;
using PodcatcherDotNet.Messages;
using System;

namespace PodcatcherDotNet.ViewModels {
    public class RemoveItemViewModel : Screen {
        private ItemViewModel _item;
        private IEventAggregator _eventAggregator;

        public string Title {
            get { return _item.Title; }
        }

        public string FeedTitle {
            get { return _item.FeedTitle; }
        }

        public string PublishedPretty {
            get { return _item.PublishedPretty; }
        }

        public Action<ItemViewModel> Action { get; set; }

        public RemoveItemViewModel(ItemViewModel item) {
            _item = item;
            _eventAggregator = IoC.Get<IEventAggregator>();
        }

        public void Remove() {
            Action(_item);
            TryClose(true);
        }

        public void Activate() {
            _eventAggregator.PublishOnUIThread(new ActivateScreenMessage(this));
        }
    }
}
