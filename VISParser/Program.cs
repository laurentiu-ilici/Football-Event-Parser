using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MachineLearning.KernelMethods;
using VISParser.FieldObjects;
using VISParser.Parsers;
using System.Text;

namespace VISParser
{
    public class Program
    {
        public delegate void Message(string message);
        
        static void Main(string[] args)
        {

            Console.WriteLine("Reading Data");
            string dataPath = @"E:\Work\Football\2011-12 BL 17.Sp. K'lautern vs. Hannover\130375.pos";

           // Program.OutputEpsilonResults(dataPath, 0, 100);
            //Program.BuildFieldDataImages(dataPath,Teams.Home);
            //Program.CalculateTanimotoKernelMatrix(dataPath, Teams.Home);
            //Program.CalculateRBFKernelMatrix(dataPath);
            //Program.BuildClusterDominanceImages(dataPath, Teams.Home);
            //List<Frame> Data = Program.TestPosesion(dataPath);
            Program.CalculateKernelMatrixFromGraphStrings(dataPath);
            Console.WriteLine("Starting build process");
            //Program.FortuneVoronoi();
            DataReader reader = new DataReader(dataPath);
          //  GraphExtractor extractor = new GraphExtractor(reader.DataList);
           // Dictionary<int,BenTools.Mathematics.VoronoiGraph> graphs = extractor.BuildVorGraphs(extractor.ConvertRawDataToGraph(),0,false);
            ImageBuilder builder = new ImageBuilder(1050, 680, reader.DataList);
            Program.BuildClusterImages(dataPath, builder, 4);
            //builder.BuildImages(graphs,@"E:\Work\Football\Images\KeiserTest");
            //string result = builder.BuildFreqSubgraphImages(dataPath, @"D:\Work\PythonWorkspace\Football\VISData\Graphs10.txt", @"D:\Work\Football\Images\FrequentSubgraphs");
            //Program.BuildClusterDiffImages(dataPath,builder,4,false);
            Console.WriteLine("Done!");
            Console.ReadKey();
        }

    
        public static void BuildImages(string dataPath, string outputFolderPath, Program.Message listener=null)
        { 
            var reader = new DataReader(dataPath,500);
            var extractor = new GraphExtractor(reader.DataList);
            Dictionary<int,BenTools.Mathematics.VoronoiGraph> graphs = extractor.BuildVorGraphs(extractor.ConvertRawDataToGraph(),0,false);
            var builder = new ImageBuilder(1050, 680, reader.DataList);
            if(listener!=null)
                builder.ImageBuilt+=new ImageBuilder.Message(listener);
            builder.BuildImages(graphs, outputFolderPath);
        }

        public static void BuildFieldDataImages(string dataPath, Teams team, string outputFolderPath = @"D:\Work\Football\Images\KeiserTest")
        { 
            if(team == Teams.None)
                throw new ArgumentException("The dominance needs to be calculated for a specific team! Teams.None is not a valid value");
            var reader= new DataReader(dataPath);
            var data = reader.DataList;
            Dictionary<int,Frame> dataDict = data.ToDictionary(item => item.FrameNumber,item=> item);
            var field = new Field(2000);
            Console.WriteLine("Building Dominance");
            var result = new SortedDictionary<int, ulong[]>();
            result = field.CalculateDominance(dataDict, team);
            Console.WriteLine("Building Pictures");
            var builder = new ImageBuilder(1050,680);
            builder.BuildDominanceImages(result, dataDict, field.QueryPointGrid, outputFolderPath, team);
        }
        public static void CalculateRBFKernelMatrix(string dataPath, string outputPath = @"D:\RBFKernelMatrix.txt")
        {
            var reader = new DataReader(dataPath);
            var data = new SortedDictionary<int, List<double[]>>();
            foreach (var item in reader.DataList)
            {
                if (item.FrameNumber % 45 != 0)
                    continue;
                data.Add(item.FrameNumber, item.PlayerAbsolutePositions);
            }
            KernelFunctions.Gamma = 100;
            var kernel = new KernelComputer<List<double[]>>(data, KernelFunctions.RBFKernel);
            kernel.WriteToFile(outputPath);
        
        }
       
        public static void CalculateTanimotoKernelMatrix(string dataPath, Teams team, string outputPath = @"D:\KernelMatrix.txt")
        {
            var reader = new DataReader(dataPath);
            Dictionary<int,Frame> frameDict = reader.DataList.ToDictionary(item=>item.FrameNumber,item => item);
            List<int> relevantEntries = reader.ReadTimestampsFromGraphStrings();
            var field = new Field();
            var relevantFrames = new Dictionary<int, Frame>();
            foreach (var item in relevantEntries)
            {
                if (frameDict.ContainsKey(item))
                    relevantFrames.Add(item, frameDict[item]);
                else
                    Console.WriteLine("The entry was not found");
            }
            SortedDictionary<int, ulong[]> dominance = field.CalculateDominance(relevantFrames, Teams.Home, false);
            var kernel = new KernelComputer<ulong[]>(dominance,KernelFunctions.Tanimoto);
            using (var wr = new StreamWriter(outputPath))
            {
                wr.WriteLine(kernel.ToString());
                wr.Flush();
                wr.Close();
            } 
        }
        public static void CalculateKernelMatrixFromGraphStrings(string dataPath, string outputPath = @"D:\KernelMatrixFromGraphsNegativeLabelsOnlyOnlyFirstTanimotoTerm.txt")
        {
            var extractor = new GraphExtractor();
            List<Graph>  graphs =  extractor.ConvertStringToGraphs(dataPath);
            var graphEdgeCoding = new SortedDictionary<int, ulong[]>();
            foreach (var graph in graphs)
            {
                graphEdgeCoding.Add(graph.TimeStamp, graph.EdgeVector(true));
            }
            var kernel = new KernelComputer<ulong[]>(graphEdgeCoding, KernelFunctions.Tanimoto);
            using (var wr = new StreamWriter(outputPath))
            {
                wr.WriteLine(kernel.ToString());
                wr.Flush();
                wr.Close();
            }
        }
        
        public static void BuildClusterImages(string dataPath, ImageBuilder builder, int centers)
        {
            string result = string.Empty;
            var reader = new DataReader(dataPath);
            var extractor = new GraphExtractor(reader.DataList);
            Dictionary<int, BenTools.Mathematics.VoronoiGraph> vorGraphs = extractor.BuildVorGraphs(extractor.ConvertRawDataToGraph(), 0,false);
            for (int index = 0; index < centers; index++)
            {
                result += builder.BuildClusterImages(vorGraphs, dataPath, @"D:\PythonWorkspace\Football\Clusters\Centers" + centers.ToString() + "Center" + index + ".txt",
                   @"D:\PythonWorkspace\Football\Clusters\Centers" + centers.ToString() + "Center" + index + "Histo.txt", @"D:\PythonWorkspace\Football\ClusterImages", index, 10);
            }
            using (var writer = new StreamWriter(@"D:\ImageNames.txt"))
            {
                writer.WriteLine(result);
                writer.Flush();
                writer.Close();
            }

        }
        public static void OutputEpsilonResults(string dataPath, int minEps, int maxEps)
        {
            Console.WriteLine("Reading Data");
            var reader = new DataReader(dataPath);
            var extractor = new GraphExtractor(reader.DataList);
            StringBuilder sb = new StringBuilder();
            for (int epsilon = minEps; epsilon < maxEps; epsilon+=5)
            {
                List<Graph> voronoi = extractor.BuildVISVoronoiGraphs(extractor.ConvertRawDataToGraph(), epsilon);
                Dictionary<string, List<Graph>> result = extractor.ConvertToUniqueGraphs(voronoi);
                sb.Append(string.Format("Eps={0};Count={1};UniqueGraphs={2}", epsilon,voronoi.Count, result.Count));
                sb.Append(Environment.NewLine);
            }
            File.WriteAllLines("E:\\EpsGraphsNumber.txt", new List<string> { sb.ToString() });
        }

        public static void FortuneVoronoi(double epsilon = 10)
        {
            Console.WriteLine("Reading Data");
            string dataPath = @"E:\Work\Football\2011-12 BL 17.Sp. K'lautern vs. Hannover\130375.pos";
            var reader = new DataReader(dataPath);
            var extractor = new GraphExtractor(reader.DataList);
            List<Graph> voronoi = extractor.BuildVISVoronoiGraphs(extractor.ConvertRawDataToGraph(), epsilon);
            Dictionary<string, List<Graph>> result = extractor.ConvertToUniqueGraphs(voronoi);
            extractor.WriteToFile("D:\\Graphs" + epsilon.ToString() + ".txt", result);
            extractor.WriteGraphStrenghtStrings("D:\\Graphs" + epsilon.ToString() + "Strengths.txt", result);

        }
        public static void CopyPhases(List<GamePhase> gamePhases)
        {
            string sourceFolder = @"D:\Work\Football\Images\ColoredVersionOfFirstHalfFrequentSubgraphs";
            string destFolder = @"D:\Work\Football\Images\Game Phase";
            for (int index = 0; index < gamePhases.Count; index++)
            {
                string destPhaseFolder = Path.Combine(destFolder, "P_" + index.ToString() + "_" +
                    gamePhases[index].FrameCount.ToString() + "_" +
                    gamePhases[index].Current.ToString() + "_" + gamePhases[index].Successor.ToString());
                if (!Directory.Exists(destPhaseFolder))
                {
                    Directory.CreateDirectory(destPhaseFolder);
                }

                foreach (var frame in gamePhases[index].Frames)
                {
                    string sourceFile = Path.Combine(sourceFolder, "image" + frame.FrameNumber.ToString() + ".bmp");
                    string destFile = Path.Combine(destPhaseFolder, "image" + frame.FrameNumber.ToString() + ".bmp");
                    File.Copy(sourceFile, destFile, true);
                }
            }
        }
    }
}
