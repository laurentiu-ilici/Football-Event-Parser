namespace FootballUI
{
    static class Debugger
    {
        public static void WriteMessage(string message)
        {
            using (var wr = new System.IO.StreamWriter("log.txt", true))
            {
                wr.WriteLine(message);
                wr.Flush();
                wr.Close();
            }
        }
    }
}
