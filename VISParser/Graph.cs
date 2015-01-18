using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VISParser.FieldObjects;

namespace VISParser
{
    public sealed class Graph
    {

        public static EdgeDict EdgeDict = new EdgeDict();


        private void init()
        {

            this.Edges = new List<Coords>();
            this.Nodes = new List<Info>();

        }

        public int TimeStamp { get; set; }
        public Graph(List<Info> nodes, List<Coords> edges, int TimeStamp)
        {
            this.init();
            this.Nodes = nodes;
            this.Edges = edges;
            this.TimeStamp = TimeStamp;

        }
        public Graph(List<SortedSet<int>> edgeSet, int TimeStamp)
        {
            this.init();
            var adjacencyMatrix = new int[22, 22];
            for (int row = 0; row < edgeSet.Count; row++)
                for (int col = 0; col < edgeSet[row].Count; col++)
                    adjacencyMatrix[row, edgeSet[row].ElementAt(col)] = 1;
            for (int row = 0; row < adjacencyMatrix.GetLength(0); row++)
                for (int col = row + 1; col < adjacencyMatrix.GetLength(1); col++)
                    if (adjacencyMatrix[row, col] == 1)
                        this.Edges.Add(new Coords(row, col));
            this.TimeStamp = TimeStamp;

        }
        public Graph(string graphString, int TimeStamp)
        {
            this.init();
            string[] split = graphString.Split(' ');
            //May seem unnecessary but it may save my life at some point
            //because it will break down if you don't have the key,
            //and it ensures the string is sorted.
            var keys = new SortedSet<int>();
            for (int index = 0; index < split.Length; index++)
                keys.Add(int.Parse(split[index]));
            foreach (var key in keys)
            {
                this.Edges.Add(Graph.EdgeDict.GetReversedEdge(key));
            }
            this.TimeStamp = TimeStamp;

        }
        //Returns a vector that has a bit set to 1 if the corresponding
        //edge is present within the graph and 0 otherwise. 
        public ulong[] EdgeVector(bool negativeEdgesOnly = false)
        {
            var result = new ulong[Graph.EdgeDict.Count / 64 + 1];
            foreach (var edge in this.Edges)
            {
                if (negativeEdgesOnly && edge.Label == 1)
                    continue;
                int edgeId = Graph.EdgeDict.GetEdgeKey(edge.ToString());
                int resultIndex = edgeId / 64;
                int indexWithinUlong = edgeId % 64;
                ulong mask = 1;
                mask <<= 63 - indexWithinUlong;
                result[resultIndex] |= mask;
            }
            return result;
        }
        public int OccurenceCount { get; set; }
        public List<Info> Nodes { get; set; }
        public List<Coords> Edges { get; set; }
        public override string ToString()
        {
            var sb = new StringBuilder();
            var keys = new int[this.Edges.Count];
            for (int index = 0; index < keys.Length; index++)
            {
                try
                {
                    keys[index] = Graph.EdgeDict.GetEdgeKey(this.Edges[index].ToString());
                }
                catch
                {
                    keys[index] = -1;
                }
            }
            Array.Sort(keys);

            foreach (var item in keys)
            {
                //Means we got an exception. Still this should not happen,
                //I need to look into it.
                if (item == -1)
                    continue;
                sb.Append(item.ToString());
                sb.Append(" ");
            }
            return sb.Remove(sb.Length - 1, 1).ToString();
        }
        public string ToLongString()
        {
            var sb = new StringBuilder(this.ToString());
            sb.Append(":" + this.TimeStamp.ToString());
            return sb.ToString();

        }
        public string ToStrengthString()
        {
            var sb = new StringBuilder();
            foreach (var item in this.Edges)
            {

                if (Graph.EdgeDict.ContainsKey(item.ToString()))
                {
                    sb.Append(Graph.EdgeDict.GetEdgeKey(item.ToString()) + ":" + item.Strength);
                    sb.Append(" ");
                }

            }
            sb.Append("#" + this.TimeStamp.ToString());
            return sb.ToString();

        }
        internal bool IsEqual(Graph otherGraph)
        {
            try
            {
                return this.ToString() == otherGraph.ToString();
            }
            catch
            {
                Console.WriteLine("There was a problem with finding the string in the dictionary at timestamp = {0}", otherGraph.TimeStamp);
                Console.WriteLine("The graph was deleted");
                return false;
            }
        }
    }
}
