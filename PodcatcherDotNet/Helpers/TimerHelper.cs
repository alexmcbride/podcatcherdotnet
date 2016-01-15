using System;
using System.Windows.Threading;

namespace PodcatcherDotNet.Helpers {
    public class TimerHelper : IDisposable {
        private readonly DispatcherTimer _timer;

        public event EventHandler Tick {
            add { _timer.Tick += value; }
            remove { _timer.Tick -= value; }
        }

        public TimerHelper() {
            _timer = new DispatcherTimer();
        }

        public void Initialize(TimeSpan interval) {
            if (_timer.IsEnabled) {
                _timer.Stop();
            }

            _timer.Interval = interval;
            _timer.Start();
        }

        public void Stop() {
            _timer.Stop();
        }

        public void Dispose() {
            Stop();
        }
    }
}
