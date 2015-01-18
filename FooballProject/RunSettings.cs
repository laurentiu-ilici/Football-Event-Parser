using System;
using System.Text;

namespace FootballProject
{
    [Serializable]
    public abstract class RunSettings
    {
        public string OutputPath { get; set; }
        public int Period { get; set; }
        public string DataPath { get; set; }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("DataFile=" + this.DataPath + Environment.NewLine);
            sb.Append("DataOuput=" + this.OutputPath + Environment.NewLine);
            sb.Append("Period=" + this.Period.ToString() + Environment.NewLine);
            return sb.ToString();
        }
    }
}
