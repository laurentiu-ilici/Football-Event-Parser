using System;
using System.Collections.Generic;
using System.IO;
namespace RawDataParser
{
    //Agregates for left/right foot :). Also deletes 
    //any entries where he was not on the field.

    class AgregateForPlayer
    {
        
        public AgregateForPlayer(String pathRight, string pathLeft, string pathToWrite, long playerId)
        {
            var right = new FileStream(pathRight, FileMode.Open, FileAccess.Read);
            var left = new FileStream(pathLeft, FileMode.Open, FileAccess.Read);
           
            var readRight = new StreamReader(right);
            var readeLeft = new StreamReader(left);
            
            string line;
            var rightInf = new List<Info>();
            var leftInf = new List<Info>();
            var parsed = new List<Info>();
            while ((line = readRight.ReadLine()) != null)
            { 
                rightInf.Add(new Info(line));
            
            }
            rightInf.RemoveAt(rightInf.Count - 1);
            while ((line = readeLeft.ReadLine()) != null)
            { 
                leftInf.Add(new Info(line));
            
            }
            leftInf.RemoveAt(leftInf.Count - 1);
            long tsMin = Math.Min(rightInf[0][1],leftInf[0][1]);
            long tsMax = Math.Max(rightInf[rightInf.Count - 1][1], leftInf[leftInf.Count - 1][1]);
            int indexLeft = 0;
            int indexRight = 0;
            for (long ts = tsMin; ts < tsMax; ts++)
            {
                if (indexLeft < leftInf.Count && indexRight< rightInf.Count && leftInf[indexLeft][1] == ts && rightInf[indexRight][1] == ts )
                { 
                    Info newInf = leftInf[indexLeft].Average(rightInf[indexRight]);
                    if(newInf.IsOnField())
                           parsed.Add(newInf);
                    indexLeft++;
                    indexRight++;

                }
                else if (indexLeft < leftInf.Count && leftInf[indexLeft][1] == ts )
                {
                    
                    if (leftInf[indexLeft].IsOnField())
                        parsed.Add(leftInf[indexLeft]);
                    indexLeft++;
                    

                }
                else if (indexRight < rightInf.Count && rightInf[indexRight][1] == ts)
                {

                    if (rightInf[indexRight].IsOnField())
                        parsed.Add(rightInf[indexRight]);
                    indexRight++;
                }
                else
                {
                    Console.WriteLine("No data for ts {0}", ts);
                }
            }
           
            var result = new FileStream(string.Format("{0}{1}Pid.txt", pathToWrite, playerId), FileMode.OpenOrCreate, FileAccess.Write);
            var sw = new StreamWriter(result);
            foreach (var info in parsed)
            {
                info[0] = playerId;
                sw.WriteLine(info.ToString());
            }
            sw.Flush();
            sw.Close();
            readeLeft.Close();
            readRight.Close();
            right.Close();
            left.Close();
            result.Close();
        
        }
    }
}
