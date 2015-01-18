using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Drawing;

namespace RawDataParser
{
    class BuildImagesFromData
    {
        readonly IList<List<Info>> dataList = new List<List<Info>>();
        readonly List<long> problemsIds = new List<long>();
        Dictionary<int, Color> edgeColorScheme = new Dictionary<int, Color>();
        readonly Dictionary<int, Color> clusterColorDict = new Dictionary<int, Color>();
        private void init()
        {
         
            this.edgeColorScheme = new Dictionary<int, Color>
            {
                {0, Color.Black},
                {1, Color.Red},
                {2, Color.Blue},
                {3, Color.White},
                {4, Color.Orange}
            };
            this.clusterColorDict.Add(0, Color.FromArgb(141, 211, 199));
            this.clusterColorDict.Add(1, Color.FromArgb(255, 255, 179));
            this.clusterColorDict.Add(2, Color.FromArgb(190, 186, 218));
            this.clusterColorDict.Add(3, Color.FromArgb(251, 128, 114));
            this.clusterColorDict.Add(4, Color.FromArgb(128, 177, 211));
        }
        public BuildImagesFromData(IList<List<Info>> dataList = null, List<long> problemsIds = null)
        {
            if (dataList != null)
                this.dataList = dataList;
            if (problemsIds != null)
                this.problemsIds = problemsIds;
            this.init();
        }
        public void BuildEdgeHistogram(string dataPath, string histogramPath, string outputFolderPath)
        {
            var reader = new DataReader(dataPath, 1);
            var graph = new Graph(reader.DataList[0], reader.ReadEdgeHistogram(histogramPath), reader.DataList[0][0].TimeStamp);
            this.buildImage(graph.Nodes, outputFolderPath, graph.Edges);
        }
        public void BuildClusterDiffImage(string dataPath, string clusterFilePath, string clusterHistogramPath, string statisticsFilePath, string outputFolderPath, int targetCenter, bool multiColor = false)
        {
            var extractor = new GraphExtractor();
            List<Graph> graphs = extractor.ConvertStringToGraphs(dataPath, clusterFilePath);
            var edges = new Dictionary<string, Coords>();
            var stats = new Dictionary<string, Coords>();
            var reader = new DataReader(dataPath, 0);
            List<Coords> rawFrequncy = reader.ReadEdgeHistogram(clusterHistogramPath);
            List<Coords> rawStatistics = reader.ReadEdgeHistogram(statisticsFilePath, true);
            foreach (var item in rawFrequncy)
                edges.Add(item.ToString(), item);
            foreach (var item in rawStatistics)
                stats.Add(item.ToString(), item);
            foreach (var item in edges)
            {
                if (multiColor)
                {
                    item.Value.StrongestCluster = stats[item.Value.ToString()].StrongestCluster;
                    item.Value.Frequency = stats[item.Value.ToString()].Robustness;
                }
                else
                    item.Value.Frequency = (stats[item.Value.ToString()].Frequency - item.Value.Frequency);/// stats[item.Value.ToString()].Robustness;
                
            }

            var mean = new List<Info>();
            for (int pid = 0; pid < 18; pid++)
            {
                Info meanInfo = null;
                foreach (var item in graphs)
                {
                    if (meanInfo == null)
                    {
                        meanInfo = new Info();
                        meanInfo = meanInfo.CopyObject(item.Nodes[pid]);
                    }
                    else
                    {
                        for (int index = 0; index < meanInfo.Data.Length; index++)
                            meanInfo[index] += item.Nodes[pid][index];
                    }
                }
                for (int index = 0; index < meanInfo.Data.Length; index++)
                    meanInfo[index] /= graphs.Count;
                mean.Add(meanInfo);
            }
            foreach (var item in mean)
                item.TimeStamp = graphs[0].Nodes[0].TimeStamp;
            this.buildImage(mean, outputFolderPath, rawFrequncy, -1, 5, "Center" + targetCenter.ToString() + (multiColor == true ? "MultiColor" : ""),multiColor, targetCenter);

        }
        public string BuildClusterImages(string dataPath, string clusterGraphPath, string clusterHistogramPath, string outputFolderPath, int center, int pictureLimit = 10)
        {
            var sb = new StringBuilder();
            var extractor = new GraphExtractor();
            List<Graph> graphs = extractor.ConvertStringToGraphs(dataPath, clusterGraphPath);
            var frequency = new Dictionary<string, double>();
            var reader = new DataReader(dataPath, 0);
            List<Coords> rawFrequency = reader.ReadEdgeHistogram(clusterHistogramPath);
            foreach (var item in rawFrequency)
                frequency.Add(item.ToString(), item.Frequency);
            var rand = new Random();
            int startIndex = rand.Next(graphs.Count - pictureLimit - 1 > 0 ? graphs.Count - 1 - pictureLimit : 1);
            startIndex = startIndex < graphs.Count ? startIndex : 0;
            List<Graph> toBuild = graphs.GetRange(startIndex, startIndex + pictureLimit < graphs.Count ? pictureLimit : graphs.Count - 1 - startIndex);
            System.IO.Directory.CreateDirectory(outputFolderPath + "\\Center" + center.ToString(CultureInfo.InvariantCulture));
            foreach (var graph in graphs)
            {
                foreach (var edge in graph.Edges)
                {
                    edge.Frequency = frequency[edge.ToString()];
                }
                sb.Append(this.buildImage(graph.Nodes, outputFolderPath + "\\Center" + center + "\\", graph.Edges, -1, 5, "Center" + center.ToString(CultureInfo.InvariantCulture),false,center));
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
        public void BuildFreqSubgraphImages(string dataPath, string graphStringPath, string outputFolderPath)
        {
            var extractor = new GraphExtractor();
            List<Graph> graphs = extractor.ConvertStringToGraphs(dataPath, graphStringPath);
            foreach (var item in graphs)
                this.buildImage(item.Nodes, outputFolderPath, item.Edges, item.OccurenceCount);
        }
        private int distance(Point a, Point b)
        {
            return (int)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }
        public void BuildImages(string outputFolderPath)
        {

            foreach (var item in this.dataList)
            {
                if (this.problemsIds.Count != 0 && this.problemsIds.Contains(item[item.Count - 1].TimeStamp))
                {
                    this.buildImage(item, outputFolderPath);
                    Console.WriteLine("Problems with image {0} solved", item[item.Count - 1].TimeStamp);
                }
                else if (problemsIds.Count == 0)
                    this.buildImage(item, outputFolderPath);

            }
        }
        private string buildImage(List<Info> data, string outputFolderPath, List<Coords> edges = null, int confidence = -1, int edgeClusters = 5, string suffix = "", bool multiColor = false,int targetCenter = -1)
        {
            var bitmap = new System.Drawing.Bitmap(524, 678);
            Graphics g = Graphics.FromImage(bitmap);
            var brush = new SolidBrush(Color.White);
            g.FillRectangle(brush, 0, 0, bitmap.Width, bitmap.Height);
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    bitmap.SetPixel(x, y, this.calculateColor(new Point(x, y - 339), data));
                }
            }
            foreach (var item in data)
            {

                if (item[0] == -1)
                    continue;
                if (item[0] != 0)
                {
                    brush = new SolidBrush(Color.Black);
                    var dotRect = new Rectangle((int)(item[2] / 100), (int)(item[3] / 100 + 339), 3, 3);
                    var strRect = new Rectangle((int)(item[2] / 100), (int)(item[3] / 100 + 339), 20, 20);
                    g.FillRectangle(new SolidBrush(Color.Black), dotRect);
                    g.DrawString(item[0].ToString(), new Font("Tahoma", 8), brush, strRect);

                }
                else
                {

                    brush = new SolidBrush(item.PlayerColor);
                    var itemRect = new Rectangle((int)(item[2] / 100), (int)(item[3] / 100 + 339), 6, 6);
                    g.FillEllipse(brush, itemRect);

                }
            }
            if (edges != null)
            {

                double max = edges.Max(edge => edge.Frequency);
                double min = edges.Min(edge => edge.Frequency);
                max = Math.Max(Math.Abs(max), Math.Abs(min));
                foreach (var item in edges)
                {
                    if (!item.IsFrequencySet)
                    {
                        g.DrawLine(Pens.Black, (int)data[item.Row + 1][2] / 100, (int)(data[item.Row + 1][3] / 100 + 339), (int)data[item.Col + 1][2] / 100, (int)(data[item.Col + 1][3] / 100 + 339));
                        var strRect = new Rectangle(0, 0, 100, 20);
                        if (confidence != -1)
                            g.DrawString(confidence.ToString() + "/" + "13100", new Font("Tahoma", 8), brush, strRect);
                    }
                    else
                    {
                        //TODO: Warning if true will be removed :D
                        if (true || item.Label == 1 && item.Team == 0)
                        {
                            Pen pen;
                            if (multiColor == false)
                            {

                                pen = new Pen(item.Frequency > 0 ? Color.FromArgb((int)(255 * item.Frequency * 1 / max), Color.Black) :
                                                                    Color.FromArgb((int)(255 * Math.Abs(item.Frequency) * 1 / max), Color.Red), 3);
                            }
                            else
                            {
                                pen = new Pen(Color.FromArgb((int)(255 * item.Frequency * 1 / max), this.edgeColorScheme[item.StrongestCluster]), 3);

                            }
                            g.DrawLine(pen, (int)data[item.Row + 1][2] / 100, (int)(data[item.Row + 1][3] / 100 + 339), (int)data[item.Col + 1][2] / 100, (int)(data[item.Col + 1][3] / 100 + 339));
                            
                           
                        }
                    }
                }
                var strRect2 = new Rectangle(0, 0, 100, 20);
                if (confidence != -1)
                    g.DrawString(confidence.ToString() + "/" + "13100", new Font("Tahoma", 8), brush, strRect2);
                if (targetCenter != -1)
                {
                    brush.Color = this.clusterColorDict[targetCenter];
                    g.DrawString("Center " + targetCenter.ToString(), new Font("Tahoma", 8), brush, strRect2);
                }
            }
            bitmap.Save(outputFolderPath + "\\image" + data[0].TimeStamp.ToString() + suffix + ".bmp");
            Console.WriteLine("Done building image {0}", data[0].TimeStamp);
            return data[0].TimeStamp.ToString() + ",pictures/Center" + targetCenter.ToString() +"/image" + data[0].TimeStamp.ToString() + suffix + ".bmp";
        }

        private Color calculateColor(Point target, List<Info> data)
        {
            int minDistance = int.MaxValue;
            int minDistIndex = -1;

            //0 Is the ball , we are not interested in that at the moment 
            //17 is the referee we don't care about him either at this moment
            for (int index = 1; index < data.Count; index++)
            {
                if (data[index][0] == 17) continue;
                var curPoint = new Point((int)data[index][2] / 100, (int)data[index][3] / 100);
                int curDist = this.distance(target, curPoint);
                if (minDistance > curDist)
                {
                    minDistance = curDist;
                    minDistIndex = index;
                }
            }
            return data[minDistIndex].PlayerColor;
        }

    }

}
