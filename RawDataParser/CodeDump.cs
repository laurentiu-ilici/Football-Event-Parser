namespace RawDataParser
{
    class CodeDump
    {
        /*for (int index = 1; index < bindings.Count; index++)
   {
     AgregateForPlayer bla = new AgregateForPlayer(path + bindings[index][1].ToString() + ".txt", path + bindings[index][2].ToString() + ".txt", path, (long)bindings[index][0]);

   }*/
        /*List<string> paths = new List<string>();
        for (int index = 0; index < 18; index++)
        {
            paths.Add(path + index.ToString() + "Pid.txt");
        }
        AgregateActivity.AgregateAll(paths, path + "Final.txt");*/

        //ParseToSensorFile file = new ParseToSensorFile(@"E:\Work\Football\full-game");
        /* for (int index = 0; index < 107; index++)
        {
            if (File.Exists(@"E:\Work\Football\sensor" +index.ToString() + ".txt"))
            {
                ParseSenzorToAverageFile parser = new ParseSenzorToAverageFile(@"E:\Work\Football\", index);
            }
        }*/

        /*FileStream file = new FileStream(@"D:\Work\Football\RawDataParser\RawDataParser\Resources\config.txt", FileMode.Open, FileAccess.Read);
        StreamReader reader = new StreamReader(file);
        string line = null;
        List<int[]> bindings = new List<int[]>();
        while ((line = reader.ReadLine()) != null)
        {
            string[] split = line.Split(' ');
            int[] items = new int[split.Length];
            for (int index = 0; index < split.Length; index++)
                items[index] = int.Parse(split[index]);
            bindings.Add(items);
        }*/
       /* public List<Graph> Triangulation()
        {
            List<Graph> result = new List<Graph>();
            for (int index2 = 0; index2 < dataList.Count; index2++)
            {
                List<DelaunayTriangulator.Vertex> points = new List<DelaunayTriangulator.Vertex>();
                for (int index = 1; index < 23; index++)
                {

                    points.Add(new DelaunayTriangulator.Vertex((float)this.relativePosX(dataList[index2].Objects[index].XCoord),
                        (float)this.relativePosY(dataList[index2].Objects[index].YCoord)));
                }
                try
                {
                    Triangulator angulator = new Triangulator();
                    List<Triad> triangles = angulator.Triangulation(points, false);
                    List<SortedSet<int>> newList = this.constructList(triangles);
                    Graph newGraph = new Graph(newList, dataList[index2].FrameNumber);

                    if (result.Count == 0 || !result[result.Count - 1].IsEqual(newGraph))
                    {

                        for (int index = 0; index < this.dataList[index2].Objects.Count - 3; index++)
                        {
                            newGraph.Nodes.Add(this.dataList[index2].Objects[index]);
                        }
                        result.Add(newGraph);
                    }
                }
                catch
                {
                    Console.WriteLine("Problem with data point {0}", this.dataList[index2].FrameNumber.ToString());
                }

            }
            return result;
        }
        private List<SortedSet<int>> constructList(List<Triad> triangles)
        {
            List<SortedSet<int>> newList = new List<SortedSet<int>>();
            for (int index = 0; index < 22; index++)
            {
                newList.Add(new SortedSet<int>());
            }
            foreach (var item in triangles)
            {
                if (!newList[item.a].Contains(item.b))
                    newList[item.a].Add(item.b);
                if (!newList[item.a].Contains(item.c))
                    newList[item.a].Add(item.c);
                if (!newList[item.b].Contains(item.c))
                    newList[item.b].Add(item.c);
                if (!newList[item.b].Contains(item.a))
                    newList[item.b].Add(item.a);
                if (!newList[item.c].Contains(item.b))
                    newList[item.c].Add(item.b);
                if (!newList[item.c].Contains(item.a))
                    newList[item.c].Add(item.a);
            }
            return newList;
        }*/
    }
}
