using System.Net.NetworkInformation;
using VISParser.FieldObjects;

namespace VISParser.Events
{
    public sealed class Shot : FootballEvent
    {
        private readonly int ballYCoord;
        private readonly double ballZCoord;
        
        public Shot(Player actingPlayer, Frame startFrame, Frame endFrame,bool IsEastBound, bool userDefined) :
            base("Shot", actingPlayer,null, startFrame, endFrame, userDefined)
        {
            this.EventType = FootballEventTypes.Shot;
            var ball = this.EndFrame.GetBall();
            this.ballYCoord = PitchDimensionsHelper.RelativePosY(ball.YCoord);
            this.ballZCoord = ball.ZCoord;
            if (actingPlayer.Team == Teams.Away && IsEastBound)
                this.IsAttackingShot = true;
            else
            {
                this.IsAttackingShot = false;
            }
        }

        public bool IsAttackingShot { private set; get; }
        public bool IsOnGoal
        {
            get
            {   return this.ballZCoord < PitchDimensionsHelper.GoalHeight &&
                       this.ballYCoord > PitchDimensionsHelper.GoalNorthBar &&
                       this.ballYCoord < PitchDimensionsHelper.GoalSouthBar;
            }
        }

        public bool IsGoalAttempt
        {
            get
            { 
                return ballYCoord > PitchDimensionsHelper.SmallPenaltyAreaNorthBorder &&
                      ballYCoord < PitchDimensionsHelper.SmallPenaltyAreaSouthBorder;
            }
        }
        //This will be something that the user can set.
        public bool IsGoal { get; set; }
    }
}
