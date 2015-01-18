using System.Text;
using VISParser.FieldObjects;

namespace VISParser.Events
{
    public sealed class UnknownEvent : FootballEvent
    {
        //Unkown events are always user defined.
        public UnknownEvent(Frame startFrame, Frame endFrame):base("UnknownEvent",null,null,startFrame,endFrame,true)
        {
            this.EventType = FootballEventTypes.UnknownEvent;
        }
        public override string ToString()
        {
            var sb = new StringBuilder("Unknown Event");
            sb.Append(this.EventName);
            sb.Append(" Starting at frame: " + this.StartFrame.FrameNumber.ToString());
            sb.Append(" Stoping at frame: " + this.EndFrame.FrameNumber.ToString());
            return sb.ToString();
        }
    }
}
