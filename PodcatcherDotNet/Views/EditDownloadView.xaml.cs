using System.Windows;
using System.Windows.Controls;

namespace PodcatcherDotNet.Views {
    public partial class EditDownloadView : UserControl {
        public EditDownloadView() {
            InitializeComponent();
        }

        private void Title_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Title.IsVisible) {
                Title.Focus();
            }
        }
    }
}
