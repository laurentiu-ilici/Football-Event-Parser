using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VISParser;
using VISParser.FieldObjects;

namespace FootballUI
{
    /// <summary>
    /// Interaction logic for PlayerControl.xaml
    /// </summary>
    public sealed partial class PlayerControl : UserControl
    {
        public static int FieldYAxisOrigin;
        static ColorScheme PlayerColors = new ColorScheme();
        private Player playerInfo;
        public delegate void PlayerMessage(object sender, string message);
        public Polygon VoronoiPolygon { private set; get; }
        public PlayerControl(Player playerInfo, List<System.Drawing.PointF> voronoiPolygon)
        {
            InitializeComponent();
            this.playerInfo = playerInfo;
            this.playerID.Inlines.Add(new Bold(new Run(this.playerInfo.ItemId.ToString())));
            this.RefreshColor();
            this.VoronoiPolygon = new Polygon
            {
                Stroke = Brushes.Black,
                Fill = new SolidColorBrush(Color.FromArgb(playerInfo.ItemColor.A,
                                                playerInfo.ItemColor.R,
                                                    playerInfo.ItemColor.G,
                                                        playerInfo.ItemColor.B)),
                StrokeThickness = 2,
                Opacity = 0.5
            };
            
            this.RefreshPolygon(voronoiPolygon);
        }

        private void playerBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.CurrentProject.FootballEventManager.TargetPlayer = this.playerInfo;
            var parentField = this.Tag as FootballField;
            parentField.ChangeSelectedPlayer(this);
            parentField.clearEvents();
            this.playerBorder.BorderBrush = new SolidColorBrush(Colors.Red);
            MainWindow.CurrentProject.FootballEventManager.ActingPlayer = this.playerInfo;
            MainWindow.EventParseCode = EventParseCodes.ParsePassesByPlayerId;
            updateMainScreen();
        }
        private void passesToPlayer_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.EventParseCode = EventParseCodes.ParsePlayerToPlayerPasses;
            MainWindow.CurrentProject.FootballEventManager.ActingPlayer = this.playerInfo;
            this.playerBorder.BorderBrush = new SolidColorBrush(Colors.Brown);

        }
        public void RefreshColor()
        {
            if (this.playerInfo.ItemId < 12)
                this.playerBorder.BorderBrush = new SolidColorBrush(Colors.DarkGray);
            else
                this.playerBorder.BorderBrush = new SolidColorBrush(Colors.DarkGray);
        }
        private void passesToSAR_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.CurrentProject.FootballEventManager.ActingPlayer = this.playerInfo;
            MainWindow.EventParseCode = EventParseCodes.ParsePlayerToSRAPasses;
            var field = this.Tag as FootballField;


        }
        private void passesLength_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.CurrentProject.FootballEventManager.ActingPlayer = this.playerInfo;
            var window = new PassLengthWindow();
            window.ShowDialog();
            this.updateMainScreen();
        }

        private void updateMainScreen()
        {
            var field = this.Tag as FootballField;
            var window = field.Tag as MainWindow;
            window.ForceRefresh();
        }

        private void playerID_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                this.playerID.ContextMenu.PlacementTarget = this;
                this.playerID.ContextMenu.IsOpen = true;
                this.playerBorder.BorderBrush = new SolidColorBrush(Colors.Brown);
            }
        }

        private void changeRole_OnClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            if (item != null)
            {
                if (item.Tag.ToString() == "attacker")
                    this.playerInfo.Role = PlayerRoles.Attacker;
                if (item.Tag.ToString() == "defender")
                    this.playerInfo.Role = PlayerRoles.Defender;
                if (item.Tag.ToString() == "midfielder")
                    this.playerInfo.Role = PlayerRoles.Midfielder;
                if (item.Tag.ToString() == "goalKeeper")
                    this.playerInfo.Role = PlayerRoles.GoalKeeper;
            }
        }
        public void RefreshPolygon(List<System.Drawing.PointF> newPolygon)
        {

            this.VoronoiPolygon.Points.Clear();
            foreach (var point in newPolygon)
            {
                int length = (int)Math.Abs(PlayerControl.FieldYAxisOrigin - point.Y);
                int newY =(int) (point.Y > PlayerControl.FieldYAxisOrigin ? point.Y - 2 * length : point.Y + 2 * length);
                this.VoronoiPolygon.Points.Add(new Point(point.X, newY));
            }
        }
        public void RefreshPolygonColor()
        {
            this.VoronoiPolygon.Fill = new SolidColorBrush(Color.FromArgb(playerInfo.ItemColor.A,
                                                playerInfo.ItemColor.R,
                                                    playerInfo.ItemColor.G,
                                                        playerInfo.ItemColor.B));
        }
        public Teams GetTeam
        {
            get
            {
                return this.playerInfo.Team;
            }
        }
    }
}
