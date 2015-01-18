using System;
using VISParser.FieldObjects;

namespace VISParser.Events
{
    /// <summary>
    /// Probably will be removed and extended into more specialized classes
    /// </summary>
    [Serializable]
    public sealed class OtherFixture : FixedPlay
    {
        public OtherFixture(Player responsiblePlayer, Player actingPlayer, Player targetPlayer, Frame startFrame, Frame endFrame, bool userDefined) :
            base("OtherFixture", responsiblePlayer, actingPlayer, targetPlayer, startFrame, endFrame, userDefined)
        {
            this.EventType = FootballEventTypes.OtherFixture;
        }
    }
}
