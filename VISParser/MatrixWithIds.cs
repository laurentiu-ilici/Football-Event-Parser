using System;
using System.Collections.Generic;

namespace VISParser
{

    //Wrapper such that I can keep the Timestamps of the data records.
    public sealed class MatrixWithIds
    {
        double[,] matrix;
        int[] ids;
        public MatrixWithIds(List<String> lines)
        {
            if (lines.Count < 1)
                throw new ArgumentException("No data was given!");
            string[] split = lines[0].Split(':');
            this.matrix = new double[lines.Count, split[0].Trim().Split(' ').Length];
            this.ids = new int[lines.Count];
            for (int row = 0; row < matrix.GetLength(0); row++)
            {
                split = lines[row].Split(':');
                if(split.Length != 2)
                    throw new ArgumentException("Invalid matrix data give");
                this.ids[row] = int.Parse(split[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                string [] dataSplit = split[0].Trim().Split(' ');
                for (int col = 0; col < matrix.GetLength(1); col++)
                {
                    this.matrix[row, col] = double.Parse(dataSplit[col], System.Globalization.CultureInfo.InvariantCulture);
                }
            }
        }
        public MatrixWithIds(double[,] matrix, int[] ids)
        {
            this.matrix = matrix;
            this.ids = ids;
        }
        public double[,] Matrix
        {
            get { return this.matrix; }
        }
        public int[] Timestamps
        {
            get 
            {
                return this.ids;
            }
        }


    }
}
