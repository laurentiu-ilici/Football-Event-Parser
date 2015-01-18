using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FootballManagerUI
{
    /// <summary>
    /// Interaction logic for MiniMap.xaml
    /// </summary>
    public partial class MiniMap : UserControl
    {
        public static string PointMapPath = string.Empty;
        SortedList<int, ClusterPoint> pointMap = new SortedList<int, ClusterPoint>();
        public SortedList<int, ClusterPoint> PointMap
        {
            get
            {
                return this.pointMap;
            }
        }
        Ellipse currentEllipse = new Ellipse();
        List<Ellipse> complexEventEllipses = new List<Ellipse>();
        double minX, minY;
        double maxX, maxY;
        public int MainAxis { get; set; }
        public int SecondAxis { get; set; }
        double lengthX, lengthY;
        double xStep, yStep;
        int currentIndex = 0;
        readonly Queue<Line> snake = new Queue<Line>();
        public static readonly int SnakeSize = 3;
        public MiniMap(int mainAxis, int secondAxis, bool ignoreMissingImages = false)
        {
            InitializeComponent();
            if (!System.IO.File.Exists(MiniMap.PointMapPath))
            {
                throw new ArgumentException("The point map file was not found");
            }
            this.MainAxis = mainAxis;
            this.SecondAxis = secondAxis;
            using (var reader = new System.IO.StreamReader(MiniMap.PointMapPath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var point = new ClusterPoint(line);
                    this.pointMap.Add(point.RealId, point);
                    
                }

            }
            var newList = new SortedList<int, ClusterPoint>();
            foreach (var item in this.pointMap)
            {
                while (!ignoreMissingImages && !System.IO.File.Exists(item.Value.ToImageString()))
                {
                    item.Value.ImageId += 1;
                }
                newList.Add(item.Key, item.Value);
            }
            this.pointMap = newList;
        }
        public string NextFrame()
        {
            if (this.currentIndex < this.pointMap.Count)
                this.currentIndex++;
            return this.updateCanvas();
        }
        public string LastFrame()
        {
            if (this.currentIndex > 0)
                this.currentIndex--;
            return this.updateCanvas();
        }

        public void TryFrame(int frameNumber)
        {
            if ( this.pointMap.ContainsKey(frameNumber))
                this.currentIndex = this.pointMap.IndexOfKey(frameNumber);
            this.updateCanvas();
        }
        
        public void RefreshAxisDirections()
        {
            this.refreshPointMap();
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            this.refreshPointMap();
        }
        private void refreshPointMap()
        {
            this.miniMap.Children.Clear();
            this.minX = this.pointMap.Min(item => item.Value[this.MainAxis]);
            this.minY = this.pointMap.Min(item => item.Value[this.SecondAxis]);
            this.maxX = this.pointMap.Max(item => item.Value[this.MainAxis]);
            this.maxY = this.pointMap.Max(item => item.Value[this.SecondAxis]);
            this.lengthX = Math.Abs(maxX - minX);
            this.lengthY = Math.Abs(maxY - minY);
            this.xStep = this.miniMap.ActualWidth / lengthX;
            this.yStep = this.miniMap.ActualHeight / lengthY;
            foreach (var item in this.pointMap)
            {
                var newDataPoint = new Ellipse
                {
                    Height = 5,
                    Width = 5,
                    Fill = new SolidColorBrush(item.Value.ClusterColor),
                    Opacity = 0.5
                };
                this.miniMap.Children.Add(newDataPoint);
                Canvas.SetLeft(newDataPoint, (item.Value[this.MainAxis] - minX) * xStep);
                Canvas.SetTop(newDataPoint, this.miniMap.ActualHeight - (item.Value[this.SecondAxis] - minY) * yStep);

            }
            currentEllipse.SetValue(Canvas.TopProperty, this.miniMap.ActualHeight);
            currentEllipse.SetValue(Canvas.LeftProperty, 0d);
        }
        private string updateCanvas()
        {
            if (currentIndex >= 0 && currentIndex < this.pointMap.Count)
            {
                this.miniMap.Children.Remove(currentEllipse);
                var newCoords = new Point((this.pointMap.Values[currentIndex][this.MainAxis] - minX) * xStep,
                    this.miniMap.ActualHeight - (this.pointMap.Values[currentIndex][this.SecondAxis] - minY) * yStep);
                var newSnakeLine = new Line
                {
                    X1 = newCoords.X,
                    Y1 = newCoords.Y,
                    X2 =
                        ((double) currentEllipse.GetValue(Canvas.LeftProperty)).ToString() == double.NaN.ToString()
                            ? 0
                            : (double) currentEllipse.GetValue(Canvas.LeftProperty),
                    Y2 =
                        ((double) currentEllipse.GetValue(Canvas.TopProperty)).ToString() == double.NaN.ToString()
                            ? 0
                            : (double) currentEllipse.GetValue(Canvas.TopProperty)
                };
                newSnakeLine.StrokeThickness = 2;
                newSnakeLine.Stroke = Brushes.Black;
                this.miniMap.Children.Add(newSnakeLine);
                this.snake.Enqueue(newSnakeLine);
                if (this.snake.Count > MiniMap.SnakeSize)
                {
                    this.miniMap.Children.Remove(this.snake.Dequeue());
                }
                currentEllipse = new Ellipse
                {
                    Height = 10,
                    Width = 10,
                    Fill = new SolidColorBrush(this.pointMap.ElementAt(this.currentIndex).Value.ClusterColor),
                    Opacity = 1
                };
                Canvas.SetLeft(currentEllipse, newCoords.X);
                Canvas.SetTop(currentEllipse, newCoords.Y);
                this.miniMap.Children.Add(currentEllipse);
                return this.pointMap.ElementAt(this.currentIndex).Value.ToImageString();
            }
            else
            {
                MessageBox.Show("We reached the end of the data points!");
                return this.pointMap.ElementAt(this.currentIndex - 1).Value.ToImageString();
            }
        }

        private void resetComplexEvents()
        {
            this.complexEventEllipses.ForEach(item => this.miniMap.Children.Remove(item));
            this.complexEventEllipses.Clear();
        }
        public void DisplayComplexEvent(int startFrame, int endFrame)
        {
            this.resetComplexEvents();
            for (int frameNumber = startFrame; frameNumber < endFrame; frameNumber++)
            {
                int auxIndex = this.pointMap.IndexOfKey(frameNumber);
                if (auxIndex == -1)
                    continue;
                var newCoords = new Point((this.pointMap.Values[auxIndex][this.MainAxis] - minX) * xStep,
                     this.miniMap.ActualHeight - (this.pointMap.Values[auxIndex][this.SecondAxis] - minY) * yStep);
                var tempEllipse = new Ellipse
                {
                    Height = 15,
                    Width = 15,
                    Fill = new SolidColorBrush(this.pointMap.ElementAt(auxIndex).Value.ClusterColor),
                    Opacity = 1
                };
                Canvas.SetLeft(tempEllipse, newCoords.X);
                Canvas.SetTop(tempEllipse, newCoords.Y);
                this.miniMap.Children.Add(tempEllipse);
                this.complexEventEllipses.Add(tempEllipse);   
            }
        }
    }
}
