using Caliburn.Micro;
using PodcatcherDotNet.Helpers;
using PodcatcherDotNet.Messages;
using System;
using System.Linq;

namespace PodcatcherDotNet.ViewModels {
    public class RemoveItemsViewModel : Conductor<RemoveDateViewModel>.Collection.OneActive {
        private IEventAggregator _eventAggregator;
        private int _minItems;

        public int MinItems {
            get { return _minItems; }
            set {
                if (_minItems != value) {
                    _minItems = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool CanRemove {
            get { return ActiveItem != null; }
        }

        public RemoveItemsViewModel() {
            _eventAggregator = IoC.Get<IEventAggregator>();
        }

        protected override void OnActivate() {
            base.OnActivate();

            var now = DateTime.Now;

            Items.Add(new RemoveDateViewModel(now.Subtract(TimeSpan.FromDays(1)), "the past day"));
            Items.Add(new RemoveDateViewModel(now.Subtract(TimeSpan.FromDays(7)), "the past week"));
            Items.Add(new RemoveDateViewModel(now.Subtract(TimeSpan.FromDays(14)), "the past two weeks"));
            Items.Add(new RemoveDateViewModel(now.Subtract(TimeSpan.FromDays(28)), "the past four weeks"));
            Items.Add(new RemoveDateViewModel(now.Subtract(TimeSpan.FromDays(365)), "the past year"));

            ActiveItem = Items.First();

            MinItems = 10;
        }

        protected override void ChangeActiveItem(RemoveDateViewModel newItem, bool closePrevious) {
            base.ChangeActiveItem(newItem, closePrevious);

            NotifyOfPropertyChange("CanRemove");
        }

        public void Remove() {
            var date = ActiveItem.Date;

            var result = DialogHelper.ShowYesNo("Remove Items", "Do you want to remove all items from before {0}?", date.ToShortDateString());

            if (result) {
                _eventAggregator.PublishOnUIThread(new RemoveOldItemsMessage(date, MinItems));

                TryClose(true);
            }
        }
    }
}
