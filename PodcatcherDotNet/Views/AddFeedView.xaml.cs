using System.Windows;
using System.Windows.Controls;

namespace PodcatcherDotNet.Views {
    public partial class AddFeedView : UserControl {
        public AddFeedView() {
            InitializeComponent();

            Url.IsVisibleChanged += Url_IsVisibleChanged;
        }

        private void Url_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Url.IsVisible) {
                Url.Focus();
            }
        }
    }
}
