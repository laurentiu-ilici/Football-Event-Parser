using System.Windows;

namespace FootballManagerUI
{
    /// <summary>
    /// Interaction logic for MenuWindow.xaml
    /// </summary>
    public partial class MenuWindow : Window
    {
        public MenuWindow()
        {
            InitializeComponent();
        }

        private void buildImages_Click(object sender, RoutedEventArgs e)
        {

            var window = new ImageCreator();
            window.ShowDialog();
            
        }

        private void viewPCABtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new MainWindow();
            window.ShowDialog();
        }

        private void kernelBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new KernelComputerWindow();
            window.ShowDialog();
        }

        private void clusterBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new ClusteringWindow();
            window.ShowDialog();
        }
    }
}
