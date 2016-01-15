using Caliburn.Micro;
using PodcatcherDotNet.Messages;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PodcatcherDotNet.Views {
    public partial class BrowserView : UserControl, IHandle<BrowserNavigateMessage>, IHandle<BrowserActionMessage> {
        private IEventAggregator _eventAggregator;
        private bool _areScriptErrorsHidden;

        public BrowserView() {
            InitializeComponent();

            _eventAggregator = IoC.Get<IEventAggregator>();
            _eventAggregator.Subscribe(this);
        }

        public void Handle(BrowserNavigateMessage message) {
            _webBrowser.Navigate(message.Url);

            //((IKeyboardInputSink)_webBrowser).TabInto(new TraversalRequest(FocusNavigationDirection.First));
        }

        public void Handle(BrowserActionMessage message) {
            if (message.Action == BrowserAction.Refresh) {
                _webBrowser.Refresh();
            }
            else if (message.Action == BrowserAction.Back && _webBrowser.CanGoBack) {
                _webBrowser.GoBack();
            }
            else if (message.Action == BrowserAction.Forward && _webBrowser.CanGoForward) {
                _webBrowser.GoForward();
            }
        }

        private void CustomBrowser_Navigated(object sender, NavigationEventArgs e) {
            // HideScriptErrors can only be run after navigate has completed.
            if (!_areScriptErrorsHidden) {
                HideScriptErrors(_webBrowser, hide: true);

                _areScriptErrorsHidden = true;
            }

            _eventAggregator.PublishOnUIThread(new BrowserUrlChangedMessage(e.Uri.ToString(), _webBrowser.CanGoBack, _webBrowser.CanGoForward));
        }

        // Uses reflection to suppress Javascript errors from COM, otherwise web pages spam them all the time.
        // See: http://stackoverflow.com/questions/1298255/
        public void HideScriptErrors(WebBrowser browser, bool hide) {
            var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);

            if (fiComWebBrowser == null) {
                return;
            }

            var objComWebBrowser = fiComWebBrowser.GetValue(browser);
            if (objComWebBrowser == null) {
                browser.Loaded += (o, s) => HideScriptErrors(browser, hide); // In case we are too early
                return;
            }

            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
        }
    }
}
