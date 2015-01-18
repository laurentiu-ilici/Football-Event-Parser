using System;
using System.Collections.Generic;
using System.Xml;
using VISParser;
using VISParser.FieldObjects;

namespace FootballProject
{
    
    public sealed class FootballProject
    {
        private string projectPath;
        private string rawDataPath;
        
        public const string ProjectFileName = @"\Project.xml";
        public const string RawDataDir = @"\RawData";
        public const string MatricesResults = @"\Results\Matrices";
        public const string MapsResults = @"\Results\Maps";
        public const string UserDefinedEvents = @"\UserDefinedEvents";
        public const string FrameFolder = @"\Frames";
        public const string FirstHalf = @"\FirstHalf";
        public const string SecondHalf = @"\SecondHalf";
        public FootballEventManager FootballEventManager { private set; get; }
        
        //This is a variable that represents the half of the game we are currently working with.
        //It is called this way because this is how it is called in the data provided by the football guys.
        public int Section {get; set;}
        public FootballProject(string projectPath, string rawDataPath,int maxPlayerBallDistance, int maxBallHeight,
            int maxBallAngle,int stationaryBallLimit)
        {
            VISAPI.SetSettings(maxPlayerBallDistance, maxBallHeight, maxBallAngle, stationaryBallLimit);
            this.projectPath = projectPath;
            this.rawDataPath = rawDataPath;
            this.clearDirectory();
            this.createDirectoryStructure();
            this.copyRawData();
            //Implicitly we work with the first half.
            this.Section = 1;
            this.FootballEventManager = new FootballEventManager(this.GetPossesionRawData());

            
        }
        private FootballProject(XmlDocument xmlProject)
        {
            var root = xmlProject.SelectSingleNode("Project") as XmlElement;
            if (root == null)
                throw new ArithmeticException("The specified xml project file is not valid!");
            this.projectPath = root.Attributes["projectPath"].Value.ToString();
            this.rawDataPath = root.Attributes["rawDataPath"].Value.ToString();
            //Section will be deleted when we get to another type of parsing
            this.Section = int.Parse(root.Attributes["section"].Value.ToString());
            this.FootballEventManager = new FootballEventManager(this.GetPossesionRawData());
            var manager = xmlProject.SelectSingleNode("//Project/EventManager") as XmlElement;
            this.FootballEventManager.loadEvents(manager, xmlProject);

        }
        private void copyRawData()
        {
           string[] split = this.rawDataPath.Split('\\');
           string newDataLocation = projectPath + FootballProject.RawDataDir + '\\' + split[split.Length-1];
           System.IO.File.Copy(rawDataPath, newDataLocation);
           this.rawDataPath = newDataLocation;
        }
        private void createDirectoryStructure()
        {
            var dir = new System.IO.DirectoryInfo(this.projectPath);
            dir.Create();
            System.IO.Directory.CreateDirectory(dir.FullName + FootballProject.FrameFolder);
            System.IO.Directory.CreateDirectory(dir.FullName + FootballProject.FrameFolder + FootballProject.FirstHalf);
            System.IO.Directory.CreateDirectory(dir.FullName + FootballProject.FrameFolder + FootballProject.SecondHalf);
            System.IO.Directory.CreateDirectory(dir.FullName + FootballProject.MapsResults);
            System.IO.Directory.CreateDirectory(dir.FullName + FootballProject.MatricesResults);
            System.IO.Directory.CreateDirectory(dir.FullName + FootballProject.RawDataDir);
            System.IO.Directory.CreateDirectory(dir.FullName + FootballProject.UserDefinedEvents);
        }
        private void clearDirectory()
        {
            try
            {
               if(!System.IO.Directory.Exists(this.projectPath))
                   throw new Exception("The new project directory was not found");
               System.IO.Directory.Delete(this.projectPath, true);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public string ProjectPath
        {
            get
            {
                return this.projectPath;
            }
        }

        public void SaveProject()
        {
            var doc = new XmlDocument();
            XmlElement projectRoot = doc.CreateElement("Project");
            doc.AppendChild(projectRoot);
            XmlAttribute projectPath = doc.CreateAttribute("projectPath");
            projectPath.Value = this.projectPath;
            XmlAttribute rawDataPath = doc.CreateAttribute("rawDataPath");
            rawDataPath.Value = this.rawDataPath;
            projectRoot.Attributes.Append(projectPath);
            projectRoot.Attributes.Append(rawDataPath);
            XmlAttribute section = doc.CreateAttribute("section");
            section.Value = this.Section.ToString();
            projectRoot.Attributes.Append(section);
            this.FootballEventManager.saveEvents(projectRoot,doc);
            doc.Save(this.projectPath + FootballProject.ProjectFileName); 
        }
        public static FootballProject LoadProject(string path)
        {
            var doc = new XmlDocument();
            doc.Load(path);
            var project = new FootballProject(doc);
            return project;
        }
        public string RawDataPath
        {
            get { return this.rawDataPath; }
        }
        public void BuildImages()
        {

        }
        public IList<Frame> GetPossesionRawData()
        {
             IDictionary<int,Frame> bla = new SortedDictionary<int,Frame>();
            return VISParser.VISAPI.GetPossesionRawData(this.rawDataPath, this.Section);
        }
        public void ShowAllEvents()
        {
            this.FootballEventManager.ShowAllEvents();
            
        }
    }
}