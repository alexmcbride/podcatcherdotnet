using Caliburn.Micro;
using PodcatcherDotNet.Messages;
using System.Collections.Generic;
using System.Linq;

namespace PodcatcherDotNet.ViewModels {
    public class EditCategoriesViewModel : Conductor<EditCategoryViewModel>.Collection.OneActive, IHandle<EditCategoryUpdatedMessage> {
        private IEventAggregator _eventAggregator;

        public bool CanSave {
            get { return Items.Any(i => i.IsUpdated); }
        }

        public bool HasCategories {
            get { return Items.Any(); }
        }

        public EditCategoriesViewModel(IEnumerable<string> categories) {
            _eventAggregator = IoC.Get<IEventAggregator>();
            _eventAggregator.Subscribe(this);

            Items.Clear();
            Items.AddRange(categories.Select(c => new EditCategoryViewModel(_eventAggregator, c)));
            NotifyOfPropertyChange("HasCategories");
        }

        public void Handle(EditCategoryUpdatedMessage message) {
            NotifyOfPropertyChange("CanSave");
        }

        public void Save() {
            var categories = Items.Where(i => i.IsUpdated);
            foreach (var category in categories) {
                _eventAggregator.PublishOnUIThread(new EditCategoryMessage(category.OldName, category.Name));
            }
            TryClose(true);
        }
    }
}
