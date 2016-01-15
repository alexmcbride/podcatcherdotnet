using System;
using System.Net;

namespace PodcatcherDotNet.Helpers {
    // This only works for synchronous requests, async requests need to use their own timer.
    public class WebClientWithTimeout : WebClient {
        private TimeSpan _timeout;

        public WebClientWithTimeout(TimeSpan timeout) {
            _timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address) {
            var request = base.GetWebRequest(address);

            request.Timeout = (int)_timeout.TotalMilliseconds;

            return request;
        }
    }
}
