using System.Collections.Generic;
using System.Linq;
using MachineLearning.KernelMethods;
using System.IO;
using VISParser.Events;
using VISParser.FieldObjects;
using VISParser.Parsers;

namespace VISParser
{


// ReSharper disable once InconsistentNaming
    public static class VISAPI
    {
        public delegate void Message(string message);
        public delegate void ElementCount(int elementCount);
        public static event ElementCount TargetDataListLoaded = null;
        public const int PlayerCount = 22;
        public static event Message ProgressUpdate = null;

        private static IList<Frame> buildPossession(IList<Frame> data)
        {
            if(VISAPI.ProgressUpdate != null)
            {
                VISAPI.ProgressUpdate.Invoke("Building possesion");
            }
            var parser = new PossessionParser(data as List<Frame>);
            parser.ParsePossession();
            var result = parser.Data as IList<Frame>;
            VISAPI.workComplete();
            return result;
        }
        private static DataReader getReader(string dataPath, int section)
        {
            if (VISAPI.ProgressUpdate != null)
            {
                VISAPI.ProgressUpdate.Invoke("Reading Data!");
            }
            DataReader.GameSection = section;
            var reader = new DataReader(dataPath);
            return reader;
        }
        private static void workComplete()
        {
            if (VISAPI.ProgressUpdate != null)
            {
                VISAPI.ProgressUpdate.Invoke("Process complete!");
            }
        }
        public static IList<Frame> GetPossesionRawData(string dataPath, int section)
        {
            DataReader reader = VISAPI.getReader(dataPath, section);
            return VISAPI.buildPossession(reader.DataList);
        }
        public static void BuildImages(string dataPath, string outputFolderPath, bool parallel, int section, bool includePossesion, bool includeGraphs, VISAPI.Message listener)
        {
            List<Graph> visGraphs = null;
            DataReader reader = VISAPI.getReader(dataPath, section);
            if (VISAPI.TargetDataListLoaded != null)
                VISAPI.TargetDataListLoaded.Invoke(reader.DataList.Count);
            var extractor = new GraphExtractor(reader.DataList);
            if (VISAPI.ProgressUpdate != null)
            {

                VISAPI.ProgressUpdate.Invoke("Extracting Voronoi Graphs! This is a long process!");
            }
            Dictionary<int, BenTools.Mathematics.VoronoiGraph> graphs = extractor.BuildVorGraphs(extractor.ConvertRawDataToGraph(), 0, false);
            if (includePossesion)
            {
                if (VISAPI.ProgressUpdate != null)
                    VISAPI.ProgressUpdate.Invoke("Parsing Possession!");
                VISAPI.buildPossession(reader.DataList);

            }
            if (includeGraphs)
            {
                if (VISAPI.ProgressUpdate != null)
                {
                    VISAPI.ProgressUpdate.Invoke("Extracting Edges! This is a long process!");
                }
                visGraphs = extractor.BuildVISVoronoiGraphs(extractor.ConvertRawDataToGraph(), 0);
            }

            var builder = new ImageBuilder(1050, 680, reader.DataList);

            if (listener != null)
                builder.ImageBuilt += new ImageBuilder.Message(listener);
            builder.BuildImages(graphs, outputFolderPath, visGraphs, parallel);
            VISAPI.workComplete();
        }
        public static void SetSettings(int maxPlayerBallDistance, int maxBallHeight, int maxBallAngle, int stationaryBallLimit)
        {
            EventParser.StationaryBallLimit = stationaryBallLimit;
            PossessionParser.MaxBallHeight = maxBallHeight;
            PossessionParser.MaxAcceptedAngle = maxBallAngle;
            PossessionParser.MaxBallDistance = maxPlayerBallDistance;
        
        }
        public static MatrixWithIds CalculateRBFKernelMatrix(string dataPath, string outputPath, double gamma, int section, KernelComputer<List<double[]>>.Message listener = null)
        {
            DataReader reader = VISAPI.getReader(dataPath, section);
            var data = new SortedDictionary<int, List<double[]>>();
            foreach (var item in reader.DataList)
            {
                if (item.FrameNumber % 100 != 0)
                    continue;
                data.Add(item.FrameNumber, item.PlayerAbsolutePositions);
            }
            var timestamps = new int[data.Count];
            for (int index = 0; index < timestamps.Length; index++)
                timestamps[index] = data.ElementAt(index).Key;
            if (VISAPI.TargetDataListLoaded != null)
                VISAPI.TargetDataListLoaded.Invoke(data.Count);
            KernelFunctions.Gamma = gamma;
            if (VISAPI.ProgressUpdate != null)
            {
                VISAPI.ProgressUpdate.Invoke(string.Format("Calculating RBF Kernel for gamma = {0}!", KernelFunctions.Gamma));
            }
            var kernel = new KernelComputer<List<double[]>>(data, KernelFunctions.RBFKernel, listener);
            if (VISAPI.ProgressUpdate != null)
            {
                VISAPI.ProgressUpdate.Invoke(string.Format("Writing to {0}!", outputPath));
            }

            kernel.WriteToFile(outputPath);
            VISAPI.workComplete();
            return new MatrixWithIds(kernel.KernelMatrix, timestamps);
        }
        public static MatrixWithIds CalculateTanimotoKernelMatrix(string dataPath, string outputPath, int section, Teams team, int queryPointCount = 1024, KernelComputer<ulong[]>.Message listener = null)
        {
            DataReader reader = VISAPI.getReader(dataPath, section);
          //  Dictionary<int, Frame> frameDict = reader.DataList.ToDictionary(item => item.FrameNumber, item => item);
            var field = new Field(queryPointCount);
            var relevantFrames = reader.DataList.Where(item => item.FrameNumber%10 == 0).ToDictionary(item => item.FrameNumber);
            var timestamps = new int[relevantFrames.Count];
            for (int index = 0; index < timestamps.Length; index++)
                timestamps[index] = relevantFrames.ElementAt(index).Key;
            if (VISAPI.TargetDataListLoaded != null)
                VISAPI.TargetDataListLoaded.Invoke(relevantFrames.Count);

            SortedDictionary<int, ulong[]> dominance = field.CalculateDominance(relevantFrames, team, false);
            if (VISAPI.ProgressUpdate != null)
            {
                VISAPI.ProgressUpdate.Invoke(string.Format("Calculating Tanimoto Kernel for {0} query points!", queryPointCount));
            }
            var kernel = new KernelComputer<ulong[]>(dominance, KernelFunctions.Tanimoto, listener);
            if (VISAPI.ProgressUpdate != null)
            {
                VISAPI.ProgressUpdate.Invoke(string.Format("Writing to {0}", outputPath));
            }
            kernel.WriteToFile(outputPath);
            VISAPI.workComplete();
            return new MatrixWithIds(kernel.KernelMatrix, timestamps);
        }
        public static MatrixWithIds CalculateTanimotoKernelMatrixOnGraphs(string dataPath, string outputPath, int section, bool negativeEdgesOnly, KernelComputer<ulong[]>.Message listener = null)
        {
            DataReader reader = VISAPI.getReader(dataPath, section);
            var extractor = new GraphExtractor(reader.DataList);
            List<Graph> graphs = extractor.ConvertRawDataToGraph();
            extractor.BuildVISVoronoiGraphs(graphs);
            var graphEdgeCoding = new SortedDictionary<int, ulong[]>();
            foreach (var graph in graphs)
            {
                if (graph.TimeStamp % 10 != 0)
                    continue;
                graphEdgeCoding.Add(graph.TimeStamp, graph.EdgeVector(negativeEdgesOnly));
            }
            var timestamps = new int[graphEdgeCoding.Count];
            for (int index = 0; index < timestamps.Length; index++)
                timestamps[index] = graphEdgeCoding.ElementAt(index).Key;
            if (VISAPI.TargetDataListLoaded != null)
                VISAPI.TargetDataListLoaded.Invoke(graphEdgeCoding.Count);
            if (VISAPI.ProgressUpdate != null)
            {
                VISAPI.ProgressUpdate.Invoke(string.Format("Calculating Tanimoto Kernel for graph edges!"));
            }
            var kernel = new KernelComputer<ulong[]>(graphEdgeCoding, KernelFunctions.Tanimoto, listener);
            if (VISAPI.ProgressUpdate != null)
            {
                VISAPI.ProgressUpdate.Invoke(string.Format("Writing to {0}!", outputPath));
            }
            kernel.WriteToFile(outputPath);
            VISAPI.workComplete();
            return new MatrixWithIds(kernel.KernelMatrix, timestamps);
        }
        public static SortedList<int, ClusterPoint> ClusterData(string outputDestination, MatrixWithIds dataMatrix, bool isCovarianceMatrix, float maximumInformationLoss, int numberOfClusters)
        {
            var result = new SortedList<int, ClusterPoint>();
            var pca = new MachineLearning.PCA.PCAWrapper(dataMatrix.Matrix, isCovarianceMatrix);
            double[,] transformedMatrix = pca.CalculatePCA(maximumInformationLoss);
            var kmeans = new MachineLearning.Clustering.KMeansWrapper(transformedMatrix);
            int[] labels = kmeans.CalculateClusters(numberOfClusters);
            for (int row = 0; row < transformedMatrix.GetLength(0); row++)
            {
                var newRow = new double[transformedMatrix.GetLength(1)];
                for (int col = 0; col < transformedMatrix.GetLength(1); col++)
                    newRow[col] = transformedMatrix[row, col];
                var newPoint = new ClusterPoint(newRow, labels[row], dataMatrix.Timestamps[row]);
                result.Add(dataMatrix.Timestamps[row], newPoint);
            }
            using (var sw = new StreamWriter(outputDestination))
            {
                foreach (var item in result)
                    sw.WriteLine(item.Value.ToString());
            }
            return result;
        }
        public static IList<FootballEvent> GetEvents(IList<Frame> data)
        {
            if (VISAPI.ProgressUpdate != null)
            {
                VISAPI.ProgressUpdate.Invoke("Calculating pass data");
            }
            var parser = new EventParser(data);
            VISAPI.workComplete();
            return parser.ParseEvents();
        }
    }
}
