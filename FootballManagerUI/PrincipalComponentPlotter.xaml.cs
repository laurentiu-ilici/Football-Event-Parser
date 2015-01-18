using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FootballManagerUI
{
    /// <summary>
    /// Interaction logic for PrincipalComponentPlotter.xaml
    /// </summary>
    public partial class PrincipalComponentPlotter : UserControl
    {
        double xStepCount;
        double yStepCount = 200;
        SortedList<int, ClusterPoint> pointMap = new SortedList<int, ClusterPoint>();
        Ellipse currentIndexPointer;
        int axistToPlot;
        double stepLengthXAxis;
        double stepLengthYAxis;
        double minAxisValue, maxAxisValue;
        int currentIndex = 0;
        List<Line> linesOnScreen = new List<Line>();
        public int AxisToPlot
        {
            get
            {
                return this.axistToPlot;
            }
            set
            {
                this.axistToPlot = value;
                this.stepLengthXAxis = this.plottingSurface.Width / this.xStepCount;
                this.stepLengthYAxis = this.plottingSurface.Height /this.yStepCount;
                this.minAxisValue = this.pointMap.Min(item => item.Value[this.axistToPlot]);
                this.maxAxisValue = this.pointMap.Max(item => item.Value[this.axistToPlot]);
                this.updatePlot(true);
            }
        }
        public PrincipalComponentPlotter(int axisToPlot, SortedList<int, ClusterPoint> list)
        {
            InitializeComponent();
            this.pointMap = list;
            this.xStepCount = Math.Min(this.pointMap.Count - 1, 500);
            this.AxisToPlot = axisToPlot;
           

        }
        private void addLines(int lowerBound)
        {
            for (int index = 1; index < this.xStepCount; index++)
            {
                if (lowerBound + index >= this.pointMap.Count)
                    break;
                var line = new Line
                {
                    X1 = ((index - 1)%this.xStepCount)*this.stepLengthXAxis,
                    Y1 =this.plottingSurface.Height -
                        ((this.pointMap.ElementAt(index - 1 + lowerBound).Value[this.AxisToPlot] - this.minAxisValue)/
                         Math.Abs(this.minAxisValue - this.maxAxisValue))*this.plottingSurface.Height,
                    X2 = (index%this.xStepCount)*this.stepLengthXAxis,
                    Y2 =(this.plottingSurface.Height -
                         ((this.pointMap.ElementAt(index + lowerBound).Value[this.AxisToPlot] - this.minAxisValue)/
                          Math.Abs(this.minAxisValue - this.maxAxisValue))*this.plottingSurface.Height),
                    Stroke = new SolidColorBrush(Colors.Black)
                };
                this.plottingSurface.Children.Add(line);
                this.linesOnScreen.Add(line);
            }

        }
        private void addEllipse()
        {
            this.currentIndexPointer.SetValue(Canvas.LeftProperty, this.linesOnScreen[this.currentIndex % (int)(this.xStepCount - 2)].X1);
            this.currentIndexPointer.SetValue(Canvas.TopProperty, this.linesOnScreen[this.currentIndex % (int) (this.xStepCount - 2)].Y1 - 4);
            this.plottingSurface.Children.Add(this.currentIndexPointer);
        }
        private void updatePlot(bool forced=false)
        {
            if (this.currentIndexPointer != null)
                this.plottingSurface.Children.Remove(this.currentIndexPointer);
            this.currentIndexPointer = new Ellipse();
            this.currentIndexPointer.Height = 8;
            this.currentIndexPointer.Width = 8;
            this.currentIndexPointer.Stroke = new SolidColorBrush(Colors.Red);

            int lowerBound;
            if (forced || currentIndex % this.xStepCount == 0)
            {

                this.plottingSurface.Children.Clear();
                this.linesOnScreen.Clear();
                lowerBound = 1 + (int) this.xStepCount * (int) (currentIndex/this.xStepCount);
                this.addLines(lowerBound);
            }
            this.addEllipse();
            this.plottingSurface.InvalidateVisual();
        }
        public void NextFrame()
        {
            if (this.pointMap.Count > this.currentIndex)
                this.currentIndex++;
            else
                return;
            updatePlot();
        }
        public void LastFrame()
        {
            if (currentIndex > 0)
                currentIndex--;
            else
                return;
            updatePlot();
        }
    }
}
