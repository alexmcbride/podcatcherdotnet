using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;

namespace PodcatcherDotNet.Helpers {
    // Download helper that deals with the fact that WebClient doesn't support timeouts.
    public class DownloadHelper : IDisposable {
        private TimeSpan _timeout;
        private WebClientWithTimeout _client;
        private Timer _timer;
        private Stream _stream;
        private bool _hasTimedOut;
        private bool _hasDownloadedData;

        public bool IsDownloading {
            get { return _client.IsBusy; }
        }

        public bool HasDownloadedData {
            get { return _hasDownloadedData; }
        }

        public event DownloadProgressChangedEventHandler DownloadProgressChanged;

        public DownloadHelper(TimeSpan timeout) {
            _timeout = timeout;

            _client = new WebClientWithTimeout(_timeout);
            _client.Headers.Add("user-agent", "PodcatcherDotNet/1.0");
            _client.DownloadProgressChanged += _client_DownloadProgressChanged;

            _timer = new Timer();
            _timer.Interval = _timeout.TotalMilliseconds;
            _timer.Elapsed += _timeoutTimer_Elapsed;
        }

        public static PodcastType GetPodcastType(string filename) {
            var ext = Path.GetExtension(filename);

            if (ext == ".mp3") {
                return PodcastType.Mp3;
            }

            if (ext == ".m4a") {
                return PodcastType.M4a;
            }

            return PodcastType.None;
        }

        private void _client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            _hasDownloadedData = true;

            OnDownloadProgressChangedEvent(e);
        }

        private void _timeoutTimer_Elapsed(object sender, ElapsedEventArgs e) {
            if (_timer.Enabled) {
                _timer.Stop();

                if (!_hasDownloadedData) {
                    _hasTimedOut = true;

                    Cancel();
                }
            }
        }

        public void Cancel() {
            if (IsDownloading) {
                _client.CancelAsync();
            }
        }

        public void Dispose() {
            if (_client != null) {
                _client.Dispose();
            }

            if (_timer != null) {
                _timer.Dispose();
            }

            if (_stream != null) {
                _stream.Dispose();
            }
        }

        public byte[] DownloadData(string address) {
            return _client.DownloadData(address);
        }

        public async Task<byte[]> DownloadDataAsync(string address) {
            _timer.Start();

            try {
                var data = await _client.DownloadDataTaskAsync(address);

                _timer.Stop();

                return data;
            }
            catch (WebException ex) {
                if (ex.Status == WebExceptionStatus.RequestCanceled) {
                    if (_hasTimedOut) {
                        throw new WebException("The connection has timed out.", WebExceptionStatus.Timeout);
                    }
                }
                throw;
            }
        }

        public XmlReader DownloadXml(string url) {
            _stream = _client.OpenRead(url);

            return XmlReader.Create(_stream);
        }

        public async Task<XmlReader> DownloadXmlAsync(string url) {
            var data = await DownloadDataAsync(url);

            _stream = new MemoryStream(data);

            return XmlReader.Create(_stream);
        }

        public async Task DownloadFileAsync(string url, string filename) {
            try {
                _timer.Start();

                await _client.DownloadFileTaskAsync(url, filename);

                _timer.Stop();
            }
            catch (WebException ex) {
                // Cleanup anything we've downloaded so far.
                if (File.Exists(filename)) {
                    File.Delete(filename);
                }

                if (ex.Status == WebExceptionStatus.RequestCanceled) {
                    if (_hasTimedOut) {
                        throw new WebException("The connection has timed out.", WebExceptionStatus.Timeout);
                    }
                }
                throw;
            }
        }

        protected virtual void OnDownloadProgressChangedEvent(DownloadProgressChangedEventArgs e) {
            var temp = DownloadProgressChanged;
            if (temp != null) {
                temp(this, e);
            }
        }
    }
}
