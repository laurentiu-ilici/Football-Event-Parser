using System;
using System.Collections.Generic;
using System.IO;

namespace RawDataParser
{
    class ParseSenzorToAverageFile
    {
        public ParseSenzorToAverageFile(string dirPath, int sid)
        {
            long timeStep = 100000000000;
            var rawData = new FileStream(dirPath + "sensor" + sid.ToString() + ".txt", FileMode.Open, FileAccess.Read);
            var parsedData = new FileStream(dirPath+ "senzorParsed"+ sid.ToString() + ".txt", FileMode.OpenOrCreate, FileAccess.Write);
            var sr = new StreamReader(rawData);
            var sw = new StreamWriter(parsedData);
            string line = null;
            int lineCount = 0;
            var start = new DateTime();
            long lastTs = 0;
            var infoGatherer = new List<Info>();
            while ((line = sr.ReadLine()) != null)
            {

                string[] split = line.Split(',');
                var info = new long[split.Length];
                long timeStamp = long.Parse(split[1]);
                timeStamp /= timeStep;
                for (int index = 0; index < split.Length; index++)
                {
                    if (index == 1)
                    {
                        info[index] = timeStamp;
                        continue;
                    }
                    info[index] = long.Parse(split[index]);
                }

                if (lineCount == 0)
                    lastTs = timeStamp;
                if (lastTs == timeStamp)
                {
                    infoGatherer.Add(new Info(info));
                }
                else
                {
                    lastTs = timeStamp;
                    var finalData = new long[infoGatherer[0].Data.Length];
                    //We go from 0 as a sanity check, cause all sid should remain 
                    //unchanged and all timestamps should be unchanged
                    for (int col = 0; col < infoGatherer[0].Data.Length; col++)
                    {
                        long average = 0;
                        for (int row = 0; row < infoGatherer.Count; row++)
                        {
                            average += infoGatherer[row].Data[col];

                        }
                        finalData[col] = average / infoGatherer.Count;
                    }
                    sw.WriteLine((new Info(finalData)).ToString());
                    infoGatherer.Clear();
                    infoGatherer.Add(new Info(info));
                }
                lineCount++;
            }
            sw.Flush();
            sw.Close();
            rawData.Close();
            parsedData.Close();
            Console.WriteLine("Process completed total number of line = {0}", lineCount);
            Console.WriteLine("Total amount of time needed {0}", (DateTime.Now - start).TotalMinutes.ToString());
        }
    }
}
