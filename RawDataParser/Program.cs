using System;

namespace RawDataParser
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"D:\Work\Football\ParsedByPlayerID\SenzorParsed";
            //DataReader reader = new DataReader(path + "Final.txt");
            //GraphExtractor extractor = new GraphExtractor();//reader.DataList);
            
            
            
            var conv = new TimeConverter();
            conv.ConvertFile(@"D:\Work\Football\Game Interruption\First Half.csv", @"D:\InterruptionsSecondHalf.txt",false);
          
            var builder = new BuildImagesFromData();//reader.DataList);
            //builder.BuildImages(@"D:\Work\Football\Images\");
            //builder.BuildFreqSubgraphImages(path + "Final.txt", @"D:\Graphs3000.txt", @"D:\Work\Football\Images");

            //builder.BuildEdgeHistogram(path + "Final.txt", @"D:\Work\PythonWorkspace\Football\ParsedByPlayerID\EdgeHistogra.txt", @"D:\Work\Football\Images");
            //builder.BuildEdgeHistogram(path + "Final.txt", @"D:\Work\PythonWorkspace\Football\Clusters\Centers5Center4Histo.txt", @"D:\Work\Football\Images");
            /*int centers = 4;
            string result = string.Empty;
            for (int index = 0; index < centers; index++)
            {
                
                 result += builder.BuildClusterImages(path + "Final.txt", @"D:\Work\PythonWorkspace\Football\Clusters\Centers"+centers.ToString()+"Center" + index + ".txt",
                    @"D:\Work\PythonWorkspace\Football\Clusters\Centers"+centers.ToString()+"Center" + index + "Histo.txt", @"D:\Work\Football\Images",index,10);
            }
            using(StreamWriter writer = new StreamWriter(@"D:\Work\Football\Images\ImageNames.txt"))
            {
                writer.WriteLine(result);
                writer.Flush();
                writer.Close();
            }*/
            
            int centers = 4;
            bool multicolor = true;
            for (int center = 0; center < centers; center++)
            {
                builder.BuildClusterDiffImage(path + "Final.txt", @"D:\Work\PythonWorkspace\Football\Clusters\Centers" + centers.ToString() + "Center" + center.ToString() + ".txt",
                        @"D:\Work\PythonWorkspace\Football\Clusters\Centers" + centers.ToString() + "Center" + center.ToString() + "Histo.txt",
                        @"D:\Work\PythonWorkspace\Football\Clusters\Centers" + centers.ToString() + "Statistics.txt", @"D:\Work\Football\Images", center, multicolor);
                if (multicolor == true)
                    break;
            }
          
            
           /* GraphExtractor extractor = new GraphExtractor();
            List<Graph> originalGraphs = extractor.ConvertStringToGraphs(path + "Final.txt");
            originalGraphs = originalGraphs.OrderBy(graph => graph.TimeStamp).ToList();
            double epsilon = 0;
            do
            {
                List<Graph> newGraphs = extractor.FortuneVoronoi(originalGraphs, epsilon);
                newGraphs = newGraphs.OrderBy(graph => graph.TimeStamp).ToList();
                Dictionary<string, List<Graph>> compressed = extractor.ConvertToUniqueGraphs(newGraphs);
                using(System.IO.StreamWriter sw = new StreamWriter(@"D:\EpsResults.txt",true))
                {
                    sw.WriteLine("{0} {1} {2}", epsilon,  newGraphs.Count,  compressed.Count);
                    Console.WriteLine("EpsVal = {0} Choices = {1} UniqueGraphs = {2}", epsilon, newGraphs.Count,  compressed.Count);
                    sw.Flush();
                    sw.Close();
                }
                epsilon += 100;
                extractor.WriteToFile("D:\\Graphs" + epsilon.ToString() + ".txt", compressed);
                
            }
            while (epsilon < 55000);*/
            /*(DataReader reader = new DataReader(path + "Final.txt");
            for(int index = 1; index < 17;index++)
            {
                TrajectoryExtractor traj = new TrajectoryExtractor(reader.DataList as List<List<Info>>);
                traj.WriteToFile(@"D:\Work\PythonWorkspace\Football\Trajectories\Player" + index + "FHF.txt", traj.GetForwardTrajectories(true,index,5));
            }*/
            Console.Write("Done!");
            Console.ReadKey();
        }
    }
}
