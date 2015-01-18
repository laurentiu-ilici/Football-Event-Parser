using System.Globalization;
using System.Text;
using System.Xml;
using VISParser.FieldObjects;

namespace VISParser.Events
{
    //Base class for football events like off-sides, passes etc.
    public enum FootballEventTypes
    {
        None = -1,
        Pass = 1,
        CornerKick = 2,
        ThrowIn = 3,
        OtherFixture = 4,
        UnknownEvent = 5,
        Shot = 6,
        Offside=7,
        GoalKick=8,
        BallOffField = 9,
        SimplePossession = 10
    }
    public abstract class FootballEvent
    {
        public string EventName { protected set; get; }
        public Player ActingPlayer { protected set; get; }
        public Frame StartFrame { protected set; get; }
        public Frame EndFrame { protected set; get; }
        public Player TargetPlayer { protected set; get; }
        /// <summary>
        /// Returns true when the user defined the event and false if the event was algoritmically found.
        /// </summary>
        public bool IsUserDefinedEvent { protected set; get; }
        public FootballEventTypes EventType { protected set; get; }
        protected FootballEvent(string eventName, Player actingPlayer, Player targetPlayer, Frame startFrame, Frame endFrame, bool userDefined)
        {
            this.EventName = eventName;
            this.ActingPlayer = actingPlayer;
            this.TargetPlayer = targetPlayer;
            this.StartFrame = startFrame;
            this.EndFrame = endFrame;
            this.IsUserDefinedEvent = userDefined;
        }

        public int EventStart
        {
            get { return this.StartFrame.FrameNumber; }
        }

        public int EventEnd
        {
            get { return this.EndFrame.FrameNumber; }
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.EventName);
            if (this.ActingPlayer != null)
                sb.Append(string.Format(" Acting Pl {0}", this.ActingPlayer.ItemId));
            if (this.TargetPlayer != null)
                sb.Append(string.Format(" Target Pl {0}", this.TargetPlayer.ItemId));
            sb.Append(string.Format(" Starting: {0}", this.StartFrame.FrameNumber));
            sb.Append(string.Format(" Ending: {0}", this.EndFrame.FrameNumber));
            return sb.ToString();
        }
        public virtual XmlElement ToXML(XmlDocument doc)
        {
            XmlElement eventXML = doc.CreateElement(this.EventName);
            //PlayerId if there is one
            XmlAttribute actingPlayerId = eventXML.Attributes.Append(doc.CreateAttribute("actingPlayerId"));
            actingPlayerId.Value = this.ActingPlayer != null ? this.ActingPlayer.ItemId.ToString(CultureInfo.InvariantCulture) : (-1d).ToString(CultureInfo.InvariantCulture);
            XmlAttribute targetPlayerId = eventXML.Attributes.Append(doc.CreateAttribute("targetPlayerId"));
            targetPlayerId.Value = this.TargetPlayer != null ? this.TargetPlayer.ItemId.ToString(CultureInfo.InvariantCulture) : (-1d).ToString(CultureInfo.InvariantCulture);
            eventXML.Attributes.Append(actingPlayerId);
            //Start and end frame
            XmlAttribute startFrameId = doc.CreateAttribute("startFrameId");
            startFrameId.Value = this.StartFrame.FrameNumber.ToString(CultureInfo.InvariantCulture);
            eventXML.Attributes.Append(startFrameId);
            XmlAttribute endFrameId = doc.CreateAttribute("endFrameId");
            endFrameId.Value = this.EndFrame.FrameNumber.ToString(CultureInfo.InvariantCulture);
            eventXML.Attributes.Append(endFrameId);
            XmlAttribute userDefinedEvent = doc.CreateAttribute("userDefined");
            userDefinedEvent.Value = this.IsUserDefinedEvent.ToString(CultureInfo.InvariantCulture);
            eventXML.Attributes.Append(userDefinedEvent);
            return eventXML;
        }
    }
}
