using System;
using System.Collections.Generic;
using System.Text;
using VISParser.FieldObjects;

namespace VISParser
{
    public sealed class GamePhase
    {
        List<Frame> frames = new List<Frame>();
        public int Predecessor { get; set; }
        public int Current { get; set; }
        public int Successor { get; set; }
        public GamePhase() { }
        public List<Frame> Frames { get { return this.frames; } }
        public int FrameCount { get { return this.frames.Count; } }
        public void AddFrame(Frame newFrame)
        {
            this.frames.Add(newFrame);
        }
        public Frame GetFrame(int index)
        {
            if (index >= this.frames.Count)
                throw new ArgumentException("Index out of bounds!");
            return this.frames[index];
        
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.Current + " " + this.Successor);
            return sb.ToString();
        }
        public string ToLongString()
        {
            var sb = new StringBuilder();
            sb.Append(this.ToLongString());
            sb.Append(" " + this.FrameCount.ToString());
            return sb.ToString();
        }

    }
}
