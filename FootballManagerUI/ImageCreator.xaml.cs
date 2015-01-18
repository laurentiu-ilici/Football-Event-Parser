using System;
using System.Windows;
using System.Windows.Controls;
using FootballProject;
using VISParser;
namespace FootballManagerUI
{
    /// <summary>
    /// Interaction logic for ImageCreator.xaml
    /// </summary>
    public partial class ImageCreator : Window
    {
        string dataPath = String.Empty;
        string outputPath = String.Empty;
        int period = 1;
        System.ComponentModel.BackgroundWorker worker = new System.ComponentModel.BackgroundWorker();
        public ImageCreator()
        {
            InitializeComponent();
        }
        private void createBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.dataPath == string.Empty)
            {
                MessageBox.Show(" The input file was not selected! Please select a valid input file!");
                return;
            }
            if (this.outputPath == string.Empty)
            {
                MessageBox.Show("The output folder was not selected or has been reset! Please select an output folder!");
                return;
            }
            string outputDir = this.outputPath + ((bool)this.secondHalfRBtn.IsChecked ? FootballProject.FootballProject.SecondHalf : FootballProject.FootballProject.FirstHalf);
            string[] files = System.IO.Directory.GetFiles(outputDir);
            try
            {
                foreach (var file in files)
                {
                    System.IO.File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            var settings = new ImageBuilderRunSettings();
            settings.Period = this.period;
            settings.IncludePossession = (bool)this.parsePossesionBtn.IsChecked;
            settings.ParallelParsing = (bool)this.parallelsBtn.IsChecked;
            settings.IncludeGraphs = (bool)this.drawGraphsBtn.IsChecked;
            settings.OutputPath = outputDir;
            settings.DataPath = this.dataPath;
            this.worker.DoWork += new System.ComponentModel.DoWorkEventHandler(buildImages);
            this.worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            this.worker.RunWorkerAsync(settings);
            this.outputPath = string.Empty;
            this.createBtn.IsEnabled = false;
        }

        void worker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            this.createBtn.IsEnabled = true;
        }
        void buildImages(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var settings = e.Argument as ImageBuilderRunSettings;
            if (settings != null)
            {
                var messageListener = new VISParser.VISAPI.Message(addMessage);
                VISParser.VISAPI.ProgressUpdate += new VISParser.VISAPI.Message(addMessage);
                VISAPI.TargetDataListLoaded += new VISAPI.ElementCount(VISAPI_TargetDataListLoaded);
                VISParser.VISAPI.BuildImages(settings.DataPath, settings.OutputPath, settings.ParallelParsing,
                                        settings.Period, settings.IncludePossession, settings.IncludeGraphs, messageListener);
            }
            else
                throw new ArgumentException("The run configuration was not set correctly");
        }

        void VISAPI_TargetDataListLoaded(int elementCount)
        {
            Dispatcher.Invoke(new VISAPI.ElementCount(setProgressBarMaximum), elementCount);
        }
        void setProgressBarMaximum(int elementCount)
        {
            this.buildProgress.Maximum = elementCount;
        }
        void addVisualMessage(string message)
        {
            this.imageList.Items.Add(message);
            this.imageList.ScrollIntoView(imageList.Items[imageList.Items.Count - 1]);
            if (message.Contains("Done building"))
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

        private void firstHalfRBtn_Checked(object sender, RoutedEventArgs e)
        {
            var btn = sender as RadioButton;
            if (btn != null && btn.Tag != null)
            {
                this.period = int.Parse(btn.Tag.ToString());
            }
        }
        public String DataPath
        {
            get { return this.dataPath; }
            set { this.dataPath = value; }
        }
        public String OutputPath
        {
            get { return this.outputPath; }
            set { this.outputPath = value; }
        }
    }
}
