
namespace PodcatcherDotNet.Messages {
    public enum BrowserAction {
        None,
        Refresh,
        Back,
        Forward
    }

    public class BrowserActionMessage {
        public BrowserAction Action { get; private set; }

        public BrowserActionMessage(BrowserAction action) {
            Action = action;
        }
    }
}
