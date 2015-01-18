using System.IO;
namespace VISParser
{
      public static class Logger
      {
          public static void LogMessage(string message)
          {
              using (TextWriter writer = new StreamWriter(@"D:\log.txt", true))
              {
                  writer.WriteLine(message);
                  writer.Flush();
              }
          }
      }
}
