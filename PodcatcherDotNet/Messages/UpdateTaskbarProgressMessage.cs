
namespace PodcatcherDotNet.Messages {
    public class UpdateTaskbarProgressMessage {
        public double Percent { get; private set; }
        public bool IsRunning { get; private set; }

        public UpdateTaskbarProgressMessage(bool isRunning) : this(isRunning, 0) { }

        public UpdateTaskbarProgressMessage(bool isRunning, double percent) {
            Percent = percent;
            IsRunning = isRunning;
        }
    }
}
