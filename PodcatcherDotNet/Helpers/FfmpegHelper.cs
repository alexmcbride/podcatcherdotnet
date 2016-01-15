using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace PodcatcherDotNet.Helpers {
    public class FfmpegHelper : IDisposable {
        private static readonly string FfmpegExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");

        private TimeSpan _duration = TimeSpan.Zero;
        private FfmpegState _state = FfmpegState.None;
        private TaskCompletionSource<bool> _task;
        private Process _ffmpeg;
        private string _m4aFileName;
        private string _lastMessage;

        public event EventHandler<FfmpegProgressEventArgs> Progress;

        public bool IsRunning {
            get { return _state == FfmpegState.Running; }
        }

        public string Mp3FileName { get; private set; }

        public FfmpegHelper(string m4aFileName) {
            _m4aFileName = m4aFileName;
            Mp3FileName = GetMp3FileName(m4aFileName);

            _ffmpeg = new Process();
            _ffmpeg.StartInfo.FileName = FfmpegExePath;
            _ffmpeg.StartInfo.Arguments = String.Format("-i \"{0}\" -acodec libmp3lame -ab 128k \"{1}\" -y", _m4aFileName, Mp3FileName);
            _ffmpeg.StartInfo.CreateNoWindow = true;
            _ffmpeg.StartInfo.UseShellExecute = false;
            _ffmpeg.StartInfo.WorkingDirectory = Path.GetDirectoryName(_m4aFileName);
            _ffmpeg.StartInfo.RedirectStandardError = true;
            _ffmpeg.StartInfo.RedirectStandardInput = true;
            _ffmpeg.ErrorDataReceived += _ffmpeg_ErrorDataReceived;
            _ffmpeg.Exited += _ffmpeg_Exited;
            _ffmpeg.EnableRaisingEvents = true;
        }

        public static bool IsFfmpegExeInDirectory() {
            return File.Exists(FfmpegExePath);
        }

        private static string GetMp3FileName(string sourceM4a) {
            var dir = Path.GetDirectoryName(sourceM4a);
            var name = Path.GetFileNameWithoutExtension(sourceM4a);
            return Path.Combine(dir, name + ".mp3");
        }

        public Task ConvertAsync() {
            if (IsRunning) {
                throw new InvalidOperationException("operation in progress");
            }

            _state = FfmpegState.None;
            _task = new TaskCompletionSource<bool>();

            try {
                _ffmpeg.Start();
                _ffmpeg.BeginErrorReadLine();
            }
            catch (Exception ex) {
                _task.SetException(ex);
            }

            return _task.Task;
        }

        public void Cancel() {
            _state = FfmpegState.Cancelled;

            // Press 'q' to quit...
            _ffmpeg.StandardInput.Write('q');
        }

        // For some reason FFmpeg outputs on stderr rather than stdout.
        private void _ffmpeg_ErrorDataReceived(object sender, DataReceivedEventArgs e) {
            try {
                if (!String.IsNullOrEmpty(e.Data)) {
                    _lastMessage = e.Data;
                    HandleOutput(e.Data);
                }
            }
            catch (Exception ex) {
                _task.SetException(ex);
            }
        }

        private void _ffmpeg_Exited(object sender, EventArgs e) {
            try {
                if (_state == FfmpegState.Finished) {
                    // Delete original file.
                    File.Delete(_m4aFileName);

                    _task.SetResult(true);
                }
                else if (_state == FfmpegState.Cancelled) {
                    // Delete currently written file.
                    File.Delete(Mp3FileName);

                    _task.SetCanceled();
                }
                else {
                    // If not finished or cancelled must be error. If error then last message *should* be error message.
                    File.Delete(Mp3FileName);

                    throw new InvalidOperationException(String.Format("FFmpeg error: {0}", _lastMessage));
                }
            }
            catch (Exception ex) {
                _task.SetException(ex);
            }
        }

        // We have to parse the crappy FFmpeg output.
        private void HandleOutput(string line) {
            // We need to get the duration to work out the percentage complete later.
            if (line.StartsWith("  Duration")) {
                _duration = GetDuration(line);
            }

            // Progress updates start with either of these two things.
            if (line.StartsWith("size=") || line.StartsWith("frame=")) {
                var elapsed = GetElapsed(line);

                if (elapsed < _duration) {
                    // Progress.
                    _state = FfmpegState.Running;
                    var percentage = (int)((elapsed.TotalSeconds / _duration.TotalSeconds) * 100);
                    OnProgress(new FfmpegProgressEventArgs(percentage));
                }
                else {
                    // Finished
                    _state = FfmpegState.Finished;
                    OnProgress(new FfmpegProgressEventArgs(100));
                }
            }
        }

        private TimeSpan GetElapsed(string line) {
            var tokens = line.Split();

            foreach (var token in tokens) {
                var tokens2 = token.Split('=');
                if (tokens2.Length == 2 && tokens2[0] == "time") {
                    return TimeSpan.Parse(tokens2[1]);
                }
            }

            throw new InvalidOperationException("time token not found");
        }

        private static TimeSpan GetDuration(string line) {
            var tokens = line.Split();
            var durationString = tokens[3].Substring(0, tokens[3].Length - 1);
            return TimeSpan.Parse(durationString);
        }

        protected virtual void OnProgress(FfmpegProgressEventArgs e) {
            var temp = Progress;
            if (temp != null) {
                temp(this, e);
            }
        }

        public void Dispose() {
            _ffmpeg.Dispose();
        }
    }
}
