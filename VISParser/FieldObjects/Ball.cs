using System;

namespace VISParser.FieldObjects
{
    
    public sealed class Ball : Info
    {
        public double ZCoord { get; set; }
        public int Possession { get; set; }
        //WTF IS THE FLAG IN THEIR DATA??????
        public double FLag { set; get; }
        public Ball() { }
        public Ball(int position, string data)
        {
            string[] split = data.Split(',');
            this.ItemId = position;
            this.XCoord = double.Parse(split[0], System.Globalization.CultureInfo.InvariantCulture);
            this.YCoord = double.Parse(split[1], System.Globalization.CultureInfo.InvariantCulture);
            //Transform to decimeters..
            this.ZCoord = double.Parse(split[2], System.Globalization.CultureInfo.InvariantCulture)/10;
            this.Speed = double.Parse(split[3], System.Globalization.CultureInfo.InvariantCulture);
            this.FLag = double.Parse(split[4], System.Globalization.CultureInfo.InvariantCulture);
            this.Possession = int.Parse(split[5], System.Globalization.CultureInfo.InvariantCulture);    
        }
        public bool IsPossessed()
        {
            return this.Possession == -1;
        }
        public bool IsMoving
        {
            get { return this.Speed > 0; }
        }
        public bool IsOffFieldLengthEast
        {
            get { return this.XCoord > 1; }
        }
        public bool IsOffFieldLengthWest
        {
            get { return this.XCoord < -1; }
        }
        public override Info CopyObject(Info other)
        {
            if (other is Ball)
            {
                var toCopy = other as Ball;
                this.XCoord = toCopy.XCoord;
                this.YCoord = toCopy.YCoord;
                this.ZCoord = toCopy.ZCoord;
                this.Speed = toCopy.Speed;
                this.Possession = toCopy.Possession;
                this.FLag = toCopy.FLag;
                this.ItemId = toCopy.ItemId;
                return this as Info;
            }
            else
                throw new ArgumentException("This is not a Ball object!");
        }
    }
}
