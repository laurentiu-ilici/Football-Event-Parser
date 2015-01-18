using System;
using System.Text;

namespace FootballProject
{
    [Serializable]
    public sealed class RBFKernelRunSettings:RunSettings
    {
        public double Gamma { get; set; }
        public override string ToString()
        {
            var sb = new StringBuilder("Type=RBFKernel" + Environment.NewLine);
            sb.Append(base.ToString());
            sb.Append("Gamma=" + this.Gamma.ToString(System.Globalization.CultureInfo.InvariantCulture) + Environment.NewLine);
            return this.ToString();
        }
    }
}
