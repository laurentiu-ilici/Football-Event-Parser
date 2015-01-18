using System;
using System.Globalization;
using System.Windows;

namespace FootballUI
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public sealed partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();
            var settings = new AppSettings();
            this.fadeInText.Text = settings.FadeInFames.ToString(CultureInfo.InvariantCulture);
            this.maxBallHeightText.Text = settings.MaxBallHeight.ToString(CultureInfo.InvariantCulture);
            this.maxDistanceText.Text = settings.MaxPlayerBallDistance.ToString(CultureInfo.InvariantCulture);
            this.maxAcceptedAngleText.Text = settings.MaxAcceptedAngle.ToString(CultureInfo.InvariantCulture);
            this.sleepValueText.Text = settings.ThreadSleep.ToString(CultureInfo.InvariantCulture);
            this.stationaryBallText.Text = settings.StationaryBallLimit.ToString(CultureInfo.InvariantCulture);
            MessageBox.Show("It is highly recommended that you use the default settings! Change the values at your own risk!");
        }
        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settings = new AppSettings
                {
                    StationaryBallLimit = int.Parse(stationaryBallText.Text.ToString(CultureInfo.InvariantCulture)),
                    ThreadSleep = int.Parse(sleepValueText.Text,CultureInfo.InvariantCulture),
                    MaxBallHeight = int.Parse(maxBallHeightText.Text, CultureInfo.InvariantCulture),
                    MaxAcceptedAngle = int.Parse(maxAcceptedAngleText.Text, CultureInfo.InvariantCulture),
                    MaxPlayerBallDistance = int.Parse(maxDistanceText.Text, CultureInfo.InvariantCulture),
                    FadeInFames = int.Parse(fadeInText.Text, CultureInfo.InvariantCulture)
                };
                MainWindow.SleepValue = settings.ThreadSleep;
                MainWindow.FadeEffectFrames = settings.FadeInFames;
                settings.Save();
                MessageBox.Show("Settings saved!");
                this.Close();
            }
            catch (FormatException)
            {
                MessageBox.Show("Please insert valid values for the settings! ");
            }
        }

    }
}
