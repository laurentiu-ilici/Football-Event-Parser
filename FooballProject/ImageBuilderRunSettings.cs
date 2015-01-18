using System;

namespace FootballProject
{
    [Serializable]
    public sealed class ImageBuilderRunSettings : RunSettings
    {
        public ImageBuilderRunSettings() { }
        public bool IncludePossession { get; set; }
        public bool IncludeGraphs { get; set; }
        public bool ParallelParsing { get; set; }
       
    }
}
