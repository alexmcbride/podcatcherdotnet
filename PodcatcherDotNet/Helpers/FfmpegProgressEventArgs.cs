using System;

namespace PodcatcherDotNet.Helpers {
    public class FfmpegProgressEventArgs : EventArgs {
        public int Percentage { get; private set; }

        public FfmpegProgressEventArgs(int percentage) {
            Percentage = percentage;
        }
    }
}
