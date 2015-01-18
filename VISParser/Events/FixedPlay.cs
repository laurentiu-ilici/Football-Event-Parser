using System;
using System.Globalization;
using System.Xml;
using System.Text;
using VISParser.FieldObjects;

namespace VISParser.Events
{
    //Just feels right to have this class in between events and specialized fixtures. 
    //I'm not sure why I need it yet.
    [Serializable]
    public abstract class FixedPlay : FootballEvent
    {
       //The player responsible for causing the fixed play. 
        //E.g. if we have a thrown-in someone had to kick the ball outside.
        public Player ResponsiblePlayer { get; private set; }
        protected FixedPlay(string eventName,Player responsiblePlayer,Player actingPlayer, Player targetPlayer, Frame startFrame, Frame endFrame,bool userDefined) :
            base(eventName, actingPlayer,targetPlayer, startFrame, endFrame, userDefined)
        {
            this.ResponsiblePlayer = responsiblePlayer;
        }

        public override XmlElement ToXML(XmlDocument doc)
        {
            XmlElement eventXML = base.ToXML(doc);
            XmlAttribute responsiblePlayerId = eventXML.Attributes.Append(doc.CreateAttribute("responsiblePlayerId"));
            responsiblePlayerId.Value = this.ResponsiblePlayer == null ? (-1).ToString(CultureInfo.InvariantCulture) : this.ResponsiblePlayer.ItemId.ToString(CultureInfo.InvariantCulture);
            return eventXML;

        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.EventName);
            if (this.ResponsiblePlayer != null)
                sb.Append(string.Format(" Responsible Pl: {0}", this.ResponsiblePlayer.ItemId));
            if (this.ActingPlayer != null)
                sb.Append(string.Format(" Acting Pl {0}", this.ActingPlayer.ItemId));
            if (this.TargetPlayer != null)
                sb.Append(string.Format(" Target Pl {0}", this.TargetPlayer.ItemId));
            
            sb.Append(string.Format(" Starting: {0}" , this.StartFrame.FrameNumber));
            sb.Append(string.Format(" Ending: {0}", this.EndFrame.FrameNumber));
            return sb.ToString();
        }
    }
}
