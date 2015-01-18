using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace FooballProject
{
    
    public sealed class ColorScheme
    {
        Dictionary<string, Color> scheme;
        public ColorScheme()
        {
            this.scheme = new Dictionary<string, Color>();
            this.initialize();
        }
        private void initialize()
        {
            this.scheme.Add("ThrowIn", Colors.Black);
            this.scheme.Add("Pass", Colors.Black);
            this.scheme.Add("SimplePos", Colors.Black);
            this.scheme.Add("CornerKick", Colors.Black);
            this.scheme.Add("OtherFixture", Colors.Black);
            this.scheme.Add("UnknownEvent", Colors.Black);
            this.scheme.Add("BallOffField", Colors.Black);
            this.scheme.Add("Shot",Colors.Black);
            this.scheme.Add("Offside",Colors.Black);
            this.scheme.Add("GoalKick",Colors.Black);
        }
        public String GetColor(string eventName)
        {
            return this.scheme[eventName].ToString();
        }

    }
}
