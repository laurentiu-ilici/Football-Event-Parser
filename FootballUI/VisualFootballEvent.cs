using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VISParser;
using System.Windows.Media;
namespace FootballUI
{
    class ColorScheme
    {
       Dictionary<string,Color> scheme;
       public ColorScheme()
       {
         this.scheme = new Dictionary<string,Color>();
         this.initialize();
       }
       private void initialize()
        {
            this.scheme.Add("ThrowIn", Colors.Cyan);
            this.scheme.Add("Pass", Colors.Red);
            this.scheme.Add("SimplePos", Colors.Blue);
            this.scheme.Add("CornerKick", Colors.Violet);
            this.scheme.Add("OtherFixture", Colors.Black);
        }
       public String GetColor(string eventName)
       {
           return this.scheme[eventName].ToString();
       }
    }
    sealed class VisualFootballEvent
    {
        private static ColorScheme scheme = new ColorScheme();
        public FootballEvent Event { private set; get; }
        
        public VisualFootballEvent(FootballEvent footEvent)
        {
            this.Event = footEvent;
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
        
    }
}
