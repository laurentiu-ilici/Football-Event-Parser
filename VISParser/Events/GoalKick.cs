using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VISParser.FieldObjects;

namespace VISParser.Events
{
    public sealed class GoalKick : FixedPlay
    {
        public GoalKick(Player responsiblePlayer, Player actingPlayer, Frame startFrame, Frame endFrame,bool userDefined) :
            base("GoalKick", responsiblePlayer, actingPlayer, null, startFrame, endFrame, userDefined)
        {
            this.EventType = FootballEventTypes.GoalKick;

        }
    }
}
