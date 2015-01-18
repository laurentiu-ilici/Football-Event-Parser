using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
namespace RawDataParser
{
    
    class DataReader
    {
        public IList<List<Info>> DataList { get; private set; }
        public List<long> ProblemsIds { get; private set; }
        private Dictionary<int, Coords> reverseEdgeDict = new Dictionary<int, Coords>();
        string path;
        string interruptionDataPath = @"D:\Work\PythonWorkspace\Football\ParsedByPlayerID\Interruptions.txt";
        public DataReader(string path,  int limit = int.MaxValue)
        {
            this.path = path;
            this.DataList = new List<List<Info>>();
            this.ProblemsIds = new List<long>();
            this.ReadData(limit);
            int value = 0;
            for (int row = 0; row < 16; row++)
            {
                for (int col = row + 1; col < 16; col++)
                {
                    this.reverseEdgeDict.Add(value, new Coords(row, col));
                    value++;
                }
            }
            this.excludeInterruptions();

        }

        private void excludeInterruptions()
        {
            var excludes = new List<long>();
            using (var reader = new StreamReader(this.interruptionDataPath))
            {
                string line = null;
                while((line = reader.ReadLine()) != null && line !="")
                {
                    string[] split = line.Split(' ');
                    excludes.Add(long.Parse(split[1]));
                    
                }
                
            }
            Dictionary<long,List<Info>> data = this.DataList.ToDictionary(item=> item[0].TimeStamp);
            foreach(var item in excludes)
            {
                if(data.ContainsKey(item))
                {
                    data.Remove(item);
                }
            }
            this.DataList.Clear();
            foreach(var item in data)
                this.DataList.Add(item.Value);
        }
        private void ReadData(int limit)
        {
            var file = new FileStream(this.path, FileMode.Open, FileAccess.Read);
            var reader = new StreamReader(file);
            string line = null;
            while ((line = reader.ReadLine()) != null && line != "" && DataList.Count < limit)
            {
                var dataPoint = new List<Info>();
                string[] split = line.Split(';');
                long ts = long.Parse(split[0]);

                for (int index = 1; index < split.Length; index++)
                {
                    var item = new Info(split[index]) {TimeStamp = ts};
                    //Always add the ball regardless of wheather it is on the field or not
                    if (index > 1 && item[0] != -1)
                    {

                        dataPoint.Add(item);
                    }
                    else if (index == 1)
                    {

                        dataPoint.Add(item);
                    }
                    else
                    {
                        item = item.CopyObject(DataList[DataList.Count - 1][index - 1]);
                        item.TimeStamp = ts;
                        if (!this.ProblemsIds.Contains(item.TimeStamp))
                            this.ProblemsIds.Add(item.TimeStamp);
                        dataPoint.Add(item);
                    }
                }
                this.DataList.Add(dataPoint);
            }

        }
        public Dictionary<string, long[]> ReadGraphStrings(string path = @"D:\Work\PythonWorkspace\Football\ParsedByPlayerID\SubDict.txt")
        {
            var result = new Dictionary<string, long[]>();
            using (var reader = new StreamReader(path))
            {
                string line = String.Empty;
                while ((line = reader.ReadLine()) != null && line != string.Empty)
                {
                    string[] splitKeyValue = line.Trim().TrimEnd('\n').Split(':');
                    if (splitKeyValue.Length != 2)
                        throw new Exception("Something is wrong with the graph data file");
                    string[] values = splitKeyValue[1].Trim().Split(' ');
                    var parsedValues = new long[values.Length];
                    for (int index = 0; index < values.Length; index++)
                    {
                        parsedValues[index] = long.Parse(values[index]);
                    }
                    result.Add(splitKeyValue[0], parsedValues);
                }
            }
            return result;

        }
        public List<Coords> ReadEdgeHistogram(string path,bool isClusterStatFile = false)
        {
            var result = new List<Coords>();
            using (var reader = new StreamReader(path))
            {
                string line = String.Empty;
                while ((line = reader.ReadLine()) != null && line != string.Empty)
                {
                    string[] splitValue = line.Trim().TrimEnd('\n').Split(':');
                    if (splitValue.Length != 2)
                        throw new Exception("Something is wrong with the histogram data file");
                    var newCoord = new Coords(this.reverseEdgeDict[int.Parse(splitValue[0])].ToString());
                    if (!isClusterStatFile)
                    {

                        newCoord.Frequency = double.Parse(splitValue[1]);
                      
                    }
                    else
                    {
                        string[] secondSplit = splitValue[1].Trim().Split(' ');
                        newCoord.Frequency = double.Parse(secondSplit[0]);
                        newCoord.Robustness = double.Parse(secondSplit[1]);
                        newCoord.StrongestCluster = int.Parse(secondSplit[2]);
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

    }
}
