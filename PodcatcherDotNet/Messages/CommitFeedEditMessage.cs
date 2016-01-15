using PodcatcherDotNet.ViewModels;

namespace PodcatcherDotNet.Messages {
    public class CommitFeedEditMessage {
        public FeedViewModel Feed { get; private set; }

        public CommitFeedEditMessage(FeedViewModel feed) {
            Feed = feed;
        }
    }
}
