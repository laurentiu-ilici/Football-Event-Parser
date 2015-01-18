using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VISParser.FieldObjects
{
    public sealed class Frame
    {
        public int FrameNumber { get; set; }
        public int Minute { get; set; }
        public int Section { get; set; }
        public List<Info> Objects;
        public int PossessionId { get; set; }
        public static Field FieldShape = new Field();
        private void init()
        {
            this.PossessionId = -1;

        }
       
        public Frame() 
        {
            this.init();
        }
        public Frame(int FrameNumber, List<Info> Objects)
        {
            this.FrameNumber = FrameNumber;
            this.Objects = Objects;
            this.init();
        }
        
        public Frame(string frameData)
        {

            string[] split = frameData.TrimEnd(';'). Split(',');
            this.FrameNumber = int.Parse(split[0]);
            this.Minute = int.Parse(split[1]);
            this.Section = int.Parse(split[2]);
            this.Objects = new List<Info>();
            this.init();
        }
        public Ball GetBall()
        {
            return this.Objects.OfType<Ball>().Select(item => item as Ball).FirstOrDefault();
        }
        /// <summary>Get the player that is currently in possession of the ball. If there is none returns null.</summary>
        public Player GetPossessionPlayer()
        {
            return this.PossessionId != -1 ? this.GetPlayerById(this.PossessionId) : null;
        }
        public Player GetPlayerById(int id)
        {
            foreach(var obj in this.Objects)
            {
                if(obj is Player)
                {
                    var player = obj as Player;
                    if( player.ItemId == id)
                        return player;
                }
            }
            throw new ArgumentException("The player id was not found, there must be a bug");
        }
        public List<double[]> PlayerAbsolutePositions
        {
            get
            {
                return this.Objects.OfType<Player>().Select(obj => Frame.FieldShape.ConvertToAbsoluteCoordinates(obj)).ToList();
            }
        }

   
        //For Roman
        public override string ToString()
        {
            var sb = new StringBuilder(this.FrameNumber.ToString());
            sb.Append(';');
            foreach (var item in this.Objects)
            {
                sb.Append(item.XCoord.ToString());
                sb.Append(',');
                sb.Append(item.YCoord.ToString());
                sb.Append(';');
            }
            return sb.ToString();
        }
    }
}
