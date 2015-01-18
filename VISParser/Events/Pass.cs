using System;
using VISParser.FieldObjects;

namespace VISParser.Events
{
    public sealed class Pass : FootballEvent
    {
       
        
        public Pass(Player actingPlayer, Player targetPlayer, Frame StartFrame, Frame EndFrame,bool userDefined) :
            base("Pass", actingPlayer,targetPlayer, StartFrame, EndFrame, userDefined)
        {
            if(targetPlayer==null)
                throw new NullReferenceException("The target player is not allowed to be null for passes!");
            this.EventType = FootballEventTypes.Pass;
        }
        //A pass is successful <=> it was made 
        // between two team mates
        public bool IsSuccessful
        {
            get {return this.ActingPlayer.Team == TargetPlayer.Team;}
        }
        public double PassLength
        {
            get
            {
                Ball ballStartPos = this.StartFrame.GetBall();
                Ball ballStopPos = this.EndFrame.GetBall();
                return PitchDimensionsHelper.CalculateDistance(ballStartPos.XCoord, ballStartPos.YCoord, ballStopPos.XCoord, ballStopPos.YCoord);
            }
        }
    }
}
