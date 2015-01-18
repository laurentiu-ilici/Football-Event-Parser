using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using VISParser;
using FootballProject;
using MachineLearning.KernelMethods;
using VISParser.FieldObjects;

namespace FootballManagerUI
{
    /// <summary>
    /// Interaction logic for KernelComputer.xaml
    /// </summary>
    public partial class KernelComputerWindow : Window
    {
        public string RawDataPath {get;set;}
        string outputDestination = string.Empty;
        System.ComponentModel.BackgroundWorker worker = new System.ComponentModel.BackgroundWorker();
        public KernelComputerWindow()
        {
            InitializeComponent();
            this.RawDataPath = string.Empty;
        }

   
        private void kernelBtn_Checked(object sender, RoutedEventArgs e)
        {
            var btn = sender as RadioButton;
            if (btn != null)
            {
                computeBtn.IsEnabled = true;
                if (btn.Tag.ToString() == "rbf")
                {
                    gammaValue.IsEnabled = (bool)btn.IsChecked;
                    onGraphsBtn.IsEnabled = !(bool)btn.IsChecked;
                    onQueryPointsBtn.IsEnabled = !(bool)btn.IsChecked;
                    onGraphsBtn.IsChecked = false;
                    onQueryPointsBtn.IsChecked = false;

                }
                if (btn.Tag.ToString() == "tanimoto")
                {
                    gammaValue.IsEnabled = !(bool)btn.IsChecked;
                    onGraphsBtn.IsEnabled = (bool)btn.IsChecked;
                    onQueryPointsBtn.IsEnabled = (bool)btn.IsChecked;
                    onGraphsBtn.IsChecked = true;
                }

            }
        }

        private void computeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.RawDataPath == string.Empty)
            {
                MessageBox.Show("Please choose a raw data file!");
                return;
            }
            if (this.outputDestination == string.Empty)
            {
                MessageBox.Show("Please choose a valid output file!");
                return;
            }
        
            if (this.tanimotoBtn.IsChecked != true && this.rbfKernelBtn.IsChecked != true)
            {
                MessageBox.Show("Please choose a valid Kernel!");
                return;
            }
            this.worker = new System.ComponentModel.BackgroundWorker();
            if ((bool)this.tanimotoBtn.IsChecked)
            {
                var settings = new TanimotoKernelRunSettings();
                settings.DataPath = this.RawDataPath;
                settings.OutputPath = this.outputDestination;
                settings.Period = (bool)this.firstHalf.IsChecked ? 1 : 2;
                if ((bool)onGraphsBtn.IsChecked)
                {
                    settings.NegativeEdgesOnly = (bool)onGraphsBtn.IsChecked;
                    settings.OnGraphs = true;
                }
                else
                {
                    settings.QueryPointNumber = 1024;
                    settings.OnGraphs = false;
                    settings.Team = Teams.Home;
                }
                worker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
                worker.RunWorkerAsync(settings);

            }
            else if ((bool)this.rbfKernelBtn.IsChecked)
            {
                var settings = new RBFKernelRunSettings();
                settings.DataPath = this.RawDataPath;
                settings.OutputPath = this.outputDestination;
                settings.Period = (bool)this.firstHalf.IsChecked ? 1 : 2;
                try
                {
                    settings.Gamma = double.Parse(this.gammaValue.Text.ToString());

                }
                catch
                {
                    MessageBox.Show("Please insert a valid floating point value for gamma!");
                    return;
                }
                worker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerAsync(settings);
            }
            this.outputDestination = string.Empty;
            this.computeBtn.IsEnabled = false;
        }

        void worker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            this.computeBtn.IsEnabled = true;
        }

        void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            VISAPI.ProgressUpdate += new VISAPI.Message(addMessage);
            VISAPI.TargetDataListLoaded +=new VISAPI.ElementCount(VISAPI_TargetDataListLoaded);
            if (e.Argument is RBFKernelRunSettings)
            {
                
                var listener = new KernelComputer<List<double[]>>.Message(addMessage);
                var settings = e.Argument as RBFKernelRunSettings;
                VISAPI.CalculateRBFKernelMatrix(settings.DataPath, settings.OutputPath, settings.Gamma, settings.Period,listener);
            }
            else if (e.Argument is TanimotoKernelRunSettings)
            {
                var listener = new KernelComputer<ulong[]>.Message(addMessage);
                var settings = e.Argument as TanimotoKernelRunSettings;
                VISAPI.TargetDataListLoaded += new VISAPI.ElementCount(VISAPI_TargetDataListLoaded);
                if (settings.OnGraphs)
                {
                    VISAPI.CalculateTanimotoKernelMatrixOnGraphs(settings.DataPath, settings.OutputPath, settings.Period, settings.NegativeEdgesOnly, listener);
                }
                else
                {
                    VISAPI.CalculateTanimotoKernelMatrix(settings.DataPath, settings.OutputPath, settings.Period, settings.Team, settings.QueryPointNumber,listener);
                }
            }
        }
        void addVisualMessage(string message)
        {
            this.statusListBox.Items.Add(message);
            this.statusListBox.ScrollIntoView(statusListBox.Items[statusListBox.Items.Count - 1]);
            if (message.Contains("building row"))
            {
                this.buildProgress.Value += 1;
                this.progressText.Text = Math.Floor(((this.buildProgress.Value - this.buildProgress.Minimum) /
                                        (this.buildProgress.Maximum - this.buildProgress.Minimum) * 100)).ToString() + "%";
            }
        }
        void addMessage(string message)
        {
            Dispatcher.Invoke(new VISParser.VISAPI.Message(addVisualMessage), message);
        }

        void VISAPI_TargetDataListLoaded(int elementCount)
        {
            Dispatcher.Invoke(new VISAPI.ElementCount(setProgressBarMaximum), elementCount);
        }
        void setProgressBarMaximum(int elementCount)
        {
            this.buildProgress.Maximum = elementCount;
        }
        private void onGraphsBtn_Checked(object sender, RoutedEventArgs e)
        {
            var btn = sender as RadioButton;
            if (btn != null)
            {
                if (this.allEdgesBtn != null)
                {
                    allEdgesBtn.IsChecked = false;
                    negativeEdgesBtn.IsChecked = true;
                    allEdgesBtn.IsEnabled = true;
                    negativeEdgesBtn.IsEnabled = true;
                }

            }
        }

        private void onGraphsBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            var btn = sender as RadioButton;
            if (btn != null)
            {
                if (this.allEdgesBtn != null)
                {
                    allEdgesBtn.IsChecked = false;
                    negativeEdgesBtn.IsChecked = false;
                    allEdgesBtn.IsEnabled = false;
                    negativeEdgesBtn.IsEnabled = false;
                }
            }

        }
        public string OutputDataFolder
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
