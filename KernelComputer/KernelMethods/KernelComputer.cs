using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineLearning.KernelMethods
{
    public class KernelComputer<T>
    {

        public delegate double KernelFunction(T featuresX, T featuresY);
        private KernelFunction kernelFunction;
        public delegate void Message(string message);
        public event Message RowBuilt;
        SortedDictionary<int, T> data;
        double[,] kernelMatrix;
        public KernelComputer(SortedDictionary<int, T> data, KernelFunction kernelFucntion, Message listener=null)
        {
            this.kernelMatrix = new double[data.Count, data.Count];
            this.data = data;
            this.kernelFunction = kernelFucntion;
            if (listener != null)
                this.RowBuilt += listener;
            this.ConstructKernelMatrix();
        }
        public void ConstructKernelMatrix()
        {
            for (int row = 0; row < kernelMatrix.GetLength(0); row++)
            {
                //Take advantage of the symetry
                for (int col = row; col < kernelMatrix.GetLength(1); col++)
                {
                    //The "y" features are processed on the fly
                    double kernelValue = this.kernelFunction.Invoke(this.data.ElementAt(row).Value, this.data.ElementAt(col).Value);
                    kernelMatrix[row, col] = kernelValue;
                    kernelMatrix[col, row] = kernelValue;
                }
                if (this.RowBuilt != null)
                {
                    this.RowBuilt.Invoke(string.Format("Done building row {0} of {1}...",row,kernelMatrix.GetLength(0)));
                }
            }
        }
        public double[,] KernelMatrix
        {
            get
            {
                return this.kernelMatrix;
            }
        }
        public void WriteToFile(string outputPath)
        {
            using (System.IO.StreamWriter wr = new System.IO.StreamWriter(outputPath))
            {
                for (int row = 0; row < this.kernelMatrix.GetLength(0); row++)
                {
                    StringBuilder builder = new StringBuilder();
                    for (int col = 0; col < this.kernelMatrix.GetLength(1); col++)
                    {
                        builder.Append(((float)(this.kernelMatrix[row, col])).ToString(System.Globalization.CultureInfo.InvariantCulture) + " ");
                    }
                    builder.Append(" : " + this.data.ElementAt(row).Key.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    wr.WriteLine(builder.ToString());
                }
                wr.Flush();
                wr.Close();
            }
        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int row = 0; row < this.kernelMatrix.GetLength(0); ++row)
            {
                for (int col = 0; col < this.kernelMatrix.GetLength(1); ++col)
                {
                    builder.Append(this.kernelMatrix[row, col].ToString(System.Globalization.CultureInfo.InvariantCulture) + " ");
                }
                builder.Append(" : " + this.data.ElementAt(row).Key.ToString(System.Globalization.CultureInfo.InvariantCulture));
                builder.Append('\n');
            }
            return builder.ToString();
        }
    }
}
