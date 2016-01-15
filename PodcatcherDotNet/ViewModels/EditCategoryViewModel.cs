using Caliburn.Micro;
using PodcatcherDotNet.Messages;

namespace PodcatcherDotNet.ViewModels {
    public class EditCategoryViewModel : PropertyChangedBase {
        private IEventAggregator _eventAggregator;
        private string _name;

        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    NotifyOfPropertyChange();
                    _eventAggregator.PublishOnUIThread(new EditCategoryUpdatedMessage());
                }
            }
        }

        public string OldName { get; private set; }

        public bool IsUpdated {
            get { return _name != OldName; }
        }

        public EditCategoryViewModel(IEventAggregator eventAggregator, string name) {
            _eventAggregator = eventAggregator;
            Name = name;
            OldName = name;
        }
    }
}
