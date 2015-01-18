using System;
using System.Text;
using VISParser;
using VISParser.FieldObjects;

namespace FootballProject
{
    [Serializable]
    public sealed class TanimotoKernelRunSettings : RunSettings
    {
        public Teams Team {get;set;}
        public int QueryPointNumber{get;set;}
        public bool NegativeEdgesOnly { get; set; }
        public bool OnGraphs { get; set; }
        public override string ToString()
        {
            var sb = new StringBuilder("Type=RBFKernel" + Environment.NewLine);
            sb.Append(base.ToString());
            sb.Append("Team=" + this.Team.ToString() + Environment.NewLine);
            sb.Append("QueryPoints=" + this.QueryPointNumber.ToString() + Environment.NewLine);
            sb.Append("NegativeEdgesOnly=" + this.NegativeEdgesOnly.ToString() + Environment.NewLine);
            sb.Append("OnGraphs=" + this.OnGraphs.ToString() + Environment.NewLine);
            return this.ToString();
        }
    }
}
