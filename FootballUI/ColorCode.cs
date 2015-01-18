using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace FootballUI
{
    internal sealed class ColorCode
    {
         Dictionary<int, Color> playerColor = new Dictionary<int, Color>();
         public ColorCode()
        {
            init();
        }
        public Color GetColor(int playerId)
        {
            return this.playerColor[playerId];
        }
        /// <summary>
        /// Initialize the colors for your player's fields of influence
        /// </summary>
        private void init()
        {
            
            playerColor.Add(-1, Colors.White);
            playerColor.Add(0, Colors.Red);
            playerColor.Add(1, Color.FromRgb(6, 114, 48));
            playerColor.Add(2, Color.FromRgb(19, 125, 57));
            playerColor.Add(3, Color.FromRgb(31, 136, 66));
            playerColor.Add(4, Color.FromRgb(42, 147, 75));
            playerColor.Add(5, Color.FromRgb(53, 158, 83));
            playerColor.Add(6, Color.FromRgb(64, 170, 92));
            playerColor.Add(7, Color.FromRgb(82, 179, 101));
            playerColor.Add(8, Color.FromRgb(101, 188, 110));
            playerColor.Add(9, Color.FromRgb(119, 197, 120));
            playerColor.Add(10, Color.FromRgb(151, 212, 147));
            playerColor.Add(11, Color.FromRgb(135, 204, 133));
            playerColor.Add(12, Color.FromRgb(12, 86, 160));
            playerColor.Add(13, Color.FromRgb(21, 98, 169));
            playerColor.Add(14, Color.FromRgb(30, 109, 178));
            playerColor.Add(15, Color.FromRgb(41, 121, 185));
            playerColor.Add(16, Color.FromRgb(53, 133, 191));
            playerColor.Add(17, Color.FromRgb(65, 145, 197));
            playerColor.Add(18, Color.FromRgb(80, 155, 203));
            playerColor.Add(19, Color.FromRgb(95, 165, 208));
            playerColor.Add(20, Color.FromRgb(110, 175, 214));
            playerColor.Add(21, Color.FromRgb(128, 185, 218));
            playerColor.Add(22, Color.FromRgb(147, 185, 222));
          
        }
    }
}
