using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace RawDataParser
{
    
        
    public class Info
    {
        Dictionary<int, Color> playerColor = new Dictionary<int, Color>();
        public long[] Data{ get; set;}
        private void init()
        {
            this.Data = new long[13];
            playerColor.Add(-1, Color.White);
            playerColor.Add(0, Color.Red);
            playerColor.Add(2, Color.FromArgb(0,90,60));
            playerColor.Add(4, Color.FromArgb(0, 190, 60));
            playerColor.Add(6, Color.FromArgb(0, 110, 60));
            playerColor.Add(8, Color.FromArgb(0, 150, 60));
            playerColor.Add(10, Color.FromArgb(0, 170, 60));
            playerColor.Add(12, Color.FromArgb(0, 130, 60));
            playerColor.Add(14, Color.FromArgb(0, 210, 60));
            playerColor.Add(16, Color.FromArgb(0, 250, 60));
            playerColor.Add(1, Color.FromArgb(0, 60, 110));
            playerColor.Add(3, Color.FromArgb(100, 50, 210));
            playerColor.Add(5, Color.FromArgb(20, 60, 130));
            playerColor.Add(7, Color.FromArgb(40, 60, 150));
            playerColor.Add(9, Color.FromArgb(60, 60, 190));
            playerColor.Add(11, Color.FromArgb(80, 60, 170));
            playerColor.Add(13, Color.FromArgb(100, 100, 230));
            playerColor.Add(15, Color.FromArgb(120, 75, 255));
            playerColor.Add(17, Color.LightGreen);
        }
        public Info()
        {
            init();
        }
        public Info(long [] info)
        {
            init();
            this.Data = info;
          

        }
        public Info(string data)
        {
            init();
            string[] split = data.Split(',');
            this.Data = new long[split.Length];
            for (int index = 0; index < split.Length; index++)
                this.Data[index] = long.Parse(split[index]);

        }
        public long TimeStamp { get { return this[1]; } set { this[1] = value; } }
        public bool IsOnField()
        {
            if (this.Data[2] < 0 || this.Data[2] > 52483
                || this.Data[3] < -33960 ||
                this.Data[3] > 33965)
                return false;
            return true;
        }
        public Info CopyObject(Info other)
        {
            var newData = new long[other.Data.Length];
            for (int index = 0; index < newData.Length; index++)
                newData[index] = other[index];
            var result = new Info(newData);
            return result;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (long item in this.Data)
                sb.Append(item.ToString() + ",");
            return sb.ToString().Trim(',');
        }
        public Color PlayerColor{get{return this.playerColor[(int)this[0]];}}
        public  long this[int index]
        {
           get{ return this.Data[index];}
            set { this.Data[index] = value; }
        }
        public double Distance(Info other)
        { 
            return Math.Sqrt(Math.Pow(this.Data[2] - other.Data[2],2) + Math.Pow(this.Data[3] - other.Data[3],2));
        }
        public Info Average(Info other)
        {
            var result = new long[this.Data.Length];
            for (int index = 0; index < result.Length; index++)
            {
                if (index == 1)
                    continue;
                result[index] = this.Data[index] + other.Data[index];
                result[index] /= 2;
            
            }
            return new Info(result);
        }
    }
}
