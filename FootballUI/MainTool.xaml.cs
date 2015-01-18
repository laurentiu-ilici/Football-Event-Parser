using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FootballProject;
using VISParser;
using VISParser.Events;
using Frame = VISParser.FieldObjects.Frame;
using FootballManagerUI;

namespace FootballUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public enum EventParseCodes
    {
        ParseAllEvents = 0,
        ParsePlayerToPlayerPasses = 1,
        ParsePlayerToSRAPasses = 2,
        ParsePassesByLength = 3,
        ParseSRAToSRAPasses = 4,
        ParseFilters = 5,
        ParseUnknownEvents = 6,
        ParsePassesByPlayerId = 7,
        ParseComplexQueries = 8
    }
    public sealed partial class MainWindow : Window
    {
        private MiniMap miniMap;
        public static FootballProject.FootballProject CurrentProject = null;
        public static double FadeEffectFrames = 50;
        private static readonly double OpacityStep = 0.75 / MainWindow.FadeEffectFrames;
        public static double MinPassLength { get; set; }
        public static double MaxPassLength { get; set; }
        VisualFootballEvent currentEvent;
        IList<Frame> data;
        readonly FootballField field = new FootballField();
        public static EventParseCodes EventParseCode;
        bool isPaused = true;
        public static int SleepValue = 30;
        //Sets the amount of frames to skip when increasing the match speed.
        int frameSkipAmount = 0;
        delegate void Repaint();
        private event Repaint RepaintWindow;
        private VisualFootballEvent highlightedEvent;
        readonly System.ComponentModel.BackgroundWorker worker = new System.ComponentModel.BackgroundWorker();
        public MainWindow()
        {
            InitializeComponent();
            this.fieldBorder.Child = this.field;
            var settings = new AppSettings();
            MainWindow.FadeEffectFrames = settings.FadeInFames;
            MainWindow.SleepValue = settings.ThreadSleep;
            VISAPI.ProgressUpdate += new VISAPI.Message(VISAPI_ProgressUpdate);
            //I try to set the tag as the parent for each control such that I can go through the 
            //heirarchy easier and call methods on the parents :).
            this.field.Tag = this;
        }

        void VISAPI_ProgressUpdate(string message)
        {
            this.Dispatcher.Invoke(new VISAPI.Message(updateStatusLabel), message);
        }
        private void projectNotLoadedError()
        {
            if (MainWindow.CurrentProject == null)
            {
                MessageBox.Show("Please open or create a project first!");
            }
        }
        private void calculateStartFrame()
        {
            if (this.currentEvent != null)
            {
                int startFrameValue = this.currentEvent.Event.StartFrame.FrameNumber - (int)MainWindow.FadeEffectFrames;
                startFrameValue = startFrameValue >= 0 ? startFrameValue : 0;
                this.movieSlider.Value = startFrameValue;
            }
        }
        private void refreshProjectUI()
        {
            this.data = MainWindow.CurrentProject.FootballEventManager.Data;
            this.RepaintWindow += this.updateFrameNumber;
            this.worker.DoWork += worker_DoWork;
            this.worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            this.movieSlider.Minimum = 0;
            this.movieSlider.Maximum = this.data.Count;
            this.movieSlider.Value = 1;
            MainWindow.EventParseCode = EventParseCodes.ParseAllEvents;
            this.parseEvents();
        }
        private void newProjectMenu_Click(object sender, RoutedEventArgs e)
        {
            var folderDiag = new System.Windows.Forms.FolderBrowserDialog { ShowNewFolderButton = true };
            var result = folderDiag.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                var messageResult = MessageBox.Show("Any data contained by the selected folder will be deleted!!! Do you wish to continue?", "Important", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel);
                if (messageResult == MessageBoxResult.Cancel)
                    return;
                var fileDialog = new System.Windows.Forms.OpenFileDialog { Multiselect = false };
                result = fileDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    messageResult = MessageBox.Show("The data will be copied to the project directory! Do you wish to continue?", "Important", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel);
                    if (messageResult == MessageBoxResult.Cancel)
                        return;
                }
                try
                {
                    var settings = new AppSettings();
                    MainWindow.SleepValue = settings.ThreadSleep;
                    MainWindow.FadeEffectFrames = settings.FadeInFames;
                    MainWindow.CurrentProject = new FootballProject.FootballProject(folderDiag.SelectedPath, fileDialog.FileName, settings.MaxPlayerBallDistance, settings.MaxBallHeight,
                        settings.MaxAcceptedAngle, settings.StationaryBallLimit);
                    this.refreshProjectUI();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }

        }
        private void updateStatusLabel(string message)
        {
            this.statusLabel.Content = message;
        }
        void worker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Debugger.WriteMessage("Worker Stopped");
            this.isPaused = true;
            this.backwardsBtn.IsEnabled = true;
            this.forwardsBtn.IsEnabled = true;
        }
        void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Debugger.WriteMessage("Worker Started");
            while (this.isPaused == false)
            {
                Dispatcher.Invoke(this.RepaintWindow);
                System.Threading.Thread.Sleep(MainWindow.SleepValue);
            }
        }
        private void updateFrameNumber()
        {

            if (this.movieSlider.Value + 1 + this.frameSkipAmount < this.data.Count)
            {
                this.movieSlider.Value += this.frameSkipAmount + 1;
            }
            else
            {
                this.movieSlider.Value = this.data.Count - 1;
            }
            Debugger.WriteMessage("FrameUpdated to number: " + this.movieSlider.Value.ToString(CultureInfo.InvariantCulture));
        }
        private void openProjectMenu_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CurrentProject != null)
            {
                var messResult = MessageBox.Show("Do you wish to save the changes to the current project?",
                                                                    "Save current project?",
                                                                         MessageBoxButton.YesNoCancel,
                                                                            MessageBoxImage.Question);
                if (messResult == MessageBoxResult.Yes)
                {
                    MainWindow.CurrentProject.SaveProject();
                }
            }
            MainWindow.CurrentProject = null;
            var folderDiag = new System.Windows.Forms.FolderBrowserDialog();
            folderDiag.ShowNewFolderButton = false;
            var folderResult = folderDiag.ShowDialog();
            if (folderResult == System.Windows.Forms.DialogResult.OK)
            {
                string fileName = folderDiag.SelectedPath + FootballProject.FootballProject.ProjectFileName;
                if (!System.IO.File.Exists(fileName))
                {
                    MessageBox.Show("Invalid project directory!", "Invalid project directory!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                //try
                //{
                MainWindow.CurrentProject = FootballProject.FootballProject.LoadProject(fileName);
                this.refreshProjectUI();
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show(ex.Message);
                //}
            }
        }
        private void saveProjectMenu_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CurrentProject == null)
            {
                MessageBox.Show("No project is loaded at the moment. Aborting!", "No project to save!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            try
            {
                MainWindow.CurrentProject.SaveProject();
                MessageBox.Show("Project saved!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void backwardsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CurrentProject == null)
            {
                this.projectNotLoadedError();
                return;
            }
            if (this.data != null)
            {
                if (this.isPaused)
                {
                    if (this.movieSlider.Value - 1 >= 0)
                    {

                        this.movieSlider.Value--;
                    }
                }
                else
                {
                    if (this.frameSkipAmount > 0)
                        this.frameSkipAmount--;
                }
            }
            else
            {
                MessageBox.Show("Please load the data first!");
            }
        }
        private void forwardsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CurrentProject == null)
            {
                this.projectNotLoadedError();
                return;
            }
            if (this.data != null)
            {
                if (this.isPaused)
                {
                    if (this.movieSlider.Value + 1 < this.data.Count)
                    {
                        this.movieSlider.Value++;
                    }
                }
                else
                {
                    this.frameSkipAmount++;

                }
            }
            else
            {
                MessageBox.Show("Please load the data first!");
            }
        }
        private void playBtn_Click(object sender, RoutedEventArgs e)
        {

            if (MainWindow.CurrentProject == null)
            {
                this.projectNotLoadedError();
                return;
            }
            if (this.currentEvent != null && !(currentEvent.EventId < this.movieSlider.Value && this.movieSlider.Value < currentEvent.Event.EndFrame.FrameNumber))
            {
                this.calculateStartFrame();
            }
            this.isPaused = !this.isPaused;
            this.refreshButtonStates();

        }
        private void refreshButtonStates()
        {
            if (!isPaused)
            {
                this.isPaused = false;
                this.worker.RunWorkerAsync();
                this.playBtn.Background = this.Resources["Pause"] as ImageBrush;
                this.frameSkipAmount = 0;
            }
            else
            {
                this.isPaused = true;
                this.playBtn.Background = this.Resources["Play"] as ImageBrush;
            }
        }
        private void movieSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MainWindow.CurrentProject == null && sender != this.movieSlider)
                return;
            var sb = new StringBuilder();
            sb.Append("Frame : ");
            sb.Append((int)(this.movieSlider.Value));
            sb.Append("/");
            sb.Append(this.movieSlider.Maximum.ToString());
            this.frameCounterLabel.Content = sb.ToString();
            var movieSliderValue = (int)this.movieSlider.Value;
            movieSliderValue = movieSliderValue >= 0 ? movieSliderValue : 0;
            movieSliderValue = movieSliderValue < this.data.Count ? movieSliderValue : this.data.Count - 1;
            if (this.currentEvent != null && movieSliderValue < this.currentEvent.Event.StartFrame.FrameNumber)
            {
                double opacity = 1 - (this.currentEvent.Event.StartFrame.FrameNumber - movieSliderValue) * MainWindow.OpacityStep;
                this.field.RefreshFrame(this.data[movieSliderValue], opacity);
                if (this.miniMap != null)
                    this.miniMap.TryFrame(this.data[movieSliderValue].FrameNumber);
                return;
            }
            else if (this.currentEvent != null &&
                movieSliderValue >= this.currentEvent.Event.EndFrame.FrameNumber
                && movieSliderValue < this.currentEvent.Event.EndFrame.FrameNumber + MainWindow.FadeEffectFrames)
            {
                double opacity = 1d - (movieSliderValue - this.currentEvent.Event.EndFrame.FrameNumber) * MainWindow.OpacityStep;
                this.field.RefreshFrame(this.data[movieSliderValue], opacity);
                if (this.miniMap != null)
                    this.miniMap.TryFrame(this.data[movieSliderValue].FrameNumber);
                return;
            }
            else if (this.currentEvent != null &&
                (int)this.movieSlider.Value >= this.currentEvent.Event.EndFrame.FrameNumber + MainWindow.FadeEffectFrames)
            {
                if (this.eventList.SelectedItems.Count > 1)
                {

                    int index = this.eventList.SelectedItems.IndexOf(this.currentEvent);
                    if (index + 1 < this.eventList.SelectedItems.Count)
                    {
                        var nextEvent = this.eventList.SelectedItems[index + 1] as VisualFootballEvent;
                        this.changeCurrentEvent(nextEvent);
                    }
                    else
                    {
                        this.isPaused = true;
                        this.refreshButtonStates();
                        this.changeCurrentEvent(this.eventList.SelectedItems[0] as VisualFootballEvent, false);
                    }
                }
                else
                {
                    this.isPaused = true;
                    this.refreshButtonStates();
                    this.movieSlider.Value = this.currentEvent.Event.EndFrame.FrameNumber;
                }
            }
            else
            {
                this.field.RefreshFrame(this.data[movieSliderValue], 1);
                if (this.miniMap != null)
                    this.miniMap.TryFrame(this.data[movieSliderValue].FrameNumber);
            }
            if (this.highlightedEvent != null)
            {
                if (this.highlightedEvent.Event.EndFrame.FrameNumber < movieSlider.Value || this.highlightedEvent.Event.StartFrame.FrameNumber > this.movieSlider.Value)
                {
                    this.updateHighlightedEvent(MainWindow.CurrentProject.FootballEventManager.FindCurrentDisplayedEvent((int)movieSlider.Value));
                }
            }
            else
            {
                this.updateHighlightedEvent(MainWindow.CurrentProject.FootballEventManager.FindCurrentDisplayedEvent((int)movieSlider.Value));
            }
            if (this.highlightedEvent != null && this.highlightedEvent.Event.ActingPlayer != null)
            {
                this.fieldBorder.BorderBrush = this.highlightedEvent.Event.ActingPlayer.Team == VISParser.FieldObjects.Teams.Home ? Brushes.Green : Brushes.Blue;
               
                if (this.highlightedEvent is VisualComplexEvent)
                {
                    var complexEvent = highlightedEvent as VisualComplexEvent;
                    this.field.HighlightPlayers(complexEvent.ComplexEvent.ParticipatingPlayers);
                }
                else
                {
                    this.field.UnhighlightPlayers();
                }
            }
            ////Debugging code:
            //int posId = (int)this.movieSlider.Value > 0 ? this.data[(int)this.movieSlider.Value - 1].PossessionId : -1;
            //if (posId != -1)
            //{
            //    // this.debugBlock.Text = this.data[(int)this.movieSlider.Value].GetPlayerById(posId).DebugPossesionInfo;
            //}
        }

        private void updateHighlightedEvent(VisualFootballEvent newHighlight)
        {
            ListViewItem visualContainer;
            if (this.highlightedEvent != null)
            {

                visualContainer = this.eventList.ItemContainerGenerator.ContainerFromItem(this.highlightedEvent) as ListViewItem;
                if (visualContainer != null)

                    visualContainer.Background = this.eventList.SelectedItems.Contains(this.highlightedEvent) ? Brushes.Blue : Brushes.White;

            }
            if (newHighlight != null)
            {
                visualContainer = this.eventList.ItemContainerGenerator.ContainerFromItem(newHighlight) as ListViewItem;

                if (visualContainer != null)
                    visualContainer.Background = Brushes.Red;
                this.eventList.ScrollIntoView(newHighlight);
            }
            this.highlightedEvent = newHighlight;
            if (this.highlightedEvent != null && this.highlightedEvent.Event.ActingPlayer != null)
            {
                this.fieldBorder.BorderBrush = this.highlightedEvent.Event.ActingPlayer.Team == VISParser.FieldObjects.Teams.Home ? Brushes.Green : Brushes.Blue;
                if (this.highlightedEvent is VisualComplexEvent)
                {
                    var complexEvent = highlightedEvent as VisualComplexEvent;
                    this.field.DrawMode = DrawMode.ComplexEvent;
                    this.field.GenerateComplexEventStaticPicture(this.data, complexEvent);
                    if(miniMap != null)
                        this.miniMap.DisplayComplexEvent(complexEvent.ComplexEvent.StartFrame.FrameNumber, complexEvent.ComplexEvent.EndFrame.FrameNumber);
                }
                else
                {
                    this.field.DrawMode = DrawMode.SimpleEvent;
                }
                
            }

        }
        private void updateBidingSource()
        {
            this.eventList.ItemsSource = null;
            this.eventList.Items.Clear();
            this.eventList.ItemsSource = MainWindow.CurrentProject.FootballEventManager.DisplayedEvents;
            this.eventList.UpdateLayout();
        }
        private void parseEventsButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.data.Count == 0)
            {
                this.projectNotLoadedError();
                return;
            }
            this.parseEvents();
        }
        private void parseEvents()
        {
            this.field.VisualEvents = null;
            switch (MainWindow.EventParseCode)
            {

                case EventParseCodes.ParsePlayerToPlayerPasses:
                    MainWindow.CurrentProject.FootballEventManager.FilterPlayerToPlayerPasses();
                    this.field.ResetSRAs();
                    this.changeQueryTabToCustomQuery();
                    break;
                case EventParseCodes.ParsePassesByLength:
                    MainWindow.CurrentProject.FootballEventManager.FilterPassesByLength(MainWindow.MinPassLength, MainWindow.MaxPassLength);
                    this.field.ResetSRAs();
                    this.changeQueryTabToCustomQuery();
                    break;
                case EventParseCodes.ParsePlayerToSRAPasses:
                    if (this.field.FirstSRA.IsSet)
                    {
                        MainWindow.CurrentProject.FootballEventManager.FilterPlayerToSRAPasses(this.field.FirstSRA.NorthWest.X, this.field.FirstSRA.NorthWest.Y,
                            this.field.FirstSRA.SouthEast.X, this.field.FirstSRA.SouthEast.Y);
                        this.changeQueryTabToCustomQuery();

                    }
                    else
                    {
                        MessageBox.Show("Please specify a rectangular area first!");

                    }
                    break;
                case EventParseCodes.ParseSRAToSRAPasses:
                    if (this.field.FirstSRA.IsSet && this.field.SecondSRA.IsSet)
                    {
                        MainWindow.CurrentProject.FootballEventManager.FilterSRAtoSRAPasses(this.field.FirstSRA.NorthWest.X, this.field.FirstSRA.NorthWest.Y,
                            this.field.FirstSRA.SouthEast.X, this.field.FirstSRA.SouthEast.Y, this.field.SecondSRA.NorthWest.X, this.field.SecondSRA.NorthWest.Y,
                            this.field.SecondSRA.SouthEast.X, this.field.SecondSRA.SouthEast.Y);
                        this.changeQueryTabToCustomQuery();
                    }
                    else
                    {
                        MessageBox.Show("Please specify the rectangular areas first!");

                    }
                    break;
                case EventParseCodes.ParseAllEvents:
                    MainWindow.CurrentProject.ShowAllEvents();
                    this.field.ResetSRAs();
                    break;
                case EventParseCodes.ParseFilters:
                    var filterList = new List<FootballEventTypes>();
                    //TO DO: make this less ugly
                    if ((bool)this.cornerKickCheck.IsChecked)
                        filterList.Add(FootballEventTypes.CornerKick);
                    if ((bool)this.passesCheck.IsChecked)
                        filterList.Add(FootballEventTypes.Pass);
                    if ((bool)this.possessionCheck.IsChecked)
                        filterList.Add(FootballEventTypes.SimplePossession);
                    if ((bool)this.throwinCheck.IsChecked)
                        filterList.Add(FootballEventTypes.ThrowIn);
                    if ((bool)this.othersCheck.IsChecked)
                        filterList.Add(FootballEventTypes.OtherFixture);
                    if ((bool)this.unknownCheck.IsChecked)
                        filterList.Add(FootballEventTypes.UnknownEvent);
                    if ((bool)this.shotCheck.IsChecked)
                        filterList.Add(FootballEventTypes.Shot);
                    if ((bool)this.goalKickCheck.IsChecked)
                        filterList.Add(FootballEventTypes.GoalKick);
                    if ((bool)this.offsideCheck.IsChecked)
                        filterList.Add(FootballEventTypes.Offside);
                    MainWindow.CurrentProject.FootballEventManager.ApplyFilters(filterList);
                    this.field.ResetSRAs();
                    break;
                case EventParseCodes.ParsePassesByPlayerId:
                    MainWindow.CurrentProject.FootballEventManager.FilterPassesByPlayerId();
                    this.field.ResetSRAs();
                    this.field.VisualEvents = MainWindow.CurrentProject.FootballEventManager.DisplayedEvents;
                    this.changeQueryTabToCustomQuery();
                    break;
                case EventParseCodes.ParseComplexQueries:
                    this.field.ResetSRAs();
                    MainWindow.CurrentProject.FootballEventManager.DisplayComplexEvents();
                    break;


            }
            this.updateVisuals();
        }
        private void updateVisuals()
        {
            this.updateBidingSource();
            this.field.RefreshPlayerColors();
            this.movieSlider.Ticks.Clear();
            if (MainWindow.EventParseCode != EventParseCodes.ParsePassesByPlayerId)
            {
                if (MainWindow.EventParseCode == EventParseCodes.ParseSRAToSRAPasses ||
                    MainWindow.EventParseCode == EventParseCodes.ParsePlayerToSRAPasses)
                    this.field.clearEvents(false);
                else
                    this.field.clearEvents();
            }
            this.field.RefreshFrame(this.data[(int)this.movieSlider.Value], 1);
            if (MainWindow.EventParseCode != EventParseCodes.ParseAllEvents)
            {
                foreach (var item in MainWindow.CurrentProject.FootballEventManager.DisplayedEvents)
                {
                    this.movieSlider.Ticks.Add(item.Event.StartFrame.FrameNumber);
                }
            }
         
        }
        private void changeCurrentEvent(VisualFootballEvent fEvent, bool goToFirstFrame = true)
        {
            if (fEvent != null)
            {
                if (goToFirstFrame)
                {
                    if (this.isPaused || this.allEventsTab.IsSelected)
                        this.movieSlider.Value = fEvent.Event.StartFrame.FrameNumber;
                    else
                    {
                        this.calculateStartFrame();

                    }
                }
                this.currentEvent = fEvent;
              
            }
        }
        private void eventList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var fEvent = this.eventList.SelectedItem as VisualFootballEvent;
            if (fEvent != null)
            {
                this.changeCurrentEvent(fEvent);
            }

        }
        private void showAllEvents_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CurrentProject != null)
            {
                MainWindow.EventParseCode = EventParseCodes.ParseAllEvents;
                this.field.ResetSRAs();
                this.parseEvents();
            }
        }
        public void RefreshEventList()
        {
            if (MainWindow.CurrentProject != null)
                this.parseEvents();
        }
        private void movieSlider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.eventList.SelectedItem = null;

            this.currentEvent = null;
        }
        private void deleteEvent_Click(object sender, RoutedEventArgs e)
        {
            if (this.eventList.SelectedItems.Count > 1)
            {
                MessageBox.Show("You can only delete one event at a time!");
                return;
            }
            if (this.complexQuery.IsSelected == false)
            {
                var theEvent = this.eventList.SelectedItem as VisualFootballEvent;
                if (theEvent != null)
                    MainWindow.CurrentProject.FootballEventManager.TransformToUnknown(theEvent);
                this.parseEvents();
            }


        }
        private void editEvent_Click(object sender, RoutedEventArgs e)
        {
            var window = new NewEventWindow(this.eventList.SelectedItem as VisualFootballEvent);
            window.Tag = this;
            window.Show();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }
        internal void SelectEvent(int eventId)
        {
            this.changeCurrentEvent(MainWindow.CurrentProject.FootballEventManager.FindCurrentDisplayedEvent(eventId));
        }
        internal void ForceRefresh()
        {
            if (MainWindow.CurrentProject != null)
            {
                this.parseEvents();
            }
        }
        private void resetCurrentEvent()
        {
            this.currentEvent = null;
        }
        private void queryTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainWindow.CurrentProject != null)
            {

                this.resetCurrentEvent();
                if (this.allEventsTab.IsSelected)
                {
                    MainWindow.EventParseCode = EventParseCodes.ParseAllEvents;
                    this.parseEvents();
                }
                //This will havely change...
                else if (this.customQuery.IsSelected)
                {
                    return;
                }
                else if (this.filterEventsTab.IsSelected)
                {
                    MainWindow.EventParseCode = EventParseCodes.ParseFilters;
                    this.parseEvents();
                }
                else if (this.complexQuery.IsSelected)
                {
                    MainWindow.EventParseCode = EventParseCodes.ParseComplexQueries;
                    this.parseEvents();
                }

            }
        }

        private void filtersCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.filtersCombo.SelectedIndex != 0)
            {
                this.filtersCombo.SelectedIndex = 0;
            }
        }

        private void filterCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CurrentProject != null)
            {
                MainWindow.EventParseCode = EventParseCodes.ParseFilters;
                this.parseEvents();
            }
        }
        internal void changeQueryTabToCustomQuery()
        {
            this.customQuery.IsSelected = true;
        }

        private void clearSelectedEvents_Click(object sender, RoutedEventArgs e)
        {
            this.eventList.SelectedItems.Clear();
            this.eventList.InvalidateVisual();
            this.currentEvent = null;
        }

        private void settings_Click(object sender, RoutedEventArgs e)
        {
            var window = new ConfigWindow();
            window.ShowDialog();
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj)
                 where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private ListView getSubEventList(object sender)
        {

            CheckBox box = sender as CheckBox;
            
            if (box != null)
            {
                var container = this.eventList.ContainerFromElement(box) as ListViewItem;
                ContentPresenter presenter = FindVisualChild<ContentPresenter>(container);

                DataTemplate template = presenter.ContentTemplate;
              
                if (container != null)
                {
                    var found = template.FindName("subEventlist", presenter);
                    return found as ListView;
                }
            }

            return null;
        }


        private VisualFootballEvent getBindedEvent(object sender)
        {
            var grid = VisualTreeHelper.GetParent(sender as DependencyObject);
            var container = VisualTreeHelper.GetParent(grid as DependencyObject) as ContentPresenter;
            return container.Content as VisualFootballEvent;
        }

        private void eventChecked(object sender, RoutedEventArgs e)
        {
            ListView list = this.getSubEventList(sender);

            var visualEvent = getBindedEvent(sender) as VisualComplexEvent;

            if (list != null && visualEvent != null)
            {
                list.ItemsSource = null;
                list.Items.Clear();
                list.ItemsSource = visualEvent.EventsToDisplay;
                list.UpdateLayout();
                list.Visibility = Visibility.Visible; 
            }


        }

        private void eventUnchecked(object sender, RoutedEventArgs e)
        {
            ListView list = this.getSubEventList(sender);

            if (list != null)
            {
                list.DataContext = null;
                list.Visibility = Visibility.Collapsed;
            }

        }

        private void loadClustering_Click_1(object sender, RoutedEventArgs e)
        {
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
                FootballManagerUI.ClusterPoint.ImageFolderPath = "none";
                this.miniMap = new MiniMap(0, 1,true);
                this.minimapBorder.Child = this.miniMap;
                this.miniMap.LastFrame();
            }
        }

        private void showComplexEvent_Click(object sender, RoutedEventArgs e)
        {
            if (this.highlightedEvent != null && this.highlightedEvent is VisualComplexEvent)
            {
                var theEvent = this.highlightedEvent as VisualComplexEvent;
                this.field.GenerateComplexEventStaticPicture(this.data,theEvent);
            }
        }
    }
}
