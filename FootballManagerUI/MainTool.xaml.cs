using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;

namespace FootballManagerUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MiniMap miniMap;
        bool isPaused = true;
        PrincipalComponentPlotter plotterAxis1;
        PrincipalComponentPlotter plotterAxis2;
        FootballProject.FootballProject currentProject = null;
        delegate void Repaint(object sender, RoutedEventArgs e);
        private event Repaint repaint;
        const int SleepValue = 100;
        System.ComponentModel.BackgroundWorker worker = new System.ComponentModel.BackgroundWorker();
        public MainWindow()
        {           
            InitializeComponent();  
        }
        void worker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            this.isPaused = true;
            this.backwardsBtn.IsEnabled = true;
            this.forwardsBtn.IsEnabled = true;   
        }
        void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (this.isPaused == false)
            {
                Dispatcher.Invoke(this.repaint,null,null);
                Thread.Sleep(MainWindow.SleepValue);
            }
        }
        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            //if (this.mapsList.Items.CurrentItem == null)
            //    return;
            string path = this.miniMap.NextFrame();
            this.plotterAxis1.NextFrame();
            this.plotterAxis2.NextFrame();
            if (path != string.Empty)
                this.loadPicture(path);
        }
        private void previousButton_Click(object sender, RoutedEventArgs e)
        {
            //if (this.mapsList.Items.CurrentItem == null)
            //    return;
            string path = this.miniMap.LastFrame();
            this.plotterAxis1.LastFrame();
            this.plotterAxis2.LastFrame();
            if (path != string.Empty)
                this.loadPicture(path);
        }
        private void loadPicture(string path)
        {
            if(System.IO.File.Exists(path))
                this.currentFieldConfig.Source = new BitmapImage(new Uri(path));
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

            var miniMap = this.minimapBorder.Child as MiniMap;
            if (miniMap != null)
            {
                var button = sender as RadioButton;
                string[] split = button.Tag.ToString().Split(' ');
                int newMainAxis = int.Parse(split[0]);
                int newSecondAxis = int.Parse(split[1]);
                miniMap.MainAxis = newMainAxis;
                miniMap.SecondAxis = newSecondAxis;
                this.plotterAxis1.AxisToPlot = newMainAxis;
                this.plotterAxis2.AxisToPlot = newSecondAxis;
                miniMap.RefreshAxisDirections();
            }
        }
        private void play_Click(object sender, RoutedEventArgs e)
        {
            //if (this.mapsList.Items.CurrentItem == null)
            //    return;
            if (isPaused)
            {
                this.isPaused = false;
                this.forwardsBtn.IsEnabled = false;
                this.backwardsBtn.IsEnabled = false;
                this.worker.RunWorkerAsync();
                this.choice1.IsEnabled = false;
                this.choice3.IsEnabled = false;
                this.choice2.IsEnabled = false;
                this.playBtn.Background = this.Resources["Pause"] as ImageBrush;

            }
            else
            {
                this.isPaused = true;
                this.forwardsBtn.IsEnabled = true;
                this.backwardsBtn.IsEnabled = true;
                this.choice1.IsEnabled = true;
                this.choice3.IsEnabled = true;
                this.choice2.IsEnabled = true;
                this.playBtn.Background = this.Resources["Play"] as ImageBrush;
              
            }
        }
        private void open_Click(object sender, RoutedEventArgs e)
        {
            if (ClusterPoint.ImageFolderPath == string.Empty)
            {
                MessageBox.Show("Please select an image source first");
                return;
            }
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Title = "Clustering Result File";
            fileDialog.Multiselect = false;
            System.Windows.Forms.DialogResult result = fileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (fileDialog.CheckPathExists)
                    MiniMap.PointMapPath = fileDialog.FileName;
                else
                {
                    MessageBox.Show("The point map was not found!");
                    return;
                }   
                this.miniMap = new MiniMap(0, 1);
                this.minimapBorder.Child = this.miniMap;
                this.plotterAxis1 = new PrincipalComponentPlotter(0, this.miniMap.PointMap);
                this.plotterAxis2 = new PrincipalComponentPlotter(1, this.miniMap.PointMap);
                this.axisABorder.Child = this.plotterAxis1;
                this.axisBBorder.Child = this.plotterAxis2;
                this.repaint += new Repaint(nextButton_Click);
                worker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
                this.loadPicture(this.miniMap.LastFrame());
            }
        }
        private void setImages_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Image Folder",
                ShowNewFolderButton = false
            };
            var result = folderDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ClusterPoint.ImageFolderPath = folderDialog.SelectedPath;
                
                
            }
        }
        private void newProjectMenu_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderDiag = new System.Windows.Forms.FolderBrowserDialog();
            folderDiag.ShowNewFolderButton = true;
            var result = folderDiag.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;
            var messageResult = MessageBox.Show("Any data contained by the selected folder will be deleted!!! Do you wish to continue?", "Important", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel);
            if (messageResult == MessageBoxResult.Cancel)
                return;
            var fileDialog = new System.Windows.Forms.OpenFileDialog {Multiselect = false};
            result = fileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {

                messageResult = MessageBox.Show("The data will be copied to the project directory! Do you wish to continue?", "Important", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel);
                if (messageResult == MessageBoxResult.Cancel)
                    return;
            }
            try
            {
                this.currentProject = new FootballProject.FootballProject(folderDiag.SelectedPath, fileDialog.FileName,30,25,5,20);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void openProjectMenu_Click(object sender, RoutedEventArgs e)
        {
            if (currentProject != null)
            {
                var messResult = MessageBox.Show("Do you wish to save the changes to the current project?", 
                                                                    "Save current project?", 
                                                                         MessageBoxButton.YesNoCancel,
                                                                            MessageBoxImage.Question);
                if (messResult == MessageBoxResult.Yes)
                {
                    currentProject.SaveProject();
                }
            }
            this.currentProject = null;
            var folderDiag = new System.Windows.Forms.FolderBrowserDialog {ShowNewFolderButton = false};
            var folderResult = folderDiag.ShowDialog();
            if (folderResult != System.Windows.Forms.DialogResult.OK) return;
            var fileName = folderDiag.SelectedPath + FootballProject.FootballProject.ProjectFileName;
            if (!System.IO.File.Exists(fileName))
            {
                MessageBox.Show("Invalid project directory!", "Invalid project directory!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                currentProject = FootballProject.FootballProject.LoadProject(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void saveProjectMenu_Click(object sender, RoutedEventArgs e)
        {
            if(this.currentProject == null)
            {
                MessageBox.Show("No project is loaded at the moment. Aborting!","No project to save!",MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            try
            {
                this.currentProject.SaveProject();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            if (this.currentProject != null)
            {
                try
                {
                    var messageResult = MessageBox.Show("Do you wish to save the current project?", "Important", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel);
                    if (messageResult == MessageBoxResult.Yes)
                        this.currentProject.SaveProject();
                    else if (messageResult == MessageBoxResult.Cancel)
                        e.Cancel = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void buildImagesMenu_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentProject == null)
            {
                MessageBox.Show("Please open for which to create the images first!");
                return;
            }
            var window = new ImageCreator();
            window.DataPath = this.currentProject.RawDataPath;
            window.OutputPath = this.currentProject.ProjectPath + FootballProject.FootballProject.FrameFolder;
            window.ShowDialog();
        }
        private void addKernelMenu_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentProject == null)
            {
                MessageBox.Show("Please open for which to create the kernel matrix first!");
                return;
            }
            var window = new KernelComputerWindow
            {
                RawDataPath = this.currentProject.RawDataPath,
                OutputDataFolder = this.currentProject.ProjectPath + FootballProject.FootballProject.MatricesResults
            };
            window.ShowDialog();
        }
        private void clusterDataMenu_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentProject == null)
            {
                MessageBox.Show("Please open for which to create the kernel matrix first!");
                return;
            }
            var window = new ClusteringWindow
            {
                RawDataPath = this.currentProject.RawDataPath,
                OutputPath = this.currentProject.ProjectPath + FootballProject.FootballProject.MapsResults
            };
            window.ShowDialog();
        }
    }
}
