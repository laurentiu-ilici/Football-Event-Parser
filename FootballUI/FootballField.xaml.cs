using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VISParser;
using VISParser.FieldObjects;

namespace FootballUI
{
    public enum DrawMode
    {
        SimpleEvent,
        ComplexEvent
    }
    /// <summary>
    /// Interaction logic for FootballField.xaml
    /// </summary>
    public sealed partial class FootballField : UserControl
    {
        Point origin;
        Rectangle currentRect = null;
        Rectangle previousRect = null;
        public RectangleStruct FirstSRA { private set; get; }
        public RectangleStruct SecondSRA { private set; get; }
        private PlayerControl selectedPlayerControl;
        private Dictionary<int, PlayerControl> playerControlDict = new Dictionary<int, PlayerControl>();
        private Ellipse ball;
        public IList<FootballProject.VisualFootballEvent> VisualEvents { get; set; }
        List<Line> complexEventLines = new List<Line>();
        Point currentRectStartPoint;
        VISParser.Parsers.GraphExtractor graphExtractor = new VISParser.Parsers.GraphExtractor();
        VISParser.Parsers.PolygonExtractor polygonExtractor = new VISParser.Parsers.PolygonExtractor();
        private bool areVisualEventsAdded = false;
        VISParser.FieldObjects.Frame currentConfiguration = null;
        private DrawMode drawMode;

        public FootballField()
        {
            InitializeComponent();
            var bla = new AppSettings();
            this.FirstSRA = new RectangleStruct();
            this.SecondSRA = new RectangleStruct();

        }
        private double relativePosX(double xCoord)
        {
            if (xCoord < -1)
                xCoord = -1;
            if (xCoord > 1)
                xCoord = 1;
            var newCoord = (int)(this.origin.X * (1d + xCoord));
            return newCoord;

        }
        private double relativePosY(double yCoord)
        {
            if (yCoord < -1)
                yCoord = -1;
            if (yCoord > 1)
                yCoord = 1;
            int newCoord = (int)(this.origin.Y * ((1d - yCoord)));
            return newCoord;
        }
        private void addPlayer(Player player, List<System.Drawing.PointF> voronoiPolygon, double opacity)
        {



            var playerRect = new PlayerControl(player, voronoiPolygon);
            this.field.Children.Add(playerRect.VoronoiPolygon);
            this.field.Children.Add(playerRect);
            playerRect.SetValue(Canvas.LeftProperty, this.relativePosX(player.XCoord) - playerRect.ActualWidth / 2);
            playerRect.SetValue(Canvas.TopProperty, this.relativePosY(player.YCoord) - playerRect.ActualHeight / 2);
            playerRect.Opacity = opacity;
            playerRect.Tag = this;
            this.playerControlDict.Add(player.ItemId, playerRect);

        }
        //To DO: probably Will be deleted
        /*
        private void addReferee(Referee referee)
        {
            Brush brush;
            Rectangle dotRect; 
            brush = new SolidColorBrush(Colors.Beige);
            dotRect = new Rectangle();
            dotRect.Height = 1;
            dotRect.Width = 1;
            dotRect.Fill = brush;
            this.field.Children.Add(dotRect);
            dotRect.SetValue(Canvas.LeftProperty, this.relativePosX(referee.XCoord));
            dotRect.SetValue(Canvas.TopProperty, this.relativePosY(referee.YCoord));
        }*/
        private void addBall(Ball ball, double opacity)
        {
            Brush brush = new SolidColorBrush(Colors.Red);
            var dotRect = new Ellipse { Height = 15, Width = 15, Opacity = opacity, Fill = brush };
            this.field.Children.Add(dotRect);
            dotRect.SetValue(Canvas.LeftProperty, this.relativePosX(ball.XCoord));
            dotRect.SetValue(Canvas.TopProperty, this.relativePosY(ball.YCoord));
            this.ball = dotRect;
        }
        public void RefreshFrame(VISParser.FieldObjects.Frame frame, double opacity)
        {
            this.currentConfiguration = frame;
            Dictionary<int, List<System.Drawing.PointF>> polygons = null;
            if (this.drawMode == FootballUI.DrawMode.SimpleEvent)
            {
                try
                {
                    polygons = this.polygonExtractor.ExtractPolygon(this.graphExtractor.BuildVorGraph(this.currentConfiguration));
                }
                catch
                {

                }
            }
            foreach (var item in frame.Objects)
            {
                if (item is Player)
                {

                    if (this.playerControlDict.ContainsKey(item.ItemId))
                    {
                        PlayerControl currentPlayer = this.playerControlDict[item.ItemId];
                        currentPlayer.SetValue(Canvas.LeftProperty, this.relativePosX(item.XCoord) - currentPlayer.ActualWidth / 2);
                        currentPlayer.SetValue(Canvas.TopProperty, this.relativePosY(item.YCoord) - currentPlayer.ActualHeight / 2);
                        currentPlayer.Opacity = opacity;
                        if (polygons != null && polygons.ContainsKey(item.ItemId))
                            currentPlayer.RefreshPolygon(polygons[item.ItemId]);

                    }
                    else
                    {
                        if (polygons != null && polygons.ContainsKey(item.ItemId))
                            this.addPlayer(item as Player, polygons[item.ItemId], opacity);
                    }
                }
                else if (item is Referee)
                {
                    continue;
                }
                else
                {
                    if (this.ball != null)
                    {
                        this.ball.SetValue(Canvas.LeftProperty, this.relativePosX(item.XCoord));
                        this.ball.SetValue(Canvas.TopProperty, this.relativePosY(item.YCoord));
                        this.ball.Opacity = opacity;
                    }
                    else
                        this.addBall(item as Ball, opacity);
                }
            }
            if (this.VisualEvents != null && this.areVisualEventsAdded == false)
            {
                this.addEvents();
            }
        }
        private void field_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.currentRect != null)
            {
                //Set the SRA such that it can
                //be parsed with the football data.
                //You need to do a transformation in the coordinate system.
                var nortWestX = (float)Canvas.GetLeft(this.currentRect);
                var nortWestY = (float)Canvas.GetTop(this.currentRect);
                float southEastX = nortWestX + (float)this.currentRect.Width;
                float southEastY = nortWestY + (float)this.currentRect.Height;
                nortWestX = 2 * nortWestX / (float)this.field.ActualWidth - 1;
                southEastX = 2 * southEastX / (float)this.field.ActualWidth - 1;
                southEastY = -(2 * southEastY / (float)this.field.ActualHeight - 1);
                nortWestY = -(2 * nortWestY / (float)this.field.ActualHeight - 1);
                var newRect = new RectangleStruct(new System.Drawing.PointF(nortWestX, nortWestY),
                                                new System.Drawing.PointF(southEastX, southEastY));
                //If the rectangle is NaN, don't consider it a SRA (there's a drawing bug if we
                //don't check for this, i.e. every click will be considered a SRA) 
                if (float.IsNaN(newRect.Area) || newRect.Area == 0)
                {
                    this.field.Children.Remove(this.currentRect);
                    this.currentRect = null;
                    return;
                }
                if (!this.FirstSRA.IsSet)
                {
                    this.FirstSRA = newRect;
                    this.previousRect = this.currentRect;
                    this.currentRect = null;
                    if (this.selectedPlayerControl != null)
                    {
                        var window = this.Tag as MainWindow;
                        MainWindow.EventParseCode = EventParseCodes.ParsePlayerToSRAPasses;
                        window.ForceRefresh();
                    }
                }
                else
                {
                    this.SecondSRA = newRect;
                    var window = this.Tag as MainWindow;
                    this.currentRect.Stroke = new SolidColorBrush(Colors.Red);
                    MainWindow.EventParseCode = EventParseCodes.ParseSRAToSRAPasses;
                    window.ForceRefresh();

                }
            }
            //this.currentRect = null;
        }
        private void field_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //  this.field.Children.Clear();
            this.origin = new Point(this.field.ActualWidth / 2, this.field.ActualHeight / 2);

            PlayerControl.FieldYAxisOrigin = (int)this.origin.Y;
            if (this.currentConfiguration != null)
            {
                this.RefreshFrame(this.currentConfiguration, 1);
            }
        }

        private void field_LeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.currentRectStartPoint = e.GetPosition(this.field);
            this.currentRect = new Rectangle
            {
                Stroke = Brushes.LightBlue,
                StrokeThickness = 2
            };
            Canvas.SetTop(this.currentRect, this.currentRectStartPoint.X);
            Canvas.SetLeft(this.currentRect, this.currentRectStartPoint.Y);
            field.Children.Add(this.currentRect);
        }
        private void field_LeftMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released || this.currentRect == null)
                return;
            var pos = e.GetPosition(this.field);
            var x = Math.Min(pos.X, this.currentRectStartPoint.X);
            var y = Math.Min(pos.Y, this.currentRectStartPoint.Y);
            var w = Math.Max(pos.X, this.currentRectStartPoint.X) - x;
            var h = Math.Max(pos.Y, this.currentRectStartPoint.Y) - y;
            this.currentRect.Width = w;
            this.currentRect.Height = h;
            Canvas.SetLeft(this.currentRect, x);
            Canvas.SetTop(this.currentRect, y);
        }
        public void RefreshPlayerColors()
        {
            foreach (var player in this.playerControlDict.Values)
            {
                player.RefreshColor();
            }
        }
        public void ResetSRAs()
        {
            if (this.currentRect != null)
                this.field.Children.Remove(currentRect);
            this.FirstSRA = new RectangleStruct();
            this.SecondSRA = new RectangleStruct();
            this.previousRect = null;
            this.currentRect = null;

        }
        private void addEvents()
        {
            foreach (var point in this.VisualEvents)
            {
                //An ellipse for the starting point of the event
                var ellipse = new Ellipse { Width = 10, Height = 10, Fill = point.PossesionColor, Opacity = 0.5 };
                ellipse.SetValue(Canvas.LeftProperty, this.relativePosX(point.Event.StartFrame.GetBall().XCoord));
                ellipse.SetValue(Canvas.TopProperty, this.relativePosY(point.Event.StartFrame.GetBall().YCoord));
                this.field.Children.Add(ellipse);
                ellipse.MouseLeftButtonUp += new MouseButtonEventHandler(event_MouseLeftButtonUp);
                ellipse.Tag = point.EventId;
                //A line between the starting and the stoping point...
                var eventPath = new Line
                {
                    X1 = this.relativePosX(point.Event.StartFrame.GetBall().XCoord) + 5,
                    Y1 = this.relativePosY(point.Event.StartFrame.GetBall().YCoord) + 5,
                    X2 = this.relativePosX(point.Event.EndFrame.GetBall().XCoord),
                    Y2 = this.relativePosY(point.Event.EndFrame.GetBall().YCoord),
                    Stroke = new SolidColorBrush(Colors.Black),
                    Opacity = 0.3,
                    StrokeThickness = 3,
                    Tag = point.EventId
                };
                eventPath.MouseLeftButtonUp += new MouseButtonEventHandler(event_MouseLeftButtonUp);
                this.field.Children.Add(eventPath);
            }
            this.areVisualEventsAdded = true;
        }
        void event_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var eventControl = sender as Shape;
            if (eventControl != null)
            {
                var window = this.Tag as MainWindow;
                if (window != null)
                {
                    window.SelectEvent(int.Parse(eventControl.Tag.ToString()));
                    this.clearEvents();
                }
            }
        }
        public void ChangeSelectedPlayer(PlayerControl playerControl)
        {
            if (this.selectedPlayerControl != null)
                this.selectedPlayerControl.RefreshColor();
            this.selectedPlayerControl = playerControl;
        }
        private void clearEvents_Click(object sender, RoutedEventArgs e)
        {
            this.clearEvents();
        }
        internal void clearEvents(bool clearSRA = true)
        {
            if (clearSRA)
            {
                this.FirstSRA = new RectangleStruct();
                this.SecondSRA = new RectangleStruct();
                this.field.Children.Clear();
            }
            else
            {
                UIElement firstSRA = null;
                UIElement secondSRA = null;
                foreach (UIElement control in this.field.Children)
                {
                    if (control is Rectangle)
                    {
                        if (firstSRA == null)
                        {
                            firstSRA = control;
                        }
                        else
                            secondSRA = control;
                    }
                }
                this.field.Children.Clear();
                if (firstSRA != null)
                    this.field.Children.Add(firstSRA);
                if (secondSRA != null)
                    this.field.Children.Add(secondSRA);
            }
            this.VisualEvents = null;
            this.ball = null;
            this.playerControlDict.Clear();
            this.areVisualEventsAdded = false;
            if (this.currentConfiguration != null)
                this.RefreshFrame(this.currentConfiguration, 1);
        }
        public void HighlightPlayers(IDictionary<int, Player> players)
        {
            foreach (var item in this.playerControlDict)
            {
                if (players.ContainsKey(item.Key))
                    item.Value.VoronoiPolygon.Fill = Brushes.Orange;
                else
                    item.Value.RefreshPolygonColor();
            }
        }
        public void UnhighlightPlayers()
        {
            foreach (var item in this.playerControlDict)
            {
                item.Value.VoronoiPolygon.Opacity = 0.5;
            }
        }

        public DrawMode DrawMode
        {
            get
            {
                return this.drawMode;
            }
            set
            {
                if (value == FootballUI.DrawMode.SimpleEvent)
                    this.clearLines();
                this.drawMode = value;
            }
        }

        private void clearLines()
        {
            foreach (var line in this.complexEventLines)
            {
                this.field.Children.Remove(line);
            }
            this.complexEventLines.Clear();
        }


        //TODO: replace this idiotic name for the procedure and refactor the procedure itself.
        public void GenerateComplexEventStaticPicture(IList<VISParser.FieldObjects.Frame> data, FootballProject.VisualComplexEvent drawnEvent)
        {
            this.clearLines();
            foreach (var player in this.playerControlDict)
            {
                player.Value.VoronoiPolygon.Points.Clear();
                player.Value.playerBorder.BorderBrush = player.Value.GetTeam == Teams.Away ? Brushes.Blue : Brushes.Green;
                player.Value.Opacity = drawnEvent.ComplexEvent.ParticipatingPlayers.ContainsKey(player.Key) ? 1d : 0.3d;
            }
            var frameDictionary = new Dictionary<int, VISParser.FieldObjects.Frame>();
            foreach (var item in data)
                frameDictionary.Add(item.FrameNumber, item);
            var enumerator = drawnEvent.ComplexEvent.ParticipatingPlayers.Values.GetEnumerator();
            enumerator.MoveNext();
            Teams currentTeam = enumerator.Current.Team;
            int colorCounter = currentTeam == Teams.Away ? 12 : 1;
            int maxColor = colorCounter == 1 ? 11 : 22;
            ColorCode colors = new ColorCode();
            foreach (var item in drawnEvent.ComplexEvent.Events.Values)
            {
                if (colorCounter > maxColor)
                    colorCounter = maxColor == 11 ? 1 : 12;
                SolidColorBrush brush = new SolidColorBrush(colors.GetColor(colorCounter));
                int lastIndexThatWorked = -1;
                for (int index = item.StartFrame.FrameNumber; index < item.EndFrame.FrameNumber; index++)
                {
                    if (frameDictionary.ContainsKey(index))
                    {
                        if (lastIndexThatWorked == -1)
                        {
                            lastIndexThatWorked = index;
                            continue;
                        }
                        VISParser.FieldObjects.Frame currentFrame = frameDictionary[index];
                        VISParser.FieldObjects.Frame lastFrame = frameDictionary[lastIndexThatWorked];
                        foreach (var player in drawnEvent.ComplexEvent.ParticipatingPlayers)
                        {
                            Player newPosition = currentFrame.GetPlayerById(player.Key);
                            Player lastPosition = lastFrame.GetPlayerById(player.Key);
                            var line = new Line
                            {
                                X1 = this.relativePosX(lastPosition.XCoord),
                                Y1 = this.relativePosY(lastPosition.YCoord),
                                X2 = this.relativePosX(newPosition.XCoord),
                                Y2 = this.relativePosY(newPosition.YCoord),
                                Stroke = brush,
                                StrokeThickness = 2

                            };
                            this.field.Children.Add(line);
                            this.complexEventLines.Add(line);
                        }
                        Ball lastBallPosition = lastFrame.GetBall();
                        Ball newBallPosition = currentFrame.GetBall();
                        var ballLine = new Line
                        {
                                X1 = this.relativePosX(lastBallPosition.XCoord),
                                Y1 = this.relativePosY(lastBallPosition.YCoord),
                                X2 = this.relativePosX(newBallPosition.XCoord),
                                Y2 = this.relativePosY(newBallPosition.YCoord),
                                Stroke = Brushes.Red,
                                StrokeThickness = 2,
                                Opacity = 0.5
                        };
                        this.field.Children.Add(ballLine);
                        this.complexEventLines.Add(ballLine);
                        lastIndexThatWorked = index;
                    }
                }
                colorCounter++;
            }
        }


    }

}
