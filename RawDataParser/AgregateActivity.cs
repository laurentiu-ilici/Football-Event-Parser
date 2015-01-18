using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace RawDataParser
{
    class AgregateActivity
    {
        public static void AgregateAll(List<string> pathsIn, string pathOut)
        {
            var dataList = new List<Dictionary<long, Info>>();
            int filtered = 0;
            foreach (var path in pathsIn)
            {
                var file = new FileStream(path, FileMode.Open, FileAccess.Read);
                var reader = new StreamReader(file);
                var dict = new Dictionary<long, Info>();
                string line = null;
                while ((line = reader.ReadLine()) != null && line != "")
                {
                    var info = new Info(line);
                    if (!dict.ContainsKey(info[1]))
                    {
                        dict.Add(info[1], info);

                    }
                    else 
                        filtered++;
                }
                dataList.Add(dict);
                reader.Close();
                file.Close();

            }
            Console.WriteLine("Filtered {0} entries", filtered.ToString());
            var activities = new List<string>();
            //First half with the part without ball data. The index is the timestamp
            for (long index = 107532; index < 123989; index++)
            {
                var sb = new StringBuilder(index.ToString());
               
                foreach(var item in dataList)
                {
                    if (item.ContainsKey(index))
                    {
                        sb.Append(";");
                        sb.Append(item[index].ToString());
                    }
                    else
                    {
                        var nullInfo = new Info(new long[13]);
                        nullInfo[0] = -1;
                        sb.Append(";");
                        sb.Append(nullInfo.ToString());
                    }
                }
                activities.Add(sb.ToString().TrimEnd(','));
            }
            //Second half
            for (long index = 130866; index < 1487963; index++)
            {
                var sb = new StringBuilder(index.ToString());
                bool ok = false;
                foreach (var item in dataList)
                {
                    if (item.ContainsKey(index))
                    {
                        sb.Append(";");
                        sb.Append(item[index].ToString());
                        ok = true;
                    }
                    else
                    {
                        var nullInfo = new Info(new long[13]);
                        nullInfo[0] = -1;
                        sb.Append(";");
                        sb.Append(nullInfo.ToString());
                    }
                }
                if(ok)
                   activities.Add(sb.ToString().TrimEnd(','));
            }
            var result = new FileStream(pathOut, FileMode.OpenOrCreate, FileAccess.Write);
            var sw = new StreamWriter(result);
            foreach (var item in activities)
            {
                sw.WriteLine(item);
            }
            sw.Flush();
            sw.Close();
            result.Close();   
        }
    }
}
