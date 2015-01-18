using VISParser.FieldObjects;

namespace VISParser.Events
{

    public sealed class CornerKick : FixedPlay
    {
        public CornerKick(Player responsiblePlayer,Player actingPlayer,Player targetPlayer, Frame startFrame, Frame endFrame,bool userDefined) :
            base("CornerKick",responsiblePlayer, actingPlayer,targetPlayer, startFrame, endFrame,userDefined)
        {
            this.EventType = FootballEventTypes.CornerKick;
        }
    }
}
