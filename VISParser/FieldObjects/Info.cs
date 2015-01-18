using System.Drawing;

namespace VISParser.FieldObjects
{
    
    public abstract class Info
    {
        public static ColorScheme Scheme = new ColorScheme();

        protected Info()
        {
            
        }
        public virtual double XCoord { get; set; }
        public virtual double YCoord { get; set; }
        public virtual double Speed { get; set; }
        public virtual int ItemId { get; set; }
        public virtual double[] Coords
        {
            get
            {
                var result = new double[2];
                result[0] = this.XCoord;
                result[1] = this.YCoord;
                return result;
            }
        }
        public bool IsOutOfFieldLength
        {
            get { return this.XCoord > 1 || this.XCoord < -1; }
        }
        public bool IsOutOfFieldWidth
        {
            get { return this.YCoord < -1 || this.YCoord > 1; }
        }
        public virtual bool IsOnField
        {
            get
            {
                return this.XCoord < -1 || this.XCoord > 1
                       || this.YCoord < -1 || this.YCoord > 1;
            }
        }
        public abstract  Info CopyObject(Info other);

        public virtual Color ItemColor 
        { 
            get 
            { 
                return Info.Scheme.GetColor(this.ItemId); 
            } 
        }
        internal int Distance(Info info)
        {
            return (int)PitchDimensionsHelper.CalculateDistance(this.XCoord, this.YCoord, info.XCoord, info.YCoord);
        }
    }
}
