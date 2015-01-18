using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VISParser.FieldObjects;

namespace VISParser.Events
{
    internal sealed class BallOffField :FootballEvent
    {
        //For this class the acting player is the last persone to touch the ball... 
        public BallOffField(Player actingPlayer, Player targetPlayer, Frame startFrame, Frame endFrame) :
            base("BallOffField", actingPlayer, targetPlayer, startFrame, endFrame, false)
        {
            this.EventType = FootballEventTypes.BallOffField;
        }
    }
}
