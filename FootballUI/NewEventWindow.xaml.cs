using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using FootballProject;
using VISParser.Events;

namespace FootballUI
{
    /// <summary>
    /// Interaction logic for NewEventWindow.xaml
    /// </summary>
    public partial class NewEventWindow : Window
    {
        private readonly VisualFootballEvent modifiedEvent;
        private readonly List<int> playerIds = new List<int>();
        int originalEventIndex;
        readonly int originalStartFrameIndex;
        readonly int originalEndFrameIndex;
        FootballEventTypes currentType;
        public NewEventWindow(FootballProject.VisualFootballEvent visEvent)
        {
            InitializeComponent();
            if (visEvent == null)
                throw new ArgumentException("Please provide an unknown event to modify!");
            this.modifiedEvent = visEvent;
            for (int index = -1; index < 23; index++)
            {
                if (index == 0) continue;
                this.playerIds.Add(index);
            }

            this.originalEventIndex = MainWindow.CurrentProject.FootballEventManager.Events.IndexOf(modifiedEvent);
            this.originalStartFrameIndex = MainWindow.CurrentProject.FootballEventManager.Data.IndexOf(modifiedEvent.Event.StartFrame);
            this.originalEndFrameIndex = MainWindow.CurrentProject.FootballEventManager.Data.IndexOf(modifiedEvent.Event.EndFrame);
            this.startFrameBox.Text = this.originalStartFrameIndex.ToString(CultureInfo.InvariantCulture);
            this.endFrameBox.Text = this.originalEndFrameIndex.ToString(CultureInfo.InvariantCulture);
            this.endFrameBox.Loaded += endFrameBox_Loaded;
            this.eventTypesBox.ItemsSource = FootballProject.FootballEventManager.EventTypes.Keys;
            this.actingPlayerBox.DataContextChanged += playerIdBoxLoaded;
            this.responsiblePlayerBox.DataContextChanged += playerIdBoxLoaded;
            this.passToBox.DataContextChanged += playerIdBoxLoaded; 
            this.passToBox.ItemsSource = this.playerIds;
            this.responsiblePlayerBox.ItemsSource = this.playerIds;
            this.actingPlayerBox.ItemsSource = this.playerIds;
           

        }

        private void playerIdBoxLoaded(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var box = sender as ComboBox;
            if (box != null)
            {
                box.SelectedItem = box.Items.IndexOf(-1);
            }
        }
        void endFrameBox_Loaded(object sender, RoutedEventArgs e)
        {
            this.eventTypesBox.SelectedItem = this.eventTypesBox.Items[this.eventTypesBox.Items.IndexOf(this.modifiedEvent.Event.EventType)];
        }
        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            int startFrameId;
            int endFrameId;
            int startFrameIndex;
            int endFrameIndex;
            try
            {
                startFrameIndex = int.Parse(this.startFrameBox.Text);
                endFrameIndex = int.Parse(this.endFrameBox.Text);
                startFrameId = MainWindow.CurrentProject.FootballEventManager.Data[startFrameIndex].FrameNumber;
                endFrameId = MainWindow.CurrentProject.FootballEventManager.Data[endFrameIndex].FrameNumber;
            }
            catch
            {
                MessageBox.Show("Please insert valid frame numbers! They must be positive integers!");
                return;
            }
            if (this.eventTypesBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please select the event type you want to define from the event drop down menu!");
                return;
            }
            if (startFrameId >= endFrameId)
            {
                MessageBox.Show("The starting frame must have a smaller value than the ending frame!");
                return;
            }
            if (this.originalStartFrameIndex > startFrameIndex ||
                this.originalEndFrameIndex < startFrameIndex)
            {
                MessageBox.Show(String.Format("The value of the starting frame must be between {0} and {1}", this.originalStartFrameIndex,
                    this.originalEndFrameIndex));
                return;
            }
            if (this.originalStartFrameIndex > endFrameIndex ||
               this.originalEndFrameIndex < endFrameIndex)
            {
                MessageBox.Show(String.Format("The value of the ending frame must be between {0} and {1}", this.originalStartFrameIndex,
                    this.originalEndFrameIndex));
                return;
            }
            if (this.actingPlayerBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please select the player performing the action from the Acting Player box!");
                return;
            }
            this.save(startFrameIndex, endFrameIndex, startFrameId, endFrameId);
            this.Close();
        }

        private void save(int startFrameIndex, int endFrameIndex, int startFrameId, int endFrameId)
        {
            
               MainWindow.CurrentProject.FootballEventManager.NewUserDefinedEvent(this.modifiedEvent,
               this.currentType,
               startFrameIndex,
               endFrameIndex,
               startFrameId,
               endFrameId,
               (int)this.actingPlayerBox.SelectedItem,
               (int)this.passToBox.SelectedItem,
               (int)this.responsiblePlayerBox.SelectedItem);
        }
        private void eventTypesBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.eventTypesBox.SelectedItem.ToString() == "Simple Possession")
            {
                this.passToBox.IsEnabled = false;
                this.responsiblePlayerBox.IsEnabled = false;
            }
            else if (this.eventTypesBox.SelectedItem.ToString() == "Pass")
            {
                this.passToBox.IsEnabled = true;
                this.responsiblePlayerBox.IsEnabled = false;
            }
            else
            {
                this.passToBox.IsEnabled = true;
                this.responsiblePlayerBox.IsEnabled = true;
            }
            this.currentType = FootballProject.FootballEventManager.EventTypes[this.eventTypesBox.SelectedItem.ToString()];
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var window = this.Tag as MainWindow;
            if (window != null)
                window.ForceRefresh();
        }

    }
}
