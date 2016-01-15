using PodcatcherDotNet.ViewModels;

namespace PodcatcherDotNet.Messages {
    public class BeginFeedEditMessage {
        public FeedViewModel Feed { get; private set; }

        public BeginFeedEditMessage(FeedViewModel feed) {
            Feed = feed;
        }
    }
}
