using Accord.MachineLearning;
namespace MachineLearning.Clustering
{
    public class KMeansWrapper
    {
        private readonly double[][] dataMatrix;
        public KMeansWrapper(double[,] dataMatrix)
        {
            this.dataMatrix = new double[dataMatrix.GetLength(0)][];
            for(int row = 0 ; row < dataMatrix.GetLength(0);row++)
            {
                this.dataMatrix[row] = new double[dataMatrix.GetLength(1)];
                for(int col = 0 ; col < this.dataMatrix[row].GetLength(0);col++)
                    this.dataMatrix[row][col] = dataMatrix[row,col];
            }
        }

        public int[] CalculateClusters(int nrOfClusters)
        {
            KMeans kmeans = new KMeans(nrOfClusters);
            int[] labels = kmeans.Compute(this.dataMatrix);
            return labels;
        }
    }
}
