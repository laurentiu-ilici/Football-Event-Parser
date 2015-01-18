using VISParser.Events;
using VISParser.FieldObjects;

namespace VISParser.Events
{
    public sealed class Offside : FixedPlay
    {
        public Offside(Player responsiblePlayer, Player actingPlayer, Frame startFrame, Frame endFrame)
            : base("Offside",responsiblePlayer, actingPlayer, null, startFrame, endFrame, false)
        {
            this.EventType = FootballEventTypes.Offside;
        }
    }
}
