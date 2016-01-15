using Caliburn.Micro;
using Humanizer;
using PodcatcherDotNet.Helpers;
using PodcatcherDotNet.Messages;
using PodcatcherDotNet.Models;
using PodcatcherDotNet.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PodcatcherDotNet.ViewModels {
    [DebuggerDisplay("Title = {Title}, Feed = {FeedTitle}")]
    public class DownloadViewModel : PropertyChangedBase, IDisposable {
        private static readonly TimeSpan DownloadUpdateInternal = TimeSpan.FromMilliseconds(100);

        private IEventAggregator _eventAggregator;
        private DownloadHelper _downloadHelper;
        private Download _download;
        private DownloadState _state;
        private DateTime _downloadedDate;
        private DispatcherTimer _updateTimer;
        private double _totalSize;
        private double _totalReceived;
        private string _downloadTime;
        private int _percentComplete;
        private double _downloadSpeed;
        private long _currentBytesReceived;
        private int _currentProgressPercentage;
        private long _currentTotalBytesToReceive;
        private PodcastType _podcastType;
        private FfmpegHelper _ffmpeg;

        public event EventHandler<EventArgs> DownloadProgressChanged;

        public string Title { get; private set; }
        public string FeedTitle { get; private set; }
        public string FileName { get; private set; }
        public string Url { get; private set; }
        public DateTime StartTime { get; private set; }

        // Used for saving download between program sessions.
        public Download Download {
            get {
                if (_download == null) {
                    _download = new Download {
                        Title = Title,
                        FeedTitle = FeedTitle,
                        Url = Url,
                        DownloadedDate = DownloadedDate,
                        FileName = FileName
                    };
                }
                return _download;
            }
        }

        public DateTime DownloadedDate {
            get { return _downloadedDate; }
            set {
                if (_downloadedDate != value) {
                    _downloadedDate = value;
                    NotifyOfPropertyChange();
                    NotifyOfPropertyChange("DownloadedDatePretty");
                }
            }
        }

        public string DownloadedDatePretty {
            get { return DownloadedDate.ToString("g"); }
        }

        public DownloadState State {
            get { return _state; }
            set {
                if (_state != value) {
                    _state = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        // MB
        public double TotalSize {
            get { return _totalSize; }
            set {
                if (_totalSize != value) {
                    _totalSize = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        // MB
        public double TotalReceived {
            get { return _totalReceived; }
            set {
                if (_totalReceived != value) {
                    _totalReceived = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public string DownloadTime {
            get { return _downloadTime; }
            set {
                if (_downloadTime != value) {
                    _downloadTime = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        // Mbps
        public double DownloadSpeed {
            get { return _downloadSpeed; }
            set {
                if (_downloadSpeed != value) {
                    _downloadSpeed = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public int PercentComplete {
            get { return _percentComplete; }
            set {
                if (_percentComplete != value) {
                    _percentComplete = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool IsActive {
            get { return State == DownloadState.Downloading || State == DownloadState.ConvertingM4aToMp3 || State == DownloadState.UpdatingId3; }
        }

        public DownloadViewModel() {
            _eventAggregator = IoC.Get<IEventAggregator>();
        }

        // Instantiated as active download.
        public DownloadViewModel(ItemViewModel item, string filename)
            : this() {
            _downloadHelper = new DownloadHelper(Settings.Default.DownloadTimeout);
            _downloadHelper.DownloadProgressChanged += _downloadHelper_DownloadProgressChanged;

            Title = item.Title;
            FeedTitle = item.FeedTitle;
            FileName = filename;
            Url = item.PodcastUrl;
            State = DownloadState.Downloading;
            StartTime = DateTime.Now;
            _podcastType = DownloadHelper.GetPodcastType(FileName);

            // Timer for UI updates.
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = DownloadUpdateInternal;
            _updateTimer.Tick += _updateTimer_Tick;

            StartAsync(); // Start download.
        }

        // Instantiated from disk.
        public DownloadViewModel(Download download)
            : this() {
            _download = download;

            Title = download.Title;
            FeedTitle = download.FeedTitle;
            FileName = download.FileName;
            Url = download.Url;
            DownloadedDate = download.DownloadedDate;
            StartTime = download.DownloadedDate; // StartTime used when sorting downloads.
            State = DownloadState.Finished;
        }

        public async void StartAsync() {
            try {
                await StartAsyncInternal();
            }
            catch (Exception ex) {
                State = DownloadState.Error;
                _eventAggregator.PublishOnUIThread(new DownloadCancelledMessage()); // TODO: change this to be its own DownloadErrorMessage event?
                DialogHelper.ShowError("Download Error", "Download '{0}' had the error: {1}", Title, ex.Message);
            }
        }

        private async Task StartAsyncInternal() {
            try {
                _updateTimer.Start();

                await _downloadHelper.DownloadFileAsync(Url, FileName);

                _updateTimer.Stop();

                if (_podcastType == PodcastType.M4a && Settings.Default.ConvertM4aToMp3 && FfmpegHelper.IsFfmpegExeInDirectory()) {
                    await ConvertM4aToMp3Async();
                }

                if (Settings.Default.ReplaceId3Tags) {
                    await ReplaceId3TagsAsync();
                }

                State = DownloadState.Finished;
                DownloadedDate = DateTime.Now;

                var message = String.Format("Download '{0} - {1}' finished at {2}", Title, FeedTitle, DownloadedDate.ToString("t"));
                _eventAggregator.PublishOnUIThread(new DownloadFinishedMessage(Title, DownloadedDate, FileName));
                _eventAggregator.PublishOnUIThread(new StatusMessage(message));
                _eventAggregator.PublishOnUIThread(new NotifyBalloonMessage("Download Complete", message));
            }
            catch (WebException ex) {
                if (ex.Status == WebExceptionStatus.RequestCanceled) {
                    State = DownloadState.Cancelled;
                    _eventAggregator.PublishOnUIThread(new DownloadCancelledMessage());
                }
                else {
                    throw;
                }
            }
        }

        private Task ReplaceId3TagsAsync() {
            State = DownloadState.UpdatingId3;

            var id3Tag = new Id3Tag {
                Album = FeedTitle,
                Artist = FeedTitle,
                Title = Title
            };

            return id3Tag.WriteAsync(FileName);
        }

        private async Task ConvertM4aToMp3Async() {
            State = DownloadState.ConvertingM4aToMp3;

            using (_ffmpeg = new FfmpegHelper(FileName)) {
                _ffmpeg.Progress += (sender, e) => {
                    PercentComplete = e.Percentage;
                    OnDownloadProgressChanged(EventArgs.Empty);
                };

                await _ffmpeg.ConvertAsync();

                // Update filename of downloaded file.
                FileName = _ffmpeg.Mp3FileName;
            }
        }

        public void Cancel() {
            if (_downloadHelper != null && State == DownloadState.Downloading) {
                _downloadHelper.Cancel(); // Causes WebException with RequestCanceled status to be thrown.
            }
        }

        public void Dispose() {
            if (_downloadHelper != null) {
                _downloadHelper.Dispose();
            }
        }

        private void _downloadHelper_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            _currentBytesReceived = e.BytesReceived;
            _currentProgressPercentage = e.ProgressPercentage;
            _currentTotalBytesToReceive = e.TotalBytesToReceive;
        }

        private void _updateTimer_Tick(object sender, EventArgs e) {
            const double BytesInMegabyte = 1048576.0;
            const double BitsInByte = 8.0;
            const double BitsInMegabit = 1000000.0;

            if (_downloadHelper.HasDownloadedData) {
                PercentComplete = _currentProgressPercentage;
                TotalSize = _currentTotalBytesToReceive / BytesInMegabyte; // MBs
                TotalReceived = _currentBytesReceived / BytesInMegabyte; // MBs

                var bytesPerSecond = _currentBytesReceived / (DateTime.Now - StartTime).TotalSeconds;
                var timeLeft = TimeSpan.FromSeconds((_currentTotalBytesToReceive - _currentBytesReceived) / bytesPerSecond);
                DownloadSpeed = (bytesPerSecond * BitsInByte) / BitsInMegabit; // Mbps

                if (timeLeft.TotalHours > 1) {
                    DownloadTime = "hour".ToQuantity((int)timeLeft.TotalHours);
                }
                else if (timeLeft.TotalMinutes > 1) {
                    DownloadTime = "minute".ToQuantity((int)timeLeft.TotalMinutes);
                }
                else {
                    DownloadTime = "second".ToQuantity(((int)timeLeft.TotalSeconds) + 1);
                }

                // Notify TaskbarItem progress bar to update.
                OnDownloadProgressChanged(e);
            }
        }

        public void Play() {
            try {
                if (File.Exists(FileName)) {
                    WindowsHelper.OpenMusicPlayer(FileName);
                }
                else {
                    DialogHelper.ShowError("File Not Found", "The file '{0}' could not be found", FileName);
                }
            }
            catch (Exception ex) {
                DialogHelper.ShowError("Play Download Error", ex.ToString());
            }
        }

        public void OpenFolder() {
            try {
                if (File.Exists(FileName)) {
                    WindowsHelper.OpenFolder(FileName, select: true);
                }
                else {
                    DialogHelper.ShowError("File Not Found", "The file '{0}' could not be found", FileName);
                }
            }
            catch (Exception ex) {
                DialogHelper.ShowError("Open Folder Error", ex.ToString());
            }
        }

        public void EditDownload() {
            var vm = new EditDownloadViewModel(FileName);
            vm.Activate();
        }

        public void Clear() {
            if (DialogHelper.ShowYesNo("Clear Download", "Clear the '{0}' download?", Title)) {
                _eventAggregator.PublishOnUIThread(new ClearDownloadMessage(this));
            }
        }

        protected virtual void OnDownloadProgressChanged(EventArgs e) {
            var temp = DownloadProgressChanged;
            if (temp != null) {
                temp(this, e);
            }
        }
    }
}
