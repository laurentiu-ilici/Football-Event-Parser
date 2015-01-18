using System.Collections.Generic;
using System.Windows.Media;

namespace FootballManagerUI
{
    public class ClusterColors
    {
        Dictionary<int, Color> colorScheme = new Dictionary<int, Color>();
        public ClusterColors()
        {
            this.colorScheme.Add(0, Colors.Red);
            this.colorScheme.Add(1, Colors.Green);
            this.colorScheme.Add(2, Colors.Blue);
            this.colorScheme.Add(3, Colors.Black);
            this.colorScheme.Add(4, Color.FromRgb(128, 177, 211));
        }
        public Color GetColor(int key)
        { 
            if(this.colorScheme.ContainsKey(key))
                return this.colorScheme[key];
            return Colors.White;
        }
    }
}
