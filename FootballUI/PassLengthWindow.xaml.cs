using System.Windows;

namespace FootballUI
{
    /// <summary>
    /// Interaction logic for Pass.xaml
    /// </summary>
    public sealed partial  class PassLengthWindow : Window
    {
        
        public PassLengthWindow()
        {
            InitializeComponent();
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                double minDistance = double.Parse(this.minPassText.Text);
                double maxDistance = double.Parse(this.minPassText.Text);
                if (minDistance > maxDistance)
                {
                    MessageBox.Show("The minimum distance must be lower than the maximum distance");
                    return;
                }
                MainWindow.EventParseCode = EventParseCodes.ParsePassesByLength;
                MainWindow.MinPassLength = minDistance;
                MainWindow.MaxPassLength = maxDistance;
                this.Close();
            }
            catch
            {
                MessageBox.Show("Please insert valid numbers for the minimum and maximum disntace!");
            }
        }

    }
}
