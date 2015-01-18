using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
namespace RawDataParser
{
    //Nothing nice just bullshit code
    class ParseToSensorFile
    {
        Dictionary<int, StreamWriter> writers = new Dictionary<int, StreamWriter>();
        Dictionary<int, FileStream> files = new Dictionary<int, FileStream>();
        public ParseToSensorFile(string path)
        {
            var rawData = new FileStream(path, FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(rawData);
            string line = null;
            int lineCount = 0;
            var start = new DateTime();
            while ((line = sr.ReadLine()) != null)
            {
                string[] split = line.Split(',');
                int sid = int.Parse(split[0]);
                if (writers.Keys.Contains(sid))
                {
                    writers[sid].WriteLine(line);
                }
                else
                {
                    var file = new FileStream(@"E:\Work\Football\sensor" + sid.ToString() + ".txt", FileMode.OpenOrCreate, FileAccess.Write);
                    var sw = new StreamWriter(file);
                    sw.WriteLine(line);
                    writers.Add(sid, sw);
                    files.Add(sid, file);
                }
                lineCount++;
                if(lineCount % 100000 ==0)
                    Console.WriteLine("Parsed first {0}",lineCount);
            }
            foreach (var item in writers)
            {
                item.Value.Flush();
                item.Value.Close();
            }
            foreach (var item in files)
            {
                item.Value.Close();
            }
            Console.WriteLine("Process completed total number of line = {0}", lineCount);
            Console.WriteLine("Total amount of time needed {0}", (DateTime.Now - start).TotalMinutes.ToString());

       }
    }
}
