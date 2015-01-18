using System;

namespace VISParser.FieldObjects
{
    [Serializable]
    public sealed class Referee : Info
    {
        //Is it a field or a side ref...
        public int RefType { get; private set; }
        public bool Traked { get; private set; }
        public Referee()
        { }
        public Referee(int position, string data)
        {
            string[] split = data.Split(',');
            if (split.Length == 1 || split.Length == 0)
                this.Traked = false;
            else
            {
                this.XCoord = double.Parse(split[0], System.Globalization.CultureInfo.InvariantCulture);
                this.YCoord = double.Parse(split[1], System.Globalization.CultureInfo.InvariantCulture);
                this.Speed = double.Parse(split[2], System.Globalization.CultureInfo.InvariantCulture);
                this.RefType = position % 23;
                this.ItemId = position;
            }
        }
        public override Info CopyObject(Info other)
        {
            if (other is Referee)
            {
                var toCopy = other as Referee;
                this.RefType = toCopy.RefType;
                this.XCoord = toCopy.XCoord;
                this.YCoord = toCopy.YCoord;
                this.Speed = toCopy.Speed;
                this.ItemId = toCopy.ItemId;
                return this as Info;

            }
            else
                throw new ArgumentException("This is not a Referee object!");
        }

    }
}
