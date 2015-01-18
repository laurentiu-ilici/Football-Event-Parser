using System;
using System.Collections.Generic;
using System.IO;
namespace RawDataParser
{
    //This class converts files that contain 
    // normal time (expressed in hours:minutes:seconds.miliseconds) to
    // our timestamp system.
    public class TimeConverter
    {
        public const long FirstHalfStartTS = 107532;
        public const long FirstHalfEndTS = 123988;
        public const long SecondHalfStartTS = 130866;
        public const long SecondHalfEndTS = 148935;
        Dictionary<int, DateTime> startInterruption = new Dictionary<int, DateTime>();
        Dictionary<int, DateTime> endInterruption = new Dictionary<int, DateTime>();
        public TimeConverter() 
        { 
        
        }
        public void ConvertFile(string pathSource, string pathDestination, bool firstHalf = true)
        {
            string line = null;
            using (var reader = new StreamReader(pathSource))
            {
                //Skip the header and the first line of content
                reader.ReadLine();
                while ((line = reader.ReadLine()) != null && line != "")
                {
                    string[] split = line.Split(';');
                    if (split[split.Length - 1] != "empty")
                        break;
                    if (split[split.Length - 4].EndsWith("Begin"))
                    {
                        startInterruption.Add(int.Parse(split[split.Length-2]),Convert.ToDateTime(split[split.Length - 3]));


                    }
                    else if (split[split.Length - 4].EndsWith("End"))
                        endInterruption.Add(int.Parse(split[split.Length - 2]), Convert.ToDateTime(split[split.Length - 3]));
                
                }
                reader.Close();
            }
            long currentTS = firstHalf ? TimeConverter.FirstHalfStartTS : TimeConverter.SecondHalfStartTS;
            long endTs = firstHalf ? TimeConverter.FirstHalfEndTS : TimeConverter.SecondHalfEndTS;
            var excludes = new List<long>();
            foreach (var startTime in this.startInterruption)
            {
                TimeSpan intTime = startTime.Value - this.endInterruption[startTime.Key];
                TimeSpan playTime = TimeSpan.MinValue;
                if(this.startInterruption.ContainsKey(startTime.Key+1))
                {
                    playTime = this.endInterruption[startTime.Key] - this.startInterruption[startTime.Key + 1];
                }
                int stepCounter = 0;
                int tenthsOfSeconds = Math.Abs((int)intTime.TotalMilliseconds/100);
                while (stepCounter < tenthsOfSeconds)
                {
                    excludes.Add(currentTS);
                    currentTS++;
                    stepCounter += 1;
                }
                if (playTime != TimeSpan.MinValue)
                {
                    stepCounter = 0;
                    tenthsOfSeconds = Math.Abs((int)playTime.TotalMilliseconds / 100);
                    while (stepCounter < tenthsOfSeconds)
                    {
                        currentTS++;
                        stepCounter += 1;
                    }
                }
               
            }
            using (var writer = new StreamWriter(pathDestination))
            {
                for (int index = 0; index < excludes.Count; index++)
                {
                    //fix the problem with the missing data...(first half has 3 minutes of no data in the
                    //file but has data about the "interruptions")
                    if(excludes[index]  < endTs)
                        writer.WriteLine(index + " " + excludes[index].ToString());
                
                }
                writer.Flush();
                writer.Close();
            }
        }

    }
}
