using PodcatcherDotNet.Helpers;
using System.Windows.Controls;

namespace PodcatcherDotNet.Views {
    public partial class DownloadFolderView : UserControl {
        public DownloadFolderView() {
            InitializeComponent();

            TextHelper.Initialize(DownloadFolderListView.FontFamily, DownloadFolderListView.FontSize);
        }
    }
}
