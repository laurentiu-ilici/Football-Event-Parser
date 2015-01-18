using System.Drawing;
using System;

namespace FootballUI
{
    public sealed class RectangleStruct
    {
        public PointF NorthWest;
        public PointF SouthEast;
        public bool IsSet { get; private set; }
        public RectangleStruct()
        {
            this.IsSet = false;
        }
        public RectangleStruct(PointF NorthWest, PointF SouthEast)
        {
            this.NorthWest = NorthWest;
            this.SouthEast = SouthEast;
            this.IsSet = true;
        }
        public float Area
        {
            get
            {
                float area = Math.Abs(NorthWest.X - SouthEast.X) * Math.Abs(NorthWest.Y - SouthEast.Y);
                return area;
            }
        }
    }
}
