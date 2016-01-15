using System;

namespace PodcatcherDotNet.Messages {
  public  class NotifyBalloonMessage {
      public string Title { get; private set; }
      public string Message { get; private set; }

      public NotifyBalloonMessage(string title, string message) {
          Title = title;
          Message = message;
      }
    }
}
