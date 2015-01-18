using System.Collections.Generic;
using System.Drawing;
namespace VISParser
{
    public sealed class ColorScheme
    {
       
        Dictionary<int, Color> playerColor = new Dictionary<int, Color>();
        public ColorScheme()
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
            
            playerColor.Add(-1, Color.White);
            playerColor.Add(0, Color.Red);
            playerColor.Add(1, Color.FromArgb(6, 114, 48));
            playerColor.Add(2, Color.FromArgb(19, 125, 57));
            playerColor.Add(3, Color.FromArgb(31,136,66));
            playerColor.Add(4, Color.FromArgb(42, 147, 75));
            playerColor.Add(5, Color.FromArgb(53, 158, 83));
            playerColor.Add(6, Color.FromArgb(64, 170, 92));
            playerColor.Add(7, Color.FromArgb(82, 179, 101));
            playerColor.Add(8, Color.FromArgb(101, 188, 110));
            playerColor.Add(9, Color.FromArgb(119, 197, 120));
            playerColor.Add(10, Color.FromArgb(151, 212, 147));
            playerColor.Add(11, Color.FromArgb(135, 204, 133));
            playerColor.Add(12, Color.FromArgb(12, 86, 160));
            playerColor.Add(13, Color.FromArgb(21, 98, 169));
            playerColor.Add(14, Color.FromArgb(30, 109, 178));
            playerColor.Add(15, Color.FromArgb(41, 121, 185));
            playerColor.Add(16, Color.FromArgb(53, 133, 191));
            playerColor.Add(17, Color.FromArgb(65, 145, 197));
            playerColor.Add(18, Color.FromArgb(80,155,203));
            playerColor.Add(19, Color.FromArgb(95, 165, 208));
            playerColor.Add(20, Color.FromArgb(110, 175, 214));
            playerColor.Add(21, Color.FromArgb(128, 185, 218));
            playerColor.Add(22, Color.FromArgb(147, 185, 222));
          
        }
    }
}
