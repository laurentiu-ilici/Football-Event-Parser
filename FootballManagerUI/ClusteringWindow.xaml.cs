using System.Collections.Generic;
using System.Windows;
using VISParser;
namespace FootballManagerUI
{
    /// <summary>
    /// Interaction logic for ClusteringWindow.xaml
    /// </summary>
    public partial class ClusteringWindow : Window
    {
        public ClusteringWindow()
        {
            InitializeComponent();
            this.RawDataPath = string.Empty;
            this.outputDestination = string.Empty;
        }
        public string RawDataPath { get; set; }
        SortedList<int, VISParser.ClusterPoint> clusters = new SortedList<int, VISParser.ClusterPoint>();
        MatrixWithIds data;
        System.ComponentModel.BackgroundWorker worker = new System.ComponentModel.BackgroundWorker();
        private string outputDestination;
        private void computeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.outputDestination == string.Empty)
            {
                MessageBox.Show("Please choose a valid output file!");
                return;
            }
            if (this.RawDataPath == string.Empty)
            {
                MessageBox.Show("Please choose a raw data file!");
                return;
            }
            VISAPI.ClusterData(this.outputDestination, this.data, (bool)this.isKernel.IsChecked, 0.1F, 4);
        }
        public string OutputPath
        {
            get
            {
                return this.outputDestination;
            }
            set
            {
                this.outputDestination = value;
            }
        }
    }
}
