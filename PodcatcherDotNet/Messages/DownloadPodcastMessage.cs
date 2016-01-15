using PodcatcherDotNet.ViewModels;

namespace PodcatcherDotNet.Messages {
    public class DownloadPodcastMessage {
        public ItemViewModel Item { get; private set; }
        public string FileName { get; private set; }

        public DownloadPodcastMessage(ItemViewModel item, string filename) {
            Item = item;
            FileName = filename;
        }
    }
}
