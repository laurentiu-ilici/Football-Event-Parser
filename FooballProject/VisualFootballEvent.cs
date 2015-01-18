using System;
using System.Windows.Media;
using VISParser.Events;

namespace FootballProject
{

    public  class VisualFootballEvent
    {
        private static FooballProject.ColorScheme scheme = new FooballProject.ColorScheme();
        private const string simplePosImg = "/Resources/SimplePos.png";
        private const string otherFixture = "/Resources/OtherFixture.png";
        private const string cornerKick = "/Resources/CornerKick.png";
        private const string throwIn = "/Resources/ThrowIn.png";
        private const string passTrue = "/Resources/PassTrue.png";
        private const string passFalse = "/Resources/PassFalse.png";
        private const string unknownEvent = "/Resources/UnknownEvent.png";
        private const string offside = "/Resources/Offside.jpg";
        private const string shot = "/Resources/Shot.png";
        private const string shotOnTarget = "/Resources/Shotongoal.jpg";
        private const string goalKick = "/Resources/GoalKick.jpg";
        private FootballEvent theEvent;
        public VisualFootballEvent(FootballEvent footEvent)
        {
            this.Event = footEvent;
        }
        private void setEventImage()
        {
            
            switch (this.Event.EventType)
            {
                case FootballEventTypes.SimplePossession:
                    this.EventImage = VisualFootballEvent.simplePosImg;
                    break;
                case FootballEventTypes.OtherFixture:
                    this.EventImage = VisualFootballEvent.otherFixture;
                    break;
                case FootballEventTypes.Shot:
                    var shot = this.Event as Shot;
                    this.EventImage = shot.IsOnGoal ? VisualFootballEvent.shot : VisualFootballEvent.shotOnTarget;
                    break;
                case FootballEventTypes.GoalKick:
                    this.EventImage = goalKick;
                    break;
                case FootballEventTypes.ThrowIn:
                    this.EventImage = VisualFootballEvent.throwIn;
                    break;
                case FootballEventTypes.UnknownEvent:
                    this.EventImage = VisualFootballEvent.unknownEvent;
                    break;
                case FootballEventTypes.Pass:
                    var pass = this.Event as Pass;
                    this.EventImage = pass.IsSuccessful ? VisualFootballEvent.passTrue : VisualFootballEvent.passFalse;
                    break;
                case FootballEventTypes.CornerKick:
                    this.EventImage = VisualFootballEvent.cornerKick;
                    break;
                case FootballEventTypes.Offside:
                    this.EventImage = VisualFootballEvent.offside;
                    break;
               
            }
        }
        public String EventColor
        {
            get
            {
                return VisualFootballEvent.scheme.GetColor(this.Event.EventName);
            }
        }
        public string DisplayText
        {
            get { return this.Event.ToString(); }
        }
        public string EventImage
        {
            protected set;
            get;
        }
        public FootballEvent Event
        {
            internal set
            {
                this.theEvent = value;
                this.setEventImage();
            }
            get
            {
                return this.theEvent;
            }
        }
        public bool IsUserDefinedEvent
        {
           
            get
            {
                return this.Event.IsUserDefinedEvent;
            }
        }
        public Brush PossesionColor
        {
            get
            {

                if (this.Event.ActingPlayer == null)
                    return Brushes.Black;
                else if (this.Event.ActingPlayer.Team == VISParser.FieldObjects.Teams.Away)
                {
                    return Brushes.Blue;
                }
                else
                {
                    return Brushes.Green;
                }
            }
        }
        public virtual bool ContainsFrame(int frameNumber)
        {
            return this.Event.StartFrame.FrameNumber <= frameNumber && this.Event.EndFrame.FrameNumber >= frameNumber;
        }
        public int EventId
        {
            get { return this.Event.StartFrame.FrameNumber; }
        }
        public bool IsUserMarked
        {
            get;
            set;
        }
    }
}
