using System;
using Accord.Statistics.Analysis;
namespace MachineLearning.PCA
{
    //This class should is meant as an interface between the project and the
    // Accord.NET framework.
    public class PCAWrapper
    {
        readonly double[,] dataMatrix;
        readonly double[,] pcaMatrix;
        readonly bool isCovarianceMatrix;
        public PCAWrapper(double[,] dataMatrix, bool isCovarianceMatrix)
        {
            this.dataMatrix = dataMatrix;
            this.isCovarianceMatrix = isCovarianceMatrix;
        
        }
        public double[,] DataMatrix
        {
            get { return this.dataMatrix; }
        }
        public double[,] CalculatePCA(float maximumInformationLoss)
        {
            if(maximumInformationLoss >= 1 || maximumInformationLoss < 0)
                throw new ArgumentException("The maximum information loss can be set to the interval [0,1)!");
            if (!this.isCovarianceMatrix)
            {
                PrincipalComponentAnalysis pca = new PrincipalComponentAnalysis(this.dataMatrix, AnalysisMethod.Standardize);
                pca.Compute();
                return pca.Transform(dataMatrix, pca.GetNumberOfComponents(1f - maximumInformationLoss));
            }
            else
            {
                double[] mean = Accord.Statistics.Tools.Mean(this.dataMatrix);
                PrincipalComponentAnalysis pca = PrincipalComponentAnalysis.FromCovarianceMatrix(mean, this.dataMatrix);
                pca.Compute();
                return pca.Transform(this.dataMatrix, pca.GetNumberOfComponents(1 - maximumInformationLoss));
            }
        }
    }
}
