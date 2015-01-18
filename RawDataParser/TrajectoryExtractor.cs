using System;
using System.Collections.Generic;
using System.Text;

namespace RawDataParser
{
    class TrajectoryExtractor
    {
        List<List<Info>> dataList = new List<List<Info>>();
        public TrajectoryExtractor()
        {

        }
        public TrajectoryExtractor(List<List<Info>> dataList)
        {
            this.dataList = dataList;
        }
        private bool checkForward(bool firstHalf, int team, long epsilon, Info lastRecord, Info newRecord)
        {
            if (team == 0)
            {
                if (firstHalf)
                {
                    if (lastRecord[3] + epsilon > newRecord[3])
                        return true;
                    return false;
                }
                else
                {
                    if (lastRecord[3] - epsilon < newRecord[3])
                        return true;
                    return false;
                }
            }
            else
            {
                if (firstHalf)
                {
                    if (lastRecord[3] - epsilon < newRecord[3])
                        return true;
                    return false;
                }
                else
                {
                    if (lastRecord[3] + epsilon > newRecord[3])
                        return true;
                    return false;
                }
            }

        }
        //Hmm how small should epsilon be? I guess very small would be the answer.
        public List<List<Info>> GetForwardTrajectories(bool firstHalf, int playerId, long epsilon)
        {
            var result = new List<List<Info>>();
            int team = playerId%2;
            for (int index = 0; index < this.dataList.Count;)
            {
                if (firstHalf && this.dataList[index][0].TimeStamp > 130866)
                {
                    index++;
                    continue;
                }
                else if (firstHalf == false && this.dataList[index][0].TimeStamp < 130866)
                {
                    index++;
                    continue;
                }
                var forward = new List<Info>();
                forward.Add(this.dataList[index][playerId]);
                index++;
                while(index < dataList.Count)
                {
                    if (checkForward(firstHalf, team, epsilon, forward[forward.Count - 1], dataList[index][playerId]))
                    {
                        forward.Add(dataList[index][playerId]);
                        index++;
                    }
                    else
                    {
                        break;
                    }
                }
                if (forward.Count > 1)
                    result.Add(forward);
                
            }
            return result;

        }
        public void WriteToFile(string path, List<List<Info>> data)
        {
            using(var sw = new System.IO.StreamWriter(path))
            {
                for(int index =0; index < data.Count;index++)
                {
                    var builder = new StringBuilder();
                    foreach(var item in data[index])
                    {
                        
                        builder.Append(item[2].ToString() + " "+ item[3].ToString() + " " + index.ToString());
                        builder.Append(Environment.NewLine);
                    }
                    sw.Write(builder.ToString());
                }
                sw.Flush();
                sw.Close();
            }
            
        }

    }
}
