using System;
using VISParser.FieldObjects;

namespace VISParser.Events
{
    [Serializable]
    public sealed class SimplePossession : FootballEvent
    {
       
        public SimplePossession(Player actingPlayer, Frame startFrame, Frame endFrame,bool userDefined) :
            base("SimplePos",actingPlayer,null, startFrame, endFrame, userDefined)
        {
            this.EventType = FootballEventTypes.SimplePossession;
        }
        public double RunLength
        { 
            get
            {
                Player stopPos = this.EndFrame.GetPlayerById(this.ActingPlayer.ItemId);
                return PitchDimensionsHelper.CalculateDistance(this.ActingPlayer.XCoord,this.ActingPlayer.YCoord,stopPos.XCoord,stopPos.YCoord);
            }
        }
    }
}
