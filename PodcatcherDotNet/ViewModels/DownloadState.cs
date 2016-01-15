namespace PodcatcherDotNet.ViewModels {
    public enum DownloadState {
        None,
        Downloading,
        UpdatingId3,
        ConvertingM4aToMp3,
        Finished,
        Cancelled,
        Error
    }
}
