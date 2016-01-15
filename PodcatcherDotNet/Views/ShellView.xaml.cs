using Caliburn.Micro;
using PodcatcherDotNet.Messages;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;
using System.Windows.Shell;
using System.Diagnostics;

namespace PodcatcherDotNet.Views {
    public partial class ShellView : Window, IHandle<NotifyBalloonMessage>, IHandle<UpdateTaskbarProgressMessage>, IHandle<ActivateShellViewMessage> {
        private IEventAggregator _eventAggregator;

        public ShellView() {
            InitializeComponent();

            _eventAggregator = IoC.Get<IEventAggregator>();
            _eventAggregator.Subscribe(this);

            TaskbarItemInfo = new TaskbarItemInfo();
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
        }

        public void Handle(NotifyBalloonMessage message) {
            NotifyIcon.ShowBalloonTip(message.Title, message.Message, BalloonIcon.Info);
        }

        public void Handle(UpdateTaskbarProgressMessage message) {
            if (message.IsRunning) {
                if (TaskbarItemInfo.ProgressState != TaskbarItemProgressState.Normal) {
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                }

                TaskbarItemInfo.ProgressValue = message.Percent / 100.0;
            }
            else {
                if (TaskbarItemInfo.ProgressState != TaskbarItemProgressState.None) {
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                }
            }
        }

        public void Handle(ActivateShellViewMessage message) {
            WindowState = WindowState.Normal;
            Activate();

            Debug.WriteLine("rest");
        }
    }
}
