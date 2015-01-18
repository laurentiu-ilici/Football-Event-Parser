using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VISParser.FieldObjects;
namespace VISParser.Events
{
    //Agregates several events into one "superevent"
    public sealed class ComplexEvent
    {
        public SortedList<int,FootballEvent> Events{private set; get;}
        public Frame StartFrame { get; private set; }
        public Frame EndFrame {get; private set;}
        public Dictionary<int,Player> ParticipatingPlayers { private set; get; }
        public ComplexEvent(SortedList<int,FootballEvent> events)
        {
            this.Events = events;
            this.StartFrame = events.ElementAt(0).Value.StartFrame;
            this.EndFrame = events.ElementAt(events.Count-1).Value.EndFrame;
            this.initializePlayers();
        }
        private void initializePlayers()
        {
            this.ParticipatingPlayers = new Dictionary<int,Player>();
            foreach (var footEvent in this.Events)
            {
                if(footEvent.Value.ActingPlayer!=null && !this.ParticipatingPlayers.ContainsKey(footEvent.Value.ActingPlayer.ItemId))
                    this.ParticipatingPlayers.Add(footEvent.Value.ActingPlayer.ItemId,footEvent.Value.ActingPlayer);
            }
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Complex Event Team=");
            sb.Append(this.Events.ElementAt(0).Value.ActingPlayer.Team.ToString());
            sb.Append(string.Format(" Starting: {0}", this.StartFrame.FrameNumber));
            sb.Append(string.Format(" Ending: {0}", this.EndFrame.FrameNumber));
            return sb.ToString();
        }


    }
}
