using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VISParser.FieldObjects;

namespace VISParser
{
    
    public sealed class DataReader
    {
        public IList<Frame> DataList { get; private set; }
        private readonly Dictionary<int, Coords> reverseEdgeDict = new Dictionary<int, Coords>();
        private readonly string path;
        //This number sepresents the half of the match that the reader will read. 
        //By default it reads the first half.
        private static int section = 1;
        public DataReader()
        { 
        }
        public DataReader(string path, int limit = int.MaxValue)
        {
            this.path = path;
            this.DataList = new List<Frame>();
            this.readData(limit);
            int value = 0;
            for (int row = 0; row < 22; row++)
            {
                for (int col = row + 1; col < 22; col++)
                {
                    this.reverseEdgeDict.Add(value, new Coords(row, col));
                    value++;
                }
            }
        }
        private void readData(int limit)
        {
            if (path == null || !File.Exists(this.path))
                throw new ArgumentException("The histogramPa was not set correctly, cannot read the data!");
            var file = new FileStream(this.path, FileMode.Open, FileAccess.Read);
            var reader = new StreamReader(file);
            string line = null;
            while ((line = reader.ReadLine()) != null && line != "" && DataList.Count < limit)
            {
                
                string[] split = line.Split('#');
                Frame newFrame = this.parseFrame(split[0]);
                if (newFrame.Section != DataReader.section)
                    continue;
                newFrame.Objects.Add(new Ball(0,split[4].TrimEnd(';',' ')));
                newFrame.Objects.AddRange(this.parseTeam(split[1].TrimEnd(';'), 0));
                newFrame.Objects.AddRange(this.parseTeam(split[2].TrimEnd(';'), 1));
                newFrame.Objects.AddRange(this.parseRefs(split[3].TrimEnd(';')));


                this.DataList.Add(newFrame);
            }
        }
        private IEnumerable<Info> parseRefs(string data)
        {
            string[] split = data.Split(';');
            var result = new List<Referee>();
            int position = 23;
            foreach (string subString in split)
            {
                result.Add(new Referee(position,subString));
                position++;
            }
            return result;
        }
        private IEnumerable<Info> parseTeam(string data, int team)
        {
            var result = new List<Player>();
            int position = team == 0 ? 1 : 12;
            string [] split = data.Split(';');
            foreach (string token in split)
            {
                result.Add(new Player(position,token));
                position++;
            }
            return result;
        }
        private Frame parseFrame(string param)
        {
            string[] split = param.Split(',');
            var result = new Frame(param);
            return result;
        }
        public static int GameSection
        {
            get
            {
                return DataReader.section;
            }
            set
            {
                DataReader.section = value;
            }
        }
        public Dictionary<string, long[]> ReadGraphStrings(string path = @"D:\Work\PythonWorkspace\Football\ParsedByPlayerID\SubDict.txt")
        {
            var result = new Dictionary<string, long[]>();
            using (var reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null && line != string.Empty)
                {
                    string[] splitKeyValue = line.Trim().TrimEnd('\n').Split(':');
                    if (splitKeyValue.Length != 2)
                        throw new Exception("Something is wrong with the graph data file");
                    string[] values = splitKeyValue[1].Trim().Split(' ');
                    var parsedValues = new long[values.Length];
                    for (int index = 0; index < values.Length; index++)
                    {
                        parsedValues[index] = long.Parse(values[index], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    result.Add(splitKeyValue[0], parsedValues);
                }
            }
            return result;

        }
        public List<int> ReadTimestampsFromGraphStrings(string graphsPath = @"D:\Work\PythonWorkspace\Football\VISData\Graphs10.txt")
        {
            var result = new List<int>();
            using (var reader = new StreamReader(graphsPath))
            {
                string line = String.Empty;
                while ((line = reader.ReadLine()) != null && line != string.Empty)
                {
                    string[] splitKeyValue = line.Trim().TrimEnd('\n').Split(':');
                    if (splitKeyValue.Length != 2)
                        throw new Exception("Something is wrong with the graph data file");
                    string[] values = splitKeyValue[1].Trim().Split(' ');
                   
                    for (int index = 0; index < values.Length; index++)
                    {
                        result.Add(int.Parse(values[index], System.Globalization.CultureInfo.InvariantCulture));
                    }
                   
                }
            }
            result.Sort();
            return result;
            
        }
        public List<ClusterPoint> ReadPointMap(string path = @"D:\Work\PythonWorkspace\Football\Clusters\PointMap.txt")
        {
            var result = new List<ClusterPoint>();
            using (var reader = new StreamReader(path))
            {
                string line = String.Empty;
                while ((line = reader.ReadLine()) != null && line != string.Empty)
                {
                    result.Add(new ClusterPoint(line));

                }
            }
            return result;
        }
        public List<Coords> ReadEdgeHistogram(string histogramPath,bool isClusterStatFile = false)
        {
            var result = new List<Coords>();
            using (var reader = new StreamReader(histogramPath))
            {
                string line;
                while ((line = reader.ReadLine()) != null && line != string.Empty)
                {
                    string[] splitValue = line.Trim().TrimEnd('\n').Split(':');
                    if (splitValue.Length != 2)
                        throw new Exception("Something is wrong with the histogram data file");
                    var newCoord = new Coords(this.reverseEdgeDict[int.Parse(splitValue[0],System.Globalization.CultureInfo.InvariantCulture)].ToString());
                    if (!isClusterStatFile)
                    {
                        newCoord.Frequency = double.Parse(splitValue[1], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        string[] secondSplit = splitValue[1].Trim().Split(' ');
                        newCoord.Frequency = double.Parse(secondSplit[0], System.Globalization.CultureInfo.InvariantCulture);
                        newCoord.Robustness = double.Parse(secondSplit[1], System.Globalization.CultureInfo.InvariantCulture);
                        newCoord.StrongestCluster = int.Parse(secondSplit[2], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    result.Add(newCoord);
                }
            }
            return result;
        }
        public List<Coords> ParseKey(string key)
        {
            string[] split = key.Trim().Split(' ');
            return split.Select(int.Parse).Select(keyValue => this.reverseEdgeDict[keyValue]).ToList();
        }
        public MatrixWithIds ReadMatrix(string matrixPath)
        {
            if (matrixPath == null || !File.Exists(matrixPath))
                throw new ArgumentException("The histogramPa was not set correctly, cannot read the data!");
            var lines = new List<string>();
            var file = new FileStream(matrixPath, FileMode.Open, FileAccess.Read);
            var reader = new StreamReader(file);
            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }
            return new MatrixWithIds(lines);
        }

    }
}
