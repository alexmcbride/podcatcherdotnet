
namespace PodcatcherDotNet.Messages {
    public class DownloadEditedMessage {
        public string FileName { get; private set; }
        public string Title { get; private set; }
        public string Artist { get; private set; }
        public string Album { get; private set; }

        public DownloadEditedMessage(string filename, string title, string artist, string album) {
            FileName = filename;
            Title = title;
            Artist = artist;
            Album = album;
        }
    }
}
