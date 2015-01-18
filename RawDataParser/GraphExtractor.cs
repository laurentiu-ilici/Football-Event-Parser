using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DelaunayTriangulator;
using BenTools.Mathematics;
using System.IO;
namespace RawDataParser
{
    class GraphExtractor
    {
        IList<List<Info>> dataList;
        
        List<long> timeStamps = new List<long>();
        public GraphExtractor(IList<List<Info>> dataList = null)
        {
            if( dataList != null)
                this.dataList = dataList;
           
        }
        private bool IsOnField(Vector vect)
        {
            if (vect[0] < 0 || vect[0] > 52483
                || vect[1] < -33960 ||
                vect[1] > 33965)
                return false;
            return true;
        }
        public List<Graph> Triangulation()
        {
            var result = new List<Graph>();
            for (int index2 = 0; index2 < dataList.Count; index2++)
            {
                var points = new List<Vertex>();
                for (int index = 1; index < 17; index++)
                {
                    int x = (int)this.dataList[index2][index].Data[2]/100;
                    int y = (int)this.dataList[index2][index].Data[3]/100;
                    points.Add(new Vertex(x, y));
                }
                try
                {
                    var angulator = new Triangulator();
                    List<Triad> triangles = angulator.Triangulation(points, false);
                    List<SortedSet<int>> newList = this.constructList(triangles);
                    var newGraph = new Graph(newList, dataList[index2][0].TimeStamp);
                    if (result.Count == 0 || !result[result.Count - 1].IsEqual(newGraph))
                    {
                        result.Add(newGraph);
                    }
                }
                catch 
                {
                    Console.WriteLine("Problem with data point {0}", this.dataList[index2][dataList[index2].Count-1].TimeStamp.ToString());
                }
                
            }
            return result;
        }

        private List<SortedSet<int>> constructList(List<Triad> triangles)
        {
            var newList = new List<SortedSet<int>>();
            for (int index = 0; index < 16; index++)
            { 
                newList.Add(new SortedSet<int>());
            }
            foreach(var item in triangles)
            {
                if(!newList[item.a].Contains(item.b))
                    newList[item.a].Add(item.b);
                if(!newList[item.a].Contains(item.c))
                    newList[item.a].Add(item.c);
                if(!newList[item.b].Contains(item.c))
                    newList[item.b].Add(item.c);
                if(!newList[item.b].Contains(item.a))
                    newList[item.b].Add(item.a);
                if(!newList[item.c].Contains(item.b))
                    newList[item.c].Add(item.b);
                if(!newList[item.c].Contains(item.a))
                    newList[item.c].Add(item.a);
            }
            return newList;
        }
        public List<Graph> FortuneVoronoi(List<Graph> toBeParsed, double epsilon)
        {
            var result = new List<Graph>();
            VoronoiGraph lastVoronoiGraph = null;
            Vector.Precision = 2;
            
            int countRejects = 0;
            for (int index = 0; index < toBeParsed.Count; index++)
            {

                var currentConfig = new List<BenTools.Mathematics.Vector>();
               
                for(int pidIndex = 1;pidIndex < toBeParsed[index].Nodes.Count-1;pidIndex++)
                {
                   var data = new double[2];
                   data[0] = toBeParsed[index].Nodes[pidIndex].Data[2];
                   data[1] = toBeParsed[index].Nodes[pidIndex].Data[3];
                   var vorVect = new Vector(data);
                   currentConfig.Add(new Vector(data));
                }
                VoronoiGraph vorGraph = Fortune.ComputeVoronoiGraph(currentConfig);
                var toBeDiscarded = new List<VoronoiEdge>();
                foreach (var item in vorGraph.Edges)
                {
                    if (item.IsPartlyInfinite)
                    {
                        Vector nonInfinite = double.IsInfinity(item.VVertexA.ElementSum) ? item.VVertexB : item.VVertexA;
                        if (!this.IsOnField(nonInfinite))
                        {
                            toBeDiscarded.Add(item);
                            continue;
                        }
                       
                    }
                    else if (!this.IsOnField(item.VVertexA) && this.IsOnField(item.VVertexB)
                                   || this.IsOnField(item.VVertexA) && !this.IsOnField(item.VVertexB))
                    {
                        Vector onField = this.IsOnField(item.VVertexA) ? item.VVertexA : item.VVertexB;
                        Vector position = item.DirectionVector * epsilon;
                        position += onField;
                        if (!this.IsOnField(position))
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
                    vorGraph.Edges.Remove(delete);
                vorGraph = this.mergeIdsToEdges(toBeParsed[index].Nodes, vorGraph);
                if (lastVoronoiGraph == null)
                {

                    
                    lastVoronoiGraph = vorGraph;
                }
                else
                {
                    if (this.acceptVoronoiGraph(lastVoronoiGraph, vorGraph, epsilon))
                    {                 
                        lastVoronoiGraph = vorGraph;
                    }
                    else
                    {
                        countRejects++;
                        continue;
                    }
                }

                Graph interpretation = this.interpretVoronoiGraph(toBeParsed[index].Nodes, toBeParsed[index].TimeStamp,vorGraph.Edges);
                if (result.Count == 0)
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
        private VoronoiGraph mergeIdsToEdges(List<Info> nodes, VoronoiGraph graph)
        {
            bool okLeft = false;
            bool okRight = false;
            foreach (var item in graph.Edges)
            {
                for (int pidIndex = 1; pidIndex < nodes.Count - 1; pidIndex++)
                {
                    var data = new double[2];
                    data[0] = nodes[pidIndex].Data[2];
                    data[1] = nodes[pidIndex].Data[3];
                    var vorVect = new Vector(data);
                    if (item.LeftData.Equals(vorVect))
                    {
                        item.LeftDataId = pidIndex;
                        okLeft = true;
                    }
                    else if (item.RightData.Equals(vorVect))
                    {
                        item.RightDataId = pidIndex;
                        okRight = true;
                    }
                    if (okLeft && okRight)
                        break;
                }
            }
            if (!okLeft || !okRight)
                throw new Exception("We didn't find the id's on merging????");
            return graph;
        
        }
        private bool acceptVoronoiGraph(VoronoiGraph lastGraph,VoronoiGraph testedGraph,double epsilon)
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
        private Graph interpretVoronoiGraph(List<Info> nodes, long timeStamp, BenTools.Data.HashSet<VoronoiEdge> edges)
        { 
            var graphEdges = new List<Coords>();
            foreach (var item in edges)
            {
                int idx = -1;
                int idy = -1;
                for (int pid = 1; pid < nodes.Count; pid++)
                {
                    if (item.LeftData[0] == nodes[pid][2] && item.LeftData[1] == nodes[pid][3]) 
                      idx = pid - 1;
                    if (item.RightData[0] == nodes[pid][2] && item.RightData[1] == nodes[pid][3])
                      idy = pid - 1;
                }
                if(idx == -1 || idy ==-1)
                    throw new Exception("Huston we have a problem again");
                if (idx < idy)
                { 
                    graphEdges.Add(new Coords(idx,idy));
                }
                else if(idy < idx)
                {
                    graphEdges.Add(new Coords(idy,idx));
                }
                else
                    throw new Exception("The ids should not be equal!!!");
            }
            return new Graph(nodes, graphEdges, timeStamp);
        }
        public List<Graph> ConvertStringToGraphs(string dataPath, string GraphStringsPath = @"D:\Work\PythonWorkspace\Football\ParsedByPlayerID\GraphStringsOldCompressed.txt", bool firstHalf=true)
        {
            var reader = new DataReader(dataPath);
            this.dataList = reader.DataList;
            Dictionary<long, int> timeStampIndex = new Dictionary<long, int>();
            for (int index = 0; index < this.dataList.Count; index++)
            {
                timeStampIndex.Add(this.dataList[index][dataList[index].Count - 1].TimeStamp, index);
            }
            Dictionary<string, long[]> sugraphs = reader.ReadGraphStrings(GraphStringsPath);
            List<Graph> result = new List<Graph>();
            foreach (var item in sugraphs)
            {
                List<Coords> edges = reader.ParseKey(item.Key);
                //Average over the coords of the "nodes" aka the players
                //in a very quick and dirty way :(
                List<Info> nodes = null;
                int nrOfGraphs = 0;
                for (int index = 0; index < item.Value.Length; index++)
                {
                    //Build for the first half only...
                    if (firstHalf && (item.Value[index] > 130866))
                        continue;
                    else if (!firstHalf && item.Value[index] < 130866)
                        continue;
                    nrOfGraphs++;
                    //It means the game was in an interrupted state so we can skip
                    //whatever position this was...
                    if(!timeStampIndex.ContainsKey(item.Value[index]))
                        continue;
                    int index2 = timeStampIndex[item.Value[index]];
                    if (nodes == null)
                    {
                        nodes = new List<Info>();
                        for (int playerID = 0; playerID < dataList[index2].Count; playerID++)
                        {
                            nodes.Add(new Info().CopyObject(dataList[index2][playerID]));
                        }
                        break;
                    }
                    else
                    {
                        for (int playerID = 0; playerID < dataList[index2].Count; playerID++)
                        {
                            for (int local = 0; local < nodes[playerID].Data.Length; local++)
                            {
                                nodes[playerID][local] += dataList[index2][playerID][local];
                            }
                        }
                    }
                }
                if (nodes != null)
                {
                    for (int playerID = 0; playerID < nodes.Count; playerID++)
                    {
                        for (int it = 0; it < nodes[playerID].Data.Length; it++)
                        {
                            nodes[playerID][it] /= nrOfGraphs;
                        }
                    }
                    var newGraph =new Graph(nodes, edges, item.Value[0]);
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
                    newNodes.Add(new Info().CopyObject(item));
                var newEdges = new List<Coords>();
                foreach (var edge in graph.Edges)
                {
                    double epsDistance = newNodes[edge.Row + 1].Distance(newNodes[edge.Col + 1])/2 + epsilon;
                    //Skip the ball and the ref
                    Info midPoint = new Info().CopyObject(newNodes[edge.Row + 1]).Average(newNodes[edge.Col + 1]);
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
                var newGraph = new Graph(newNodes,newEdges,graph.TimeStamp);
                if (newEdges.Count > 0 )
                    if(result.Count == 0)
                       result.Add(newGraph);
                    else if(!result[result.Count-1].IsEqual(newGraph))
                       result.Add(newGraph);
                

            }
            return result;
        }
        public Dictionary<string,List<Graph>> ConvertToUniqueGraphs(List<Graph> toConvert)
        {
            Dictionary<string, List<Graph>> result = new Dictionary<string, List<Graph>>();
            foreach(var item in toConvert)
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
        public void WriteToFile(string path,List<Graph> graphs)
        {
            using (var writer = new StreamWriter(path))
            {
                 foreach(var item in graphs)
                    writer.WriteLine(item.ToLongString());
            }   
        }
        public void WriteToFile(string path, Dictionary<string,List<Graph>> compressed)
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
    }
}
