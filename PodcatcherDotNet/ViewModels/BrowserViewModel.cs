using Caliburn.Micro;
using PodcatcherDotNet.Messages;
using System;

namespace PodcatcherDotNet.ViewModels {
    // The WebBrowser doesn't support binding so we handle everything by sending and receiving messages.
    public class BrowserViewModel : Screen, IHandle<BrowserUrlChangedMessage> {
        private IEventAggregator _eventAggregator;
        private string _url;
        private bool _canBack;
        private bool _canForward;

        public string Url {
            get { return _url; }
            set {
                if (_url != value) {
                    _url = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool CanBack {
            get { return _canBack; }
            set {
                if (_canBack != value) {
                    _canBack = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool CanForward {
            get { return _canForward; }
            set {
                if (_canForward != value) {
                    _canForward = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public BrowserViewModel() {
            _eventAggregator = IoC.Get<IEventAggregator>();
            _eventAggregator.Subscribe(this);
        }

        public void Handle(BrowserUrlChangedMessage message) {
            Url = message.Url;
            CanBack = message.CanGoBack;
            CanForward = message.CanGoForward;
        }

        public void Navigate(string url) {
            _eventAggregator.PublishOnUIThread(new BrowserNavigateMessage(url));
        }

        public void RefreshPage() {
            _eventAggregator.PublishOnUIThread(new BrowserActionMessage(BrowserAction.Refresh));
        }

        public void Back() {
            _eventAggregator.PublishOnUIThread(new BrowserActionMessage(BrowserAction.Back));
        }

        public void Forward() {
            _eventAggregator.PublishOnUIThread(new BrowserActionMessage(BrowserAction.Forward));
        }

        public void Activate(string url) {
            _eventAggregator.PublishOnUIThread(new ActivateScreenMessage(this));
            Navigate(url);
        }
    }
}
