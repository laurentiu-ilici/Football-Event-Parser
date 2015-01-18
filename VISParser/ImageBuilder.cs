using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Drawing;
using BenTools.Mathematics;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using VISParser.FieldObjects;
using VISParser.Parsers;

namespace VISParser
{
    public sealed class ImageBuilder
    {
        readonly IList<Frame> dataList = new List<Frame>();
        Dictionary<int, Color> edgeColorScheme = new Dictionary<int, Color>();
        readonly Dictionary<int, Color> clusterColorDict = new Dictionary<int, Color>();
        public delegate void Message(string message);
        public event Message ImageBuilt;
        readonly int xLength, yLength;
        int xAxisStep, yAxisStep;
        Point origin;
        private void init()
        {

            this.edgeColorScheme = new Dictionary<int, Color>();
            this.edgeColorScheme.Add(0, Color.Red);
            this.edgeColorScheme.Add(1, Color.Green);
            this.edgeColorScheme.Add(2, Color.Blue);
            this.edgeColorScheme.Add(3, Color.Black);
            this.edgeColorScheme.Add(4, Color.Orange);
            this.clusterColorDict.Add(0, Color.Red);
            this.clusterColorDict.Add(1, Color.Green);
            this.clusterColorDict.Add(2, Color.Blue);
            this.clusterColorDict.Add(3, Color.Black);
            this.clusterColorDict.Add(4, Color.FromArgb(128, 177, 211));
        }
        public ImageBuilder(int xLength, int yLength, IList<Frame> dataList = null)
        {
            if (dataList != null)
                this.dataList = dataList as IList<Frame>;
            this.init();
            this.xLength = xLength;
            this.yLength = yLength;
            this.xAxisStep = 2 / xLength;
            this.yAxisStep = 2 / yLength;
            this.origin = new Point(xLength / 2, yLength / 2);
        }
        public string BuildClusterImages(Dictionary<int, BenTools.Mathematics.VoronoiGraph> vorGraphs, string dataPath, string clusterGraphPath, string clusterHistogramPath, string outputFolderPath, int center, int pictureLimit = 10)
        {
            var sb = new StringBuilder();
            var reader = new DataReader(dataPath);
            var extractor = new GraphExtractor();
            List<Graph> graphs = extractor.ConvertStringToGraphs(dataPath, clusterGraphPath);
            var frequencies = new Dictionary<string, double>();
            List<Coords> rawFrequencys = reader.ReadEdgeHistogram(clusterHistogramPath);
            foreach (var item in rawFrequencys)
                frequencies.Add(item.ToString(), item.Frequency);
            var rand = new Random();
            int startIndex = rand.Next(graphs.Count - pictureLimit - 1 > 0 ? graphs.Count - 1 - pictureLimit : 1);
            startIndex = startIndex < graphs.Count ? startIndex : 0;
            List<Graph> toBuild = graphs.GetRange(startIndex, startIndex + pictureLimit < graphs.Count ? pictureLimit : graphs.Count - 1 - startIndex);
            System.IO.Directory.CreateDirectory(outputFolderPath + "\\Center" + center.ToString());
            Parallel.ForEach(graphs, graph =>
            {
                if (vorGraphs.ContainsKey(graph.TimeStamp))
                {
                    try
                    {
                        foreach (var edge in graph.Edges)
                        {
                            edge.Frequency = frequencies[edge.ToString()];
                        }
                        var frame = new Frame(graph.TimeStamp, graph.Nodes);
                        sb.Append(this.buildImage(vorGraphs[graph.TimeStamp], frame, outputFolderPath + "\\Center" + center.ToString() + "\\", graph.Edges, -1, 5, "Center" + center.ToString(), false, center));
                        sb.Append(Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);

                    }
                }
            });
            return sb.ToString();
        }
        public string BuildFreqSubgraphImages(string dataPath, string graphStringPath, string outputFolderPath)
        {
            var extractor = new GraphExtractor();
            List<Graph> graphs = extractor.ConvertStringToGraphs(dataPath, graphStringPath);
            Dictionary<int, VoronoiGraph> vorGraphs = extractor.BuildVorGraphs(graphs, 0, false);
            var sb = new StringBuilder();
            foreach (var item in graphs)
            {
                if (vorGraphs.ContainsKey(item.TimeStamp))
                {
                    var frame = new Frame(item.TimeStamp, item.Nodes);
                    sb.Append(this.buildImage(vorGraphs[item.TimeStamp], frame, outputFolderPath, item.Edges, item.OccurenceCount));
                    sb.Append(Environment.NewLine);
                }
            }
            return sb.ToString();
        }
        public void BuildImages(Dictionary<int, VoronoiGraph> vorGraphs, string outputFolderPath, List<Graph> visGraphs = null, bool parallel = false)
        {
            Dictionary<int, Graph> visGraphDict = null;
            if (visGraphs != null)
                visGraphDict = visGraphs.ToDictionary(item => item.TimeStamp, item => item);
            if (parallel)
            {
                Parallel.ForEach(this.dataList, item =>
                {
                    if (vorGraphs.ContainsKey(item.FrameNumber))
                    {
                        List<Coords> edges = null;
                        if (visGraphDict != null && !visGraphDict.ContainsKey(item.FrameNumber))
                            return;
                        if (visGraphDict != null)
                        {
                            edges = visGraphDict[item.FrameNumber].Edges;
                        }
                        try
                        {
                            VoronoiGraph graph = vorGraphs[item.FrameNumber];
                            if (graph != null)
                                this.buildImage(graph, item, outputFolderPath, edges);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                });
                return;
            }
            foreach (var item in dataList)
            {
                if (vorGraphs.ContainsKey(item.FrameNumber))
                {
                    if (visGraphDict != null && !visGraphDict.ContainsKey(item.FrameNumber))
                        continue;
                    List<Coords> edges = null;
                    if (visGraphDict != null && !visGraphDict.ContainsKey(item.FrameNumber))
                        return;
                    if (visGraphDict != null)
                    {
                        edges = visGraphDict[item.FrameNumber].Edges;
                    }
                    try
                    {
                        VoronoiGraph graph = vorGraphs[item.FrameNumber];
                        if (graph != null)
                            this.buildImage(graph, item, outputFolderPath, edges);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public void BuildDominanceImages(SortedDictionary<int, ulong[]> dominanceMap, Dictionary<int, Frame> dataDict, IList<PointF> queryPoints, string outputFolderPath, Teams team, bool parallel = false)
        {
            if (parallel)
            {
                Parallel.ForEach(dominanceMap, dominance =>
                {
                    if (dominance.Value != null)
                        this.buildDominanceImage(dataDict[dominance.Key], queryPoints, dominance.Value, outputFolderPath, team);

                });
                return;
            }
            foreach (var dominance in dominanceMap)
            {
                if (dominance.Value != null)
                    this.buildDominanceImage(dataDict[dominance.Key], queryPoints, dominance.Value, outputFolderPath, team);
            }

        }
        private int relativePosX(double xCoord)
        {
            if (xCoord < -1)
                xCoord = -1;
            if (xCoord > 1)
                xCoord = 1;
            int newCoord = (int)(this.origin.X * (1d + xCoord));
            return newCoord;

        }
        private int relativePosY(double yCoord)
        {
            if (yCoord < -1)
                yCoord = -1;
            if (yCoord > 1)
                yCoord = 1;
            int newCoord = (int)(this.origin.Y * (1d + yCoord));
            return newCoord;
        }
        private void addEdges(Graphics g, Frame data, List<Coords> edges, int confidence, bool multiColor)
        {
            double max = edges.Max(edge => edge.Frequency);
            double min = edges.Min(edge => edge.Frequency);
            max = Math.Max(Math.Abs(max), Math.Abs(min));
            Brush brush = new SolidBrush(Color.Black);
            foreach (var item in edges)
            {
                if (!item.IsFrequencySet)
                {
                    g.DrawLine(Pens.Black, this.relativePosX(data.Objects[item.Row + 1].XCoord),
                              this.relativePosY(data.Objects[item.Row + 1].YCoord),
                               this.relativePosX(data.Objects[item.Col + 1].XCoord),
                               this.relativePosY(data.Objects[item.Col + 1].YCoord));
                    var strRect = new Rectangle(0, 0, 100, 20);
                    if (confidence != -1)
                        g.DrawString(confidence.ToString(CultureInfo.InvariantCulture) + "/" + "13100", new Font("Tahoma", 8), brush, strRect);
                }
                else
                {
                    if (item.Label == -1)
                    {
                        Pen pen;
                        if (multiColor == false)
                        {

                            pen = new Pen(item.Frequency > 0 ? Color.FromArgb((int)(255 * item.Frequency * 1 / max), Color.Black) :
                                                            Color.FromArgb((int)(255 * Math.Abs(item.Frequency) * 1 / max), Color.Red), 2);
                        }
                        else
                        {
                            pen = new Pen(Color.FromArgb((int)(255 * item.Frequency * 1 / max), this.edgeColorScheme[item.StrongestCluster]), 2);
                        }
                        g.DrawLine(pen, this.relativePosX(data.Objects[item.Row + 1].XCoord),
                             this.relativePosY(data.Objects[item.Row + 1].YCoord),
                             this.relativePosX(data.Objects[item.Col + 1].XCoord),
                             this.relativePosY(data.Objects[item.Col + 1].YCoord));
                    }
                }
            }
        }
        private string buildDominanceImage(Frame data, IList<PointF> queryPoints, ulong[] dominance, string outputFolderPath, Teams team, string sufix = "")
        {
            var bitmap = new System.Drawing.Bitmap(this.xLength, this.yLength);
            //The color of the team for which the dominance is given by the ulong[]
            Color dominanceColor = team == Teams.Home ? Color.Green : Color.Blue;
            Color nonDominanceColor = dominanceColor == Color.Green ? Color.Blue : Color.Green;
            Graphics g = Graphics.FromImage(bitmap);
            double sideLength = Math.Ceiling(Math.Abs(queryPoints[0].X - queryPoints[1].X));
            var brush = new SolidBrush(Color.White);
            g.FillRectangle(brush, 0, 0, bitmap.Width, bitmap.Height);
            for (int index = 0; index < queryPoints.Count; index++)
            {
                Pen pen;
                PointF point = queryPoints[index];
                ulong mask = 1;
                mask <<= (63 - index % 64);
                int dominanceIndex = index / 64;
                pen = new Pen((mask & dominance[dominanceIndex]) > 0 ? dominanceColor : nonDominanceColor, 3);
                var upperLeft = new Point((int)Math.Ceiling((point.X - sideLength / 2)), (int)Math.Ceiling((point.Y - sideLength / 2)));
                var rect = new Rectangle(upperLeft, new Size((int)sideLength, (int)sideLength));
                g.FillRectangle(new SolidBrush(pen.Color), rect);

            }
            this.addObjectsToImage(g, data.Objects);
            bitmap.Save(outputFolderPath + "\\image" + data.FrameNumber.ToString(CultureInfo.InvariantCulture) + sufix + ".bmp");
            Console.WriteLine("Done building image {0}", data.FrameNumber.ToString(CultureInfo.InvariantCulture));
            if (this.ImageBuilt != null)
                this.ImageBuilt.Invoke(string.Format("Done building image {0}", data.FrameNumber.ToString(CultureInfo.InvariantCulture)));
            return data.FrameNumber.ToString(CultureInfo.InvariantCulture) + "Dominance.bmp";
        }
        //Adds players referees and the ball to the playing field.
        private void addObjectsToImage(Graphics g, List<Info> list)
        {
            Brush brush;
            foreach (var item in list)
            {


                if (item is Player || item is Referee)
                {
                    Rectangle dotRect;
                    Rectangle strRect;
                    if (item is Player)
                    {
                        var curPlayer = item as Player;
                        if (curPlayer.HasPossession)
                        {
                            brush = new SolidBrush(Color.Blue);
                            dotRect = new Rectangle(this.relativePosX(item.XCoord), this.relativePosY(item.YCoord), 12, 12);
                            strRect = new Rectangle(this.relativePosX(item.XCoord), this.relativePosY(item.YCoord), 20, 20);
                            g.FillRectangle(new SolidBrush(Color.Black), dotRect);
                            g.DrawString(item.ItemId.ToString(CultureInfo.InvariantCulture), new Font("Tahoma", 8), brush, strRect);
                            continue;
                        }
                    }
                    brush = new SolidBrush(Color.Black);
                    dotRect = new Rectangle(this.relativePosX(item.XCoord), this.relativePosY(item.YCoord), 3, 3);
                    strRect = new Rectangle(this.relativePosX(item.XCoord), this.relativePosY(item.YCoord), 20, 20);
                    g.FillRectangle(new SolidBrush(Color.Black), dotRect);
                    g.DrawString(item.ItemId.ToString(CultureInfo.InvariantCulture), new Font("Tahoma", 8), brush, strRect);

                }
                else
                {

                    brush = new SolidBrush(item.ItemColor);
                    var itemRect = new Rectangle(this.relativePosX(item.XCoord), this.relativePosY(item.YCoord), 6, 6);
                    g.FillEllipse(brush, itemRect);

                }
            }
        }
        private string buildImage(BenTools.Mathematics.VoronoiGraph vorGraph, Frame data, string outputFolderPath, List<Coords> edges = null, int confidence = -1, int edgeClusters = 5, string suffix = "", bool multiColor = false, int targetCenter = -1)
        {
            var bitmap = new System.Drawing.Bitmap(this.xLength, this.yLength);
            Graphics g = Graphics.FromImage(bitmap);
            var brush = new SolidBrush(Color.White);
            g.FillRectangle(brush, 0, 0, bitmap.Width, bitmap.Height);
            g = this.buildPolygons(vorGraph, data, g);
            this.addObjectsToImage(g, data.Objects);

            if (edges != null)
            {
                this.addEdges(g, data, edges, confidence, multiColor);
            }
            bitmap.Save(outputFolderPath + "\\image" + data.FrameNumber.ToString(CultureInfo.InvariantCulture) + suffix + ".bmp");
            Console.WriteLine("Done building image {0}", data.FrameNumber.ToString(CultureInfo.InvariantCulture));
            if (this.ImageBuilt != null)
                this.ImageBuilt.Invoke(string.Format("Done building image {0}", data.FrameNumber.ToString(CultureInfo.InvariantCulture)));
            return data.FrameNumber.ToString(CultureInfo.InvariantCulture) + ",pictures/Center" + Math.Abs(targetCenter).ToString(CultureInfo.InvariantCulture) + "/image" + data.FrameNumber.ToString(CultureInfo.InvariantCulture) + suffix + ".bmp";
        }
        private Graphics buildPolygons(BenTools.Mathematics.VoronoiGraph vorGraph, Frame data, Graphics g)
        {

            Dictionary<int, List<PointF>> edges = this.convertToPolygons(vorGraph);
            foreach (var polygon in edges)
            {

                var player = data.Objects.Single(item => item.ItemId == polygon.Key) as Player;
                var brush = new SolidBrush(player.ItemColor);
                try
                {
                    g.FillPolygon(brush, polygon.Value.ToArray(), FillMode.Winding);
                    g.DrawPolygon(new Pen(Color.Black, 2), polygon.Value.ToArray());
                }
                catch (Exception)
                {
                    Console.WriteLine("Error with Image {0}", data.FrameNumber.ToString(CultureInfo.InvariantCulture));
                    throw;
                }

            }
            return g;
        }
        private Dictionary<int, List<PointF>> convertToPolygons(VoronoiGraph vorGraph)
        {
            var polygons = new Dictionary<int, List<PointF>>();
            Dictionary<int, List<VoronoiEdge>> playerEdges = this.extractEdges(vorGraph);
            for (int pId = 1; pId <= VISAPI.PlayerCount; pId++)
            {
                polygons.Add(pId, new List<PointF>());
                var flags = new bool[playerEdges[pId].Count];
                int iterationCount = 0;
                do
                {
                    for (int index = 0; index < playerEdges[pId].Count; index++)
                    {

                        var edge = playerEdges[pId][index];
                        List<PointF> newLine = this.ConvertToLine(edge);
                        if (flags[index])
                            continue;
                        if (polygons[pId].Contains(newLine[0])
                            && polygons[pId].Contains(newLine[1]))
                        {
                            flags[index] = true;
                            break;
                        }
                        if (polygons[pId].Contains(newLine[0]))
                        {
                            int insertIndex = polygons[pId].IndexOf(newLine[0]);
                            if (insertIndex == 0)
                            {
                                polygons[pId].Insert(insertIndex, newLine[1]);


                            }
                            else if (insertIndex == polygons[pId].Count - 1)
                            {
                                polygons[pId].Add(newLine[1]);
                            }
                            else
                                throw new Exception("This else should not be reached, ever!!!");
                            flags[index] = true;
                        }
                        else if (polygons[pId].Contains(newLine[1]))
                        {
                            int insertIndex = polygons[pId].IndexOf(newLine[1]);
                            if (insertIndex == 0)
                            {
                                polygons[pId].Insert(insertIndex, newLine[0]);

                            }
                            else if (insertIndex == polygons[pId].Count - 1)
                            {
                                polygons[pId].Add(newLine[0]);
                            }
                            else
                                throw new Exception("This else should not be reached, ever!!!");
                            flags[index] = true;
                        }
                        else
                        {
                            if (polygons[pId].Count == 0)
                            {
                                polygons[pId].Add(newLine[0]);
                                polygons[pId].Add(newLine[1]);
                                flags[index] = true;
                            }
                        }
                    }
                    iterationCount++;
                    if (iterationCount > 10)
                        throw new Exception(string.Format("Image {0} got stuck in an infinite loop", iterationCount));

                }
                while (flags.Count(item => item == false) > 0);
            }
            return polygons;
        }
        private Dictionary<int, List<VoronoiEdge>> extractEdges(VoronoiGraph vorGraph)
        {
            var result = new Dictionary<int, List<VoronoiEdge>>();
            int upperLeftId = -1;
            int lowerLeftId = -1;
            int upperRightId = -1;
            int lowerRightId = -1;
            var upperLeft = new VoronoiEdge {VVertexA = new Vector(2)};
            upperLeft.VVertexA[0] = xLength;
            upperLeft.VVertexA[1] = 0;
            upperLeft.VVertexB = new Vector("(0;0)");
            var lowerLeft = new VoronoiEdge {VVertexA = new Vector(2)};
            lowerLeft.VVertexA[0] = xLength;
            lowerLeft.VVertexA[1] = yLength;
            lowerLeft.VVertexB = new Vector("(0;" + yLength.ToString(CultureInfo.InvariantCulture) + ")");
            var upperRight = new VoronoiEdge {VVertexA = new Vector(2)};
            upperRight.VVertexA[0] = 0;
            upperRight.VVertexB[1] = 0;
            upperRight.VVertexB = new Vector("(" + xLength.ToString(CultureInfo.InvariantCulture) + ";0)");
            var lowerRight = new VoronoiEdge {VVertexA = new Vector(2)};
            lowerRight.VVertexA[0] = 0;
            lowerRight.VVertexA[1] = yLength;
            lowerRight.VVertexB = new Vector("(" + xLength.ToString(CultureInfo.InvariantCulture) + ";" + yLength.ToString(CultureInfo.InvariantCulture) + ")");
            foreach (var edge in vorGraph.Edges)
            {
                if (result.ContainsKey(edge.RightDataId))
                {
                    result[edge.RightDataId].Add(edge);
                }
                else
                {
                    result.Add(edge.RightDataId, new List<VoronoiEdge>());
                    result[edge.RightDataId].Add(edge);
                }
                if (result.ContainsKey(edge.LeftDataId))
                {
                    result[edge.LeftDataId].Add(edge);
                }
                else
                {
                    result.Add(edge.LeftDataId, new List<VoronoiEdge>());
                    result[edge.LeftDataId].Add(edge);
                }
                //UpperLeft and UpperRight
                if (edge.VVertexA[1] == 0)
                {
                    if (edge.VVertexA[0] <= upperLeft.VVertexA[0])
                    {
                        upperLeft.VVertexA[0] = edge.VVertexA[0];
                        upperLeftId = edge.LeftData[0] < edge.RightData[0] ? edge.LeftDataId : edge.RightDataId;
                    }
                    if (edge.VVertexA[0] >= upperRight.VVertexA[0])
                    {
                        upperRight.VVertexA[0] = edge.VVertexA[0];
                        upperRightId = edge.LeftData[0] > edge.RightData[0] ? edge.LeftDataId : edge.RightDataId;
                    }
                }
                if (edge.VVertexB[1] == 0)
                {
                    if (edge.VVertexB[0] <= upperLeft.VVertexA[0])
                    {
                        upperLeft.VVertexA[0] = edge.VVertexB[0];
                        upperLeftId = edge.LeftData[0] < edge.RightData[0] ? edge.LeftDataId : edge.RightDataId;
                    }
                    if (edge.VVertexB[0] >= upperRight.VVertexA[0])
                    {
                        upperRight.VVertexA[0] = edge.VVertexB[0];
                        upperRightId = edge.LeftData[0] > edge.RightData[0] ? edge.LeftDataId : edge.RightDataId;
                    }
                }
                //LowerLeft and UpperLEft
                if (edge.VVertexA[1] == yLength)
                {
                    if (edge.VVertexA[0] <= lowerLeft.VVertexA[0])
                    {
                        lowerLeft.VVertexA[0] = edge.VVertexA[0];
                        lowerLeftId = edge.LeftData[0] < edge.RightData[0] ? edge.LeftDataId : edge.RightDataId;
                    }
                    if (edge.VVertexA[0] >= lowerRight.VVertexA[0])
                    {
                        lowerRight.VVertexA[0] = edge.VVertexA[0];
                        lowerRightId = edge.LeftData[0] > edge.RightData[0] ? edge.LeftDataId : edge.RightDataId;
                    }
                }
                if (edge.VVertexB[1] == yLength)
                {
                    if (edge.VVertexB[0] <= lowerLeft.VVertexA[0])
                    {
                        lowerLeft.VVertexA[0] = edge.VVertexB[0];
                        lowerLeftId = edge.LeftData[0] < edge.RightData[0] ? edge.LeftDataId : edge.RightDataId;
                    }
                    if (edge.VVertexB[0] >= lowerRight.VVertexA[0])
                    {
                        lowerRight.VVertexA[0] = edge.VVertexB[0];
                        lowerRightId = edge.LeftData[0] > edge.RightData[0] ? edge.LeftDataId : edge.RightDataId;
                    }
                }
            }
            result[lowerRightId].Add(lowerRight);
            result[upperRightId].Add(upperRight);
            result[lowerLeftId].Add(lowerLeft);
            result[upperLeftId].Add(upperLeft);
            return result;
        }
        private List<PointF> ConvertToLine(BenTools.Mathematics.VoronoiEdge edge)
        {
            var result = new List<PointF>();
            Point startPoint;
            Point endPoint;
            if (edge.IsPartlyInfinite)
            {
                //The first big number that came to mind :))
                double randNumber = 5000;
                Vector start = double.IsInfinity(edge.VVertexB.ElementSum) ? edge.VVertexA : edge.VVertexB;
                startPoint = new Point((int)start[0], (int)start[1]);
                endPoint = new Point((int)start[0] + (int)(randNumber * edge.DirectionVector[0]),
                                        (int)start[1] + (int)(randNumber * edge.DirectionVector[1]));
                result.Add(startPoint);
                result.Add(endPoint);
            }
            else
            {

                startPoint = new Point((int)edge.VVertexA[0], (int)edge.VVertexA[1]);
                endPoint = new Point((int)edge.VVertexB[0],
                                       (int)edge.VVertexB[1]);
                result.Add(startPoint);
                result.Add(endPoint);
            }
            return result;
        }
    }
}
