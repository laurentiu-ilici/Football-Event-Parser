using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using BenTools.Mathematics;
using VISParser.FieldObjects;

namespace VISParser.Parsers
{
    public sealed class GraphExtractor
    {
        IList<Frame> dataList;
        List<long> timeStamps = new List<long>();
        const int xLength = 1050, yLength = 680;
     
        readonly Point origin;
        public GraphExtractor(IList<Frame> dataList = null)
        {
            if (dataList != null)
                this.dataList = dataList;
            this.origin = new Point(xLength / 2, yLength / 2);
        }
        #region Private Methods
        private double distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y2 - y1, 2));
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
        private bool acceptVoronoiGraph(VoronoiGraph lastGraph, VoronoiGraph testedGraph, double epsilon)
        {
            var toBeTested = new List<VoronoiEdge>();
            foreach (var edge in testedGraph.Edges)
            {
                bool test = true;
                //Find the Id of the datapoint of this edge
                for (int index = 0; index < lastGraph.Edges.Count; index++)
                {
                    if (edge.Equals(lastGraph.Edges.ElementAt(index)))
                    {
                        test = false;
                        break;
                    }
                }
                if (test)
                {
                    //if the new edge is partially infinite and is of at least eps, imediately accept the new graph
                    //because the distance will be larger than whatever epsilon
                    if (edge.IsPartlyInfinite)
                    {
                        Vector nonInfinite = double.IsInfinity(edge.VVertexA.ElementSum) ? edge.VVertexB : edge.VVertexA;
                        Vector position = edge.DirectionVector * epsilon;
                        position += nonInfinite;
                        if (this.IsOnField(position))
                            return true;

                    }
                    else
                    {
                        toBeTested.Add(edge);
                    }
                }
            }
            foreach (var edge in lastGraph.Edges)
            {
                bool test = true;
                //Find the Id of the datapoint of this edge
                for (int index = 0; index < testedGraph.Edges.Count; index++)
                {
                    if (edge.Equals(testedGraph.Edges.ElementAt(index)))
                    {
                        test = false;
                        break;
                    }
                }
                if (test)
                {
                    if (!edge.IsPartlyInfinite)
                        toBeTested.Add(edge);
                    else
                    {
                        Vector nonInfinite = double.IsInfinity(edge.VVertexA.ElementSum) ? edge.VVertexB : edge.VVertexA;
                        Vector position = edge.DirectionVector * epsilon;
                        position += nonInfinite;
                        if (this.IsOnField(position))
                            return true;
                    }

                }

            }
            if (toBeTested.Count == 0)
                return false;

            return epsilon < toBeTested.Max(edge => edge.Length);
        }
        private bool IsOnField(Vector vect)
        {
            if (vect[0] < this.relativePosX(-1) || vect[0] > this.relativePosX(1)
                || vect[1] < this.relativePosY(-1) ||
                vect[1] > this.relativePosY(1))
                return false;
            return true;
        }
        private List<BenTools.Mathematics.Vector> createConfiguration(Graph target)
        {
            var result = new List<Vector>();
            for (int pidIndex = 1; pidIndex <= VISAPI.PlayerCount; pidIndex++)
            {
                if (!(target.Nodes[pidIndex] is Player))
                    continue;
                var data = new double[2];
                data[0] = this.relativePosX(target.Nodes[pidIndex].XCoord);
                data[1] = this.relativePosY(target.Nodes[pidIndex].YCoord);
                var vorVect = new Vector(data);
                result.Add(new Vector(data));
            }
            return result;
        }
        private VoronoiGraph discardEpsEdges(VoronoiGraph target, double epsilon)
        {
            var toBeDiscarded = new List<VoronoiEdge>();
            foreach (var item in target.Edges)
            {
                if (item.IsPartlyInfinite)
                {
                    Vector nonInfinite = double.IsInfinity(item.VVertexA.ElementSum) ? item.VVertexB : item.VVertexA;
                    if (!this.IsOnField(nonInfinite))
                    {
                        toBeDiscarded.Add(item);
                        continue;
                    }
                    if (double.IsInfinity(item.VVertexA.ElementSum))
                    {

                        item.VVertexA = this.intersection(nonInfinite, item.DirectionVector);
                    }
                    else
                    {
                        item.VVertexB = this.intersection(nonInfinite,    item.DirectionVector);
                    }
                   // if (item.Length < epsilon)
                     //   toBeDiscarded.Add(item);

                }
                else if (!this.IsOnField(item.VVertexA) && this.IsOnField(item.VVertexB)
                               || this.IsOnField(item.VVertexA) && !this.IsOnField(item.VVertexB))
                {
                    Vector onField = this.IsOnField(item.VVertexA) ? item.VVertexA : item.VVertexB;

                    //TODO: CHANGE HERE!!!!!
                    if (this.IsOnField(item.VVertexA))
                    {
                        item.VVertexB = this.intersection(onField, item.DirectionVector);
                    }
                    else
                    {
                        item.VVertexA = this.intersection(onField, -1 * item.DirectionVector);
                    }
                    if (item.Length < epsilon)
                        toBeDiscarded.Add(item);
                }
                else if (!this.IsOnField(item.VVertexB) && !this.IsOnField(item.VVertexA))
                {
                    toBeDiscarded.Add(item);
                }
                else if (item.IsInfinite)
                    throw new Exception("Inifinite edge????");


            }
            foreach (var delete in toBeDiscarded)
                target.Edges.Remove(delete);
            return target;
        }
        private double playerDistanceStrength(double x1, double y1, double x2, double y2)
        {
            return 1 / this.distance(x1, y1, x2, y2);
        }
        private double calculateAngle(VoronoiEdge edge)
        {
            double distanceC = edge.Length;
            double distanceA = this.distance(edge.LeftData[0], edge.LeftData[1], edge.VVertexA[0], edge.VVertexA[1]);
            double distanceB = this.distance(edge.LeftData[0], edge.LeftData[1], edge.VVertexB[0], edge.VVertexB[1]);
            double cosAlpha = (Math.Pow(distanceC, 2) - Math.Pow(distanceA, 2) - Math.Pow(distanceB, 2)) / (-2 * distanceB * distanceA);

            return Math.Acos(cosAlpha) * 180 / Math.PI;

        }
        private Vector intersection(Vector onField, Vector direction)
        {
            double intersectX, intersectY;
            var result = new Vector(2);
            if (Math.Sign(direction[0]) == 0)
            {
                intersectX = onField[0];
                if(Math.Sign(direction[1]) > 0)
                {
                    intersectY = yLength;

                }
                else if (Math.Sign(direction[1]) < 0)
                {
                    intersectY = 0;
                  
                }
                else
                {
                    throw new Exception("Vector without a direction???");
                }
                result[0] = intersectX;
                result[1] = intersectY;
                return result;
            }
            else if (Math.Sign(direction[1]) == 0)
            {
                intersectY = onField[1];
                if (Math.Sign(direction[0]) > 0)
                {
                    intersectX = xLength;
          
                }
                else if (Math.Sign(direction[0]) < 0)
                {
                    intersectX = 0;
                }
                else
                {
                    throw new Exception("Vector without a direction???");
                }
                result[0] = intersectX;
                result[1] = intersectY;
                return result;
            
            }
            else if (Math.Sign(direction[0]) > 0 && Math.Sign(direction[1]) > 0)
            {
                //Intersection with the North border
                intersectY = yLength;
                double steps = (yLength - onField[1]) / direction[1];
                intersectX = onField[0] + steps * direction[0];
                result[0] = intersectX;
                result[1] = intersectY;
                if(this.IsOnField(result))
                    return result;
                //Intersection with the East border
                intersectX = xLength;
                steps = (xLength - onField[0]) / direction[0];
                intersectY = onField[1] + steps * direction[1];
                result[0] = intersectX;
                result[1] = intersectY;
                if (this.IsOnField(result))
                    return result;
                else
                    throw new Exception("Something went wrong with the intersection points");
             
            }
            else if (Math.Sign(direction[0]) < 0 && Math.Sign(direction[1]) > 0)
            {
                //Intersection with the North border
                intersectY = yLength;
                double steps = (yLength - onField[1]) / direction[1];
                intersectX = onField[0] + steps * direction[0];
                result[0] = intersectX;
                result[1] = intersectY;
                if (this.IsOnField(result))
                    return result;
                //Intersection with the West border
                intersectX = 0;
                steps =  - onField[0] / direction[0];
                intersectY = onField[1] + steps * direction[1];
                result[0] = intersectX;
                result[1] = intersectY;
                if (this.IsOnField(result))
                    return result;
                else
                    throw new Exception("Something went wrong with the intersection points");

            }
            else if (Math.Sign(direction[0]) < 0 && Math.Sign(direction[1]) < 0)
            {
                //Intersection with the West border
                intersectX = 0;
                double steps = -onField[0] / direction[0];
                intersectY = onField[1] + steps * direction[1];
                result[0] = intersectX;
                result[1] = intersectY;
                if (this.IsOnField(result))
                    return result;
                //Intersection with the South border
                intersectY = 0;
                steps = -onField[1] / direction[1];
                intersectX = onField[0] + steps * direction[0];
                result[0] = intersectX;
                result[1] = intersectY;
                if (this.IsOnField(result))
                    return result;
                else
                    throw new Exception("Something went wrong with the intersection points");
            }
            else if (Math.Sign(direction[0]) > 0 && Math.Sign(direction[1]) < 0)
            {
                //Intersection with the East border
                intersectX = xLength;
                double steps = (xLength - onField[0]) / direction[0];
                intersectY = onField[1] + steps * direction[1];
                result[0] = intersectX;
                result[1] = intersectY;
                if(this.IsOnField(result))
                    return result;
                //Intersection with the South border
                intersectY = 0;
                steps = -onField[1] / direction[1];
                intersectX = onField[0] + steps * direction[0];
                result[0] = intersectX;
                result[1] = intersectY;
                if (this.IsOnField(result))
                    return result;
                else
                    throw new Exception("Something went wrong with the intersection points");
            }
            throw new Exception("Something went wrong with the intersection points");
        }
        private VoronoiGraph mergeIdsToEdges(List<Info> nodes, VoronoiGraph graph)
        {
            bool okLeft;
            bool okRight;
            foreach (var item in graph.Edges)
            {
                okLeft = false;
                okRight = false;
                for (int pidIndex = 1; pidIndex < nodes.Count; pidIndex++)
                {
                    var data = new double[2];
                    data[0] = this.relativePosX(nodes[pidIndex].XCoord);
                    data[1] = this.relativePosY(nodes[pidIndex].YCoord);
                    var vorVect = new Vector(data);
                    if (item.LeftData.Equals(vorVect))
                    {
                        item.LeftDataId = nodes[pidIndex].ItemId;
                        okLeft = true;
                    }
                    else if (item.RightData.Equals(vorVect))
                    {
                        item.RightDataId = nodes[pidIndex].ItemId;
                        okRight = true;

                    }
                    if (okLeft && okRight)
                        break;
                }
                if (!okLeft || !okRight)
                    throw new Exception("We didn't find the id's on merging????");
            }

            return graph;
        }
        private Graph interpretVoronoiGraph(List<Info> nodes, long timeStamp, BenTools.Data.HashSet<VoronoiEdge> edges)
        {
            var graphEdges = new List<Coords>();
            foreach (var item in edges)
            {
                if (item.RightDataId < item.LeftDataId)
                {
                    var newCoord = new Coords(item.RightDataId - 1, item.LeftDataId - 1);
                    newCoord.Strength = this.calculateAngle(item);
                    graphEdges.Add(newCoord);
                }
                else if (item.LeftDataId < item.RightDataId)
                {
                    var newCoord = new Coords(item.LeftDataId - 1, item.RightDataId - 1);
                    newCoord.Strength = this.calculateAngle(item);
                    graphEdges.Add(newCoord);
                }
                else
                    throw new Exception("The ids should not be equal!!!");
            }
            return new Graph(nodes, graphEdges, (int)timeStamp);
        }
        #endregion
        #region Public Methods
        public Dictionary<int, VoronoiGraph> BuildVorGraphs(List<Graph> toBeParsed, double epsilon, bool checkEpsilon)  
        {
            var result = new Dictionary<int, VoronoiGraph>();
            VoronoiGraph lastVoronoiGraph = null;
            int countRejects = 0;
            Vector.Precision = 2;
            for (int index = 0; index < toBeParsed.Count; index++)
            {
                List<BenTools.Mathematics.Vector> currentConfig = this.createConfiguration(toBeParsed[index]);
                var vorGraph = new VoronoiGraph();
                try
                {
                    vorGraph = Fortune.ComputeVoronoiGraph(currentConfig);
                    vorGraph = this.discardEpsEdges(vorGraph, epsilon);
                    vorGraph = this.mergeIdsToEdges(toBeParsed[index].Nodes, vorGraph);
                    if (lastVoronoiGraph == null)
                    {
                        lastVoronoiGraph = vorGraph;
                        result.Add(toBeParsed[index].TimeStamp, vorGraph);
                    }
                    else
                    {
                        if (checkEpsilon && this.acceptVoronoiGraph(lastVoronoiGraph, vorGraph, epsilon))
                        {
                            if (vorGraph == null)
                            {
                                Console.Write("Not supposed to happen!!");
                                
                            }
                            lastVoronoiGraph = vorGraph;
                            result.Add(toBeParsed[index].TimeStamp, vorGraph);
                        }
                        else if (!checkEpsilon)
                        {
                            result.Add(toBeParsed[index].TimeStamp, vorGraph);
                        }
                        else
                        {
                            countRejects++;

                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Problem with index = {0}", toBeParsed[index].TimeStamp);
                }
            }
            Console.WriteLine("Discarded {0} graphs during the Voronoi algorithm application", countRejects);
            return result;
        }
        //Converts a VISGRAPH to a Voronoi Graph. Epsilond is the length in pixels that the
        //edges have to have in order to be displayed (or taken into account).
        public  VoronoiGraph BuildVorGraph(Frame toBeParsed,double epsilon=4)
        {
            Graph parsedGraph = this.ConvertFrameToGraph(toBeParsed);
            List<BenTools.Mathematics.Vector> currentConfig = this.createConfiguration(parsedGraph);
            VoronoiGraph vorGraph = Fortune.ComputeVoronoiGraph(currentConfig);
            vorGraph = this.discardEpsEdges(vorGraph, epsilon);
            vorGraph = this.mergeIdsToEdges(parsedGraph.Nodes, vorGraph);
            return vorGraph;
        }

        public List<Graph> BuildVISVoronoiGraphs(List<Graph> toBeParsed, double epsilon = 0)
        {
            int countRejects = 0;
            var result = new List<Graph>();
            Dictionary<int, Graph> targetsDict = toBeParsed.ToDictionary(item => item.TimeStamp);
            Dictionary<int, VoronoiGraph> vorGraphs = this.BuildVorGraphs(toBeParsed, epsilon,true);
            foreach (var item in vorGraphs)
            {
                Graph interpretation = this.interpretVoronoiGraph(targetsDict[item.Key].Nodes, toBeParsed[item.Key].TimeStamp, item.Value.Edges);
                if (result.Count == 0)
                {
                    result.Add(interpretation);
                }
                else if (epsilon == 0)
                {
                    result.Add(interpretation);
                }
                else if (interpretation.Edges.Count != 0 && !result[result.Count - 1].IsEqual(interpretation))
                {

                    result.Add(interpretation);
                }
                else
                {
                    countRejects++;
                }
            }
            Console.WriteLine(" Eps = {0} Rejects = {1}", epsilon, countRejects);
            return result;
        }
        public List<Graph> ConvertStringToGraphs(string dataPath, string GraphStringsPath = @"D:\Work\PythonWorkspace\Football\VISData\Graphs10.txt", bool firstHalf = true)
        {
            var reader = new DataReader(dataPath);
            this.dataList = reader.DataList;
            var timeStampIndex = new Dictionary<long, int>();
            for (int index = 0; index < this.dataList.Count; index++)
            {
                timeStampIndex.Add(this.dataList[index].FrameNumber, index);
            }
            Dictionary<string, long[]> sugraphs = reader.ReadGraphStrings(GraphStringsPath);
            var result = new List<Graph>();
            foreach (var item in sugraphs)
            {
                List<Coords> edges = reader.ParseKey(item.Key);
                //Average over the coords of the "nodes" aka the players
                //in a very quick and dirty way :(
                List<Info> nodes = null;
                int nrOfGraphs = 0;
                for (int index = 0; index < item.Value.Length; index++)
                {
                    //It means the game was in an interrupted state so we can skip
                    //whatever position this was...
                    if (!timeStampIndex.ContainsKey(item.Value[index]))
                        continue;
                    nrOfGraphs++;
                    int index2 = timeStampIndex[item.Value[index]];
                    if (nodes == null)
                    {
                        nodes = new List<Info>();
                        for (int playerID = 0; playerID < dataList[index2].Objects.Count; playerID++)
                        {
                            if (dataList[index2].Objects[playerID] is Player)
                            {
                                nodes.Add(new Player().CopyObject(dataList[index2].Objects[playerID]));
                            }
                            else if (dataList[index2].Objects[playerID] is Ball)
                            {
                                nodes.Add(new Ball().CopyObject(dataList[index2].Objects[playerID]));

                            }
                            else
                            {
                                nodes.Add(new Referee().CopyObject(dataList[index2].Objects[playerID]));
                            }
                        }
                        break;
                    }
                    else
                    {
                        for (int playerID = 0; playerID < dataList[index2].Objects.Count; playerID++)
                        {
                            double newX = nodes[playerID].XCoord + dataList[index2].Objects[playerID].XCoord;
                            double newY = nodes[playerID].YCoord + dataList[index2].Objects[playerID].YCoord;
                            nodes[playerID].XCoord = newX;
                            nodes[playerID].YCoord = newY;
                        }
                    }

                }
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        node.XCoord /= nrOfGraphs;
                        node.YCoord /= nrOfGraphs;
                    }
                    var newGraph = new Graph(nodes, edges, (int)item.Value[0]);
                    newGraph.OccurenceCount = item.Value.Length;
                    result.Add(newGraph);
                }
            }
            return result;
        }
        public List<Graph> ConvertToEpsilonGraph(List<Graph> graphs, double epsilon)
        {
            var result = new List<Graph>();

            foreach (var graph in graphs)
            {
                var newNodes = new List<Info>();
                foreach (var item in graph.Nodes)
                {
                    if (item is Player)
                        newNodes.Add(new Player().CopyObject(item));
                    else if (item is Ball)
                        newNodes.Add(new Ball().CopyObject(item));
                    else if (item is Referee)
                        newNodes.Add(new Referee().CopyObject(item));

                }
                var newEdges = new List<Coords>();
                foreach (var edge in graph.Edges)
                {
                    double epsDistance = newNodes[edge.Row + 1].Distance(newNodes[edge.Col + 1]) / 2 + epsilon;
                    var midPoint = new Player();
                    var other = (Player)newNodes[edge.Row + 1];
                    midPoint = midPoint.CopyObject(other) as Player;
                    midPoint = midPoint.Average(newNodes[edge.Col + 1]);
                    double minDistFromMid = double.MaxValue;
                    for (int index = 1; index < newNodes.Count - 1; index++)
                    {
                        if (index == edge.Col + 1 || index == edge.Row + 1)
                            continue;
                        double curDist = midPoint.Distance(newNodes[index]);
                        if (curDist < minDistFromMid)
                            minDistFromMid = curDist;
                    }
                    if (minDistFromMid > epsDistance)
                        newEdges.Add(new Coords(edge.Row, edge.Col));

                }
                var newGraph = new Graph(newNodes, newEdges, graph.TimeStamp);
                if (newEdges.Count > 0)
                    if (result.Count == 0)
                        result.Add(newGraph);
                    else if (!result[result.Count - 1].IsEqual(newGraph))
                        result.Add(newGraph);


            }
            return result;
        }
        public List<Graph> ConvertRawDataToGraph()
        {
            return (from item in this.dataList let nodes = item.Objects.ToList() select new Graph(nodes, null, item.FrameNumber)).ToList();
        }
        public Graph ConvertFrameToGraph(Frame frame)
        {
            return new Graph(frame.Objects, null, frame.FrameNumber);
        }

        public Dictionary<string, List<Graph>> ConvertToUniqueGraphs(List<Graph> toConvert)
        {
            var result = new Dictionary<string, List<Graph>>();
            foreach (var item in toConvert)
            {
                string key = item.ToString();
                if (result.ContainsKey(key))
                    result[key].Add(item);
                else
                {
                    var list = new List<Graph>();
                    list.Add(item);
                    result.Add(key, list);
                }
            }
            return result;
        }
        public void WriteToFile(string path, List<Graph> graphs)
        {
            using (var writer = new StreamWriter(path))
            {
                foreach (var item in graphs)
                    writer.WriteLine(item.ToLongString());
            }
        }
        public void WriteToFile(string path, Dictionary<string, List<Graph>> compressed)
        {
            using (var writer = new StreamWriter(path))
            {
                foreach (var item in compressed)
                {
                    var sb = new StringBuilder(item.Key);
                    sb.Append(" :");
                    foreach (var graph in item.Value)
                    {
                        sb.Append(" " + graph.TimeStamp);
                    }

                    writer.WriteLine(sb.ToString());
                }
            }
        }
        public void WriteGraphStrenghtStrings(string path, Dictionary<string, List<Graph>> compressed, char choice = 'e')
        {
            using (var writer = new StreamWriter(path))
            {
                foreach (var item in compressed)
                {
                    foreach (var graph in item.Value)
                    {

                        foreach (var edge in graph.Edges)
                        {
                            switch (choice)
                            {
                                case 'd':
                                    edge.Strength = 1 / this.playerDistanceStrength(graph.Nodes[edge.Row].XCoord, graph.Nodes[edge.Row].YCoord,
                                    graph.Nodes[edge.Col].XCoord, graph.Nodes[edge.Col].YCoord);
                                    break;

                            }
                        }
                        var sb = new StringBuilder(graph.ToStrengthString());
                        writer.WriteLine(sb.ToString());
                    }
                }
            }

        }
        #endregion
        
    }
}
