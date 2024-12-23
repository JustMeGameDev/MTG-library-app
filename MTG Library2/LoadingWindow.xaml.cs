using System.Windows;

namespace MTG_Library2
{
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            InitializeComponent();
        }

        public void UpdateProgress(double progress, string status)
        {
            ProgressBar.Value = progress;
            ProgressText.Text = status;
        }

    }
}