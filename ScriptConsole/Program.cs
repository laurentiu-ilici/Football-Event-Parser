using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VISParser;

namespace ScriptConsole
{
    class Program
    {
        static void Main(string[] args)
        {


        }
        public static void OutputGraphCounts()
        {
            string dataPath = @"E:\Work\Football\2011-12 BL 17.Sp. K'lautern vs. Hannover\130375.pos";
            VISAPI.(dataPath, @"e:\epsilonResults.txt", false, 1, false, true, null);

        }
    }
}
