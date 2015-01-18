using System;
using VISParser.Events;
using VISParser.FieldObjects;
namespace VISParser.Parsers
{
    partial class EventParser
    {
        //This class serves as a utility class for finding the real football events.
        //It does inherit from football event, but will never get displayed to the user.
        //This is why the eventType will not be set to this class.
        [Flags]
        private enum BallState : short
        {
            None = 0,
            IsMoving = 1,
            IsPossessd = 2,
            IsOffFieldLengthEast = 4,
            IsOffFieldLengthWest = 8,
            IsOffFieldWidth = 16
        }
        private class BallStateEvent : FootballEvent
        {
            public bool IsMoving { private set; get; }
            public bool IsPossessed { private set; get; }
            public bool IsOnField { get { return !(this.IsOffFieldLength | this.IsOffFieldWidth); } }
            public bool IsOffFieldLengthEast { private set; get; }
            public bool IsOffFieldLengthWest { private set; get; }

          
            public bool IsOffFieldWidth { private set; get; }
            public BallState BallState { private set; get; }
            internal BallStateEvent(Player actingPlayer, Frame startFrame, Frame endFrame, BallState flags)
                : base("ballEvent", actingPlayer, null, startFrame, endFrame, false)
            {
                this.IsMoving = (flags & BallState.IsMoving) == BallState.IsMoving;
                this.IsPossessed = (flags & BallState.IsPossessd) == BallState.IsPossessd;
                this.IsOffFieldLengthEast = (flags & BallState.IsOffFieldLengthEast) == BallState.IsOffFieldLengthEast;
                this.IsOffFieldLengthWest = (flags & BallState.IsOffFieldLengthWest) == BallState.IsOffFieldLengthWest;
                this.IsOffFieldWidth = (flags & BallState.IsOffFieldWidth) == BallState.IsOffFieldWidth;
                this.BallState = BallState;
            }
            public bool IsOffFieldLength
            {
                get { return this.IsOffFieldLengthEast || this.IsOffFieldLengthWest; }
            }
        }
    }
}
