using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VISParser.Events;
using VISParser.FieldObjects;
using System.Windows.Media;
namespace FootballProject
{
    //This should not really inherit from VisualFootballEvent, as it is really a collection of 
    //football events... I've broken the rules here because it would be a big PAIN to rewrite all
    //the visuals. If I have time in the future this should change...
    public sealed class VisualComplexEvent : VisualFootballEvent
    {
        public ComplexEvent ComplexEvent { private set; get; }
        private new const string eventImage = "/Resources/ComplexEvent.png";
        public VisualComplexEvent(ComplexEvent complexEvent, bool isUserDefined) :
            base(complexEvent.Events.ElementAt(0).Value)
        {
            this.ComplexEvent = complexEvent;
            this.IsUserDefinedEvent = isUserDefined;
            this.EventImage = VisualComplexEvent.eventImage;



            this.EventsToDisplay = new List<VisualFootballEvent>();
            foreach (var footEvent in complexEvent.Events.Values)
            {
                this.EventsToDisplay.Add(new VisualFootballEvent(footEvent));
            }

        }

        //TO DO: 
        //This is a very dirty little hack to get the list properly displayed on the interface. 
        //In principal it is a waste of memory and would need to be improved in the future.
        public IList<VisualFootballEvent> EventsToDisplay
        {
            get;
            private set;
        }
        public new string DisplayText
        {
            get { return this.ComplexEvent.ToString(); }
        }
        public new Brush PossesionColor
        {
            get
            {
                if (this.ComplexEvent.Events.ElementAt(0).Value.ActingPlayer.Team == Teams.Away)
                {
                    return Brushes.Blue;
                }
                else
                {
                    return Brushes.Green;
                }
            }
        }
        public override bool ContainsFrame(int frameNumber)
        {
            return this.ComplexEvent.StartFrame.FrameNumber <= frameNumber && this.ComplexEvent.EndFrame.FrameNumber >= frameNumber;
        }
        public new int EventId
        {
            get { return this.ComplexEvent.StartFrame.FrameNumber; }
        }
        public new bool IsUserDefinedEvent { get; private set; }

        public int GetEventEnd()
        {
            return this.ComplexEvent.EndFrame.FrameNumber;
        }

    }
}
