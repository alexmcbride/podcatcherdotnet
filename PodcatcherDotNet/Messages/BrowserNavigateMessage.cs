
namespace PodcatcherDotNet.Messages {
    public class BrowserNavigateMessage {
        public string Url { get; private set; }

        public BrowserNavigateMessage(string url) {
            Url = url;
        }
    }
}
