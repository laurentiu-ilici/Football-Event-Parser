using System;

namespace VISParser.FieldObjects
{
    public enum Teams
    {
        None = -1,
        Home = 0,
        Away = 1
    }

    public enum PlayerRoles
    {
        Undefined=0,
        GoalKeeper = 1,
        Defender =2,
        Midfielder = 3,
        Attacker = 4
    }
    public sealed class Player : Info
    {

        public int Jersey { get; private set; }
        public Teams Team { get; private set; }
        public bool HasPossession { get; set; }
        public PlayerRoles Role { get; set; }
        public Player() { }
        public Player(int position, string data)
        {
            string[] split = data.Split(',');
            this.ItemId = position;
            this.Jersey = int.Parse(split[0], System.Globalization.CultureInfo.InvariantCulture);
            this.XCoord = double.Parse(split[1],System.Globalization.CultureInfo.InvariantCulture);
            this.YCoord = double.Parse(split[2], System.Globalization.CultureInfo.InvariantCulture);
            this.Speed = double.Parse(split[3], System.Globalization.CultureInfo.InvariantCulture);
            this.Team = position < 12 ? Teams.Home : Teams.Away;
            //TODO: This will be deleted when we get new data because the GK deosn't necessarily have to be this guy
            if (this.ItemId == 1 || this.ItemId == 12)
                this.Role = PlayerRoles.GoalKeeper;
            else
                this.Role = PlayerRoles.Undefined;
        }
        internal new double Distance(Info info)
        {
            return PitchDimensionsHelper.CalculateDistance(this.XCoord, this.YCoord, info.XCoord, info.YCoord);
        }
        public override Info CopyObject(Info other)
        {
            if (other is Player)
            {
                var toCopy = other as Player;
                this.XCoord = toCopy.XCoord;
                this.YCoord = toCopy.YCoord;
                this.Jersey = toCopy.Jersey;
                this.Speed = toCopy.Speed;
                this.Team = toCopy.Team;
                this.ItemId = toCopy.ItemId;
                return this as Info;
            }
            else
                throw new ArgumentException("This is not a Player object!");
        }
        public Player Average(Info other)
        {
            var toCopy = other as Player;
            var result = new Player();
            result.CopyObject(toCopy);
            result.XCoord += this.XCoord;
            result.YCoord += this.YCoord;
            result.XCoord /= 2;
            result.YCoord /= 2;
            return result;
        }
     
        //Dirty hack to see what happens when a player enters possession and exits it.
        //As the name suggests this will be a debug only feature and should at some point be
        //deleted.
        public string DebugPossesionInfo
        {
            get;
            set;
        }
        public override string ToString()
        {
            return String.Format("{0} : Jersey Number {1}", this.ItemId, this.Jersey);
        }

        
    }
}
