using System;
using System.Globalization;
using System.Text;

namespace VISParser
{
    public sealed class ClusterPoint
    {
        double[] data;
        public int ClusterId { private set; get; }
        public int ImageId { private set; get; }
        public ClusterPoint(double[] data, int ClusterId, int ImageId)
        {
            this.data = data;
            this.ClusterId = ClusterId;
            this.ImageId = ImageId;
        }
        public ClusterPoint(string dataString)
        {
            string[] split = dataString.Split(' ');
            if (split.Length < 5)
                throw new ArgumentException("The argument data string is not of correct format");
            this.data = new double[split.Length - 2];
            for (int index = 0; index < data.Length; index++)
            {
                this.data[index] = double.Parse(split[index], System.Globalization.CultureInfo.InvariantCulture);
            }
            this.ClusterId = int.Parse(split[split.Length - 2]);
            this.ImageId = int.Parse(split[split.Length - 1]);

        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (double item in this.data)
            {
                sb.Append(item + " ");
            }
            sb.Append(this.ClusterId.ToString(CultureInfo.InvariantCulture) + " ");
            sb.Append(this.ImageId.ToString(CultureInfo.InvariantCulture));
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
    }
}
