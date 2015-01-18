using System;
using VISParser.FieldObjects;

namespace VISParser.Events
{
    [Serializable]
    public sealed class ThrowIn : FixedPlay
    {
        public ThrowIn(Player responsiblePlayer,Player actingPlayer,Player targetPlayer, Frame startFrame, Frame endFrame,bool userDefined) :
            base("ThrowIn",responsiblePlayer,actingPlayer,targetPlayer, startFrame, endFrame,userDefined)
        {
            EventType = FootballEventTypes.ThrowIn;
        }
    }
}
