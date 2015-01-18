using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
namespace RawDataParser
{
    class ParseBallData
    {
        public static void FilterOutOfFieldData( List<string> pathsIn, string pathOut)
        {
            var parsedData = new List<Info>();
            int filtered = 0;
            foreach (var path in pathsIn)
            {
                var file = new FileStream(path, FileMode.Open, FileAccess.Read);
                var reader = new StreamReader(file);
                string line = null;
                while ((line = reader.ReadLine()) != null && line != "")
                {
                    var info = new Info(line);
                    if (info.IsOnField())
                    {
                        parsedData.Add(info);
                        
                    }
                    else filtered++;
                }
                reader.Close();
                file.Close();
            }
            Console.WriteLine("Filtered {0} entries", filtered.ToString());
            var parsed = new FileStream(pathOut, FileMode.OpenOrCreate, FileAccess.Write);
            var sw = new StreamWriter(parsed);
            parsedData = parsedData.OrderBy(x=>x.Data[1]).ToList();
            foreach (var item in parsedData)
            {
                item[0] = 0;
                sw.WriteLine(item.ToString());
            }
            sw.Flush();
            sw.Close();
            parsed.Close();

        
        }
    }
}
