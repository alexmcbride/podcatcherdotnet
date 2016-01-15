
namespace PodcatcherDotNet.Messages {
    public class BrowserUrlChangedMessage {
        public string Url { get; private set; }
        public bool CanGoBack { get; private set; }
        public bool CanGoForward { get; private set; }

        public BrowserUrlChangedMessage(string url, bool canGoBack, bool canGoForward) {
            Url = url;
            CanGoBack = canGoBack;
            CanGoForward = canGoForward;
        }
    }
}
