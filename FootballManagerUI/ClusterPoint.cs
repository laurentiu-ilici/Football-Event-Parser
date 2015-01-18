using System;
using System.Text;

namespace FootballManagerUI
{
    public class ClusterPoint
    {
        static ClusterColors Colors = new ClusterColors();
        public static string ImageFolderPath = string.Empty;
        double[] data; 
        public int ClusterId { private set; get; }
        public int ImageId {  set; get; }
        public int RealId { private set; get; }
        public ClusterPoint(double[] data, int ClusterId,int RealId, int ImageId)
        {
            this.data = data;
            this.ClusterId = ClusterId;
            this.ImageId = ImageId;
            this.RealId = RealId;
        }
        public ClusterPoint(string dataString)
        {
            string[] split = dataString.Split(' ');
            if (split.Length < 5)
                throw new ArgumentException("The argument data string for the cluster point, is not of correct format");
            this.data = new double[split.Length-2];
            for (int index = 0; index < data.Length; index++)
            {
                this.data[index] = double.Parse(split[index]);
            }
            this.ClusterId = int.Parse(split[split.Length - 2], System.Globalization.CultureInfo.InvariantCulture);
            this.ImageId = int.Parse(split[split.Length - 1], System.Globalization.CultureInfo.InvariantCulture);
            this.RealId = int.Parse(split[split.Length - 1], System.Globalization.CultureInfo.InvariantCulture);

        }
        public System.Windows.Media.Color ClusterColor
        {
            get
            {
                return ClusterPoint.Colors.GetColor(this.ClusterId);
            }
        }
        public string ToImageString()
        {
            if (ImageFolderPath == string.Empty)
                throw new ArgumentException("The path to the folder containing the images, was not correctly set");
            var sb = new StringBuilder(ClusterPoint.ImageFolderPath + "\\");
            sb.Append("image");
            sb.Append(this.ImageId.ToString(System.Globalization.CultureInfo.InvariantCulture));
            sb.Append(".bmp");
            return sb.ToString();
        }
        public double this[int index]
        { 
            get
            {
                if (index > this.data.GetLength(0) - 1)
                    throw new ArgumentOutOfRangeException();
                return this.data[index];
            }
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (double item in this.data)
            {
                sb.Append(item + " ");
            }
            sb.Append(this.ClusterId.ToString(System.Globalization.CultureInfo.InvariantCulture) + " ");
            sb.Append(this.ImageId.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return sb.ToString();
        }
    }
}
