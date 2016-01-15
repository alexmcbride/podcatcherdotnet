using PodcatcherDotNet.ViewModels;

namespace PodcatcherDotNet.Messages {
    public class ClearDownloadMessage {
        public DownloadViewModel Download { get; private set; }

        public ClearDownloadMessage(DownloadViewModel podcast) {
            Download = podcast;
        }
    }
}
