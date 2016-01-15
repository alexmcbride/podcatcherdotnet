using System;

namespace PodcatcherDotNet.Messages {
    public class StatusMessage {
        public string Message { get; private set; }

        public StatusMessage(string message, params object[] args)
            : this(String.Format(message, args)) { }

        public StatusMessage(string message) {
            Message = message;
        }
    }
}
