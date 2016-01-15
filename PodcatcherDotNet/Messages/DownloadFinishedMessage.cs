using System;

namespace PodcatcherDotNet.Messages {
    public class DownloadFinishedMessage {
        public string Title { get; private set; }
        public DateTime DownloadedDate { get; private set; }
        public string FileName { get; private set; }

        public DownloadFinishedMessage(string title, DateTime downloadedDate, string filename) {
            Title = title;
            DownloadedDate = downloadedDate;
            FileName = filename;
        }
    }
}
