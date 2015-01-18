using System;
using System.Collections.Generic;
using System.Linq;
using VISParser;
using System.Xml;
using VISParser.Events;
using VISParser.FieldObjects;

namespace FootballProject
{

    public sealed class FootballEventManager
    {
        public static Dictionary<string, FootballEventTypes> EventTypes;
        IList<VisualFootballEvent> footballEvents;
        public IList<VisualFootballEvent> DisplayedEvents { private set; get; }
        private IList<VisualComplexEvent> complexFootballEvents;
        public IList<Frame> Data { get; private set; }
        //The player for which we look for events
        private Player actingPlayer;
        //Used for identifying the player that receives passes at the moment, but this may change
        private Player targetPlayer;
        static FootballEventManager()
        {
            FootballEventManager.EventTypes = new Dictionary<string, FootballEventTypes>
            {
                {"None", FootballEventTypes.None},
                {"Corner Kick", FootballEventTypes.CornerKick},
                {"Goal Kick", FootballEventTypes.GoalKick},
                {"Offside", FootballEventTypes.Offside},
                {"Other fixture", FootballEventTypes.OtherFixture},
                {"Pass", FootballEventTypes.Pass},
                {"Shot", FootballEventTypes.Shot},
                {"Simple Possession", FootballEventTypes.SimplePossession},
                {"Throw In", FootballEventTypes.ThrowIn},
                {"Unknown Event", FootballEventTypes.UnknownEvent}
            };
        }
        public FootballEventManager(IList<Frame> data)
        {
            this.Data = data;
            this.DisplayedEvents = new List<VisualFootballEvent>();
        }
        #region Private and internal methods
        private void checkActingPlayer()
        {
            if (this.actingPlayer == null)
                throw new Exception("You need to set the acting player first!");
        }
        //Checkes wether the given sar was specified correctly
        private void checkSRAValidity(double x1, double y1, double x2, double y2)
        {
            if (x1 >= x2 || y2 >= y1)
                throw new ArgumentException("The specified rectangular area is incorrectly defined!");
        }
        private void parseEvents()
        {
            IList<FootballEvent> nonVisEvents = VISAPI.GetEvents(this.Data);
            this.DisplayedEvents.Clear();
            IList<VisualFootballEvent> visEvents = nonVisEvents.Select(fEvent => new VisualFootballEvent(fEvent)).ToList();
            this.footballEvents = visEvents;
            this.parseComplexEvents();
        }

        private void parseComplexEvents()
        {
            this.complexFootballEvents = new List<VisualComplexEvent>();
            for (int index = 0; index < this.footballEvents.Count; index++)
            {
                try
                {
                    Player actingPlayer = this.footballEvents.ElementAt(index).Event.ActingPlayer;
                    if (actingPlayer == null)
                        continue;
                    var currentTeam = actingPlayer.Team;
                    var eventList = new SortedList<int, FootballEvent>();
                    while (index < this.footballEvents.Count
                         && (this.footballEvents.ElementAt(index).Event.ActingPlayer == null ||
                         this.footballEvents.ElementAt(index).Event.ActingPlayer.Team == currentTeam))
                    {
                        eventList.Add(this.footballEvents.ElementAt(index).Event.EventStart, this.footballEvents.ElementAt(index).Event);
                        index++;
                    }
                    this.complexFootballEvents.Add(new VisualComplexEvent(new ComplexEvent(eventList), false));
                }
                catch
                {
                    Console.Write("shit");
                }
            }
            
        }
        internal void saveEvents(System.Xml.XmlElement projectRoot, System.Xml.XmlDocument doc)
        {
            System.Xml.XmlElement managerElem = doc.CreateElement("EventManager");
            projectRoot.AppendChild(managerElem);
            System.Xml.XmlElement footballEvents = doc.CreateElement("FootballEvents");
            managerElem.AppendChild(footballEvents);
            foreach (var footEvent in this.footballEvents)
            {
                footballEvents.AppendChild(footEvent.Event.ToXML(doc));
            }
        }
        internal void loadEvents(System.Xml.XmlElement manager, System.Xml.XmlDocument xmlProject)
        {
            this.footballEvents = new List<VisualFootballEvent>();
            var footballEventsXml = manager.FirstChild as XmlElement;
            if (footballEventsXml == null)
                throw new ArgumentException("The football events were not found in the given XML file. Please check the validity of the file!");
            foreach (var footEventXML in footballEventsXml.ChildNodes)
            {
                if (footEventXML is XmlElement)
                {
                    var currentNode = footEventXML as XmlElement;
                    this.footballEvents.Add(this.loadEvent(currentNode));
                }
            }
        }
        private VisualFootballEvent loadEvent(XmlElement currentNode)
        {
            FootballEvent result;
            Dictionary<int, Frame> frameDict = this.Data.ToDictionary(item => item.FrameNumber);
            int startFrameNumber = int.Parse(currentNode.Attributes.GetNamedItem("startFrameId").Value);
            int endFrameNumber = int.Parse(currentNode.Attributes.GetNamedItem("endFrameId").Value);
            int actingPlayerId = int.Parse(currentNode.Attributes.GetNamedItem("actingPlayerId").Value);
            int targetPlayerId = int.Parse(currentNode.Attributes.GetNamedItem("targetPlayerId").Value);
            int responsiblePlayerId = int.Parse(currentNode.Attributes.GetNamedItem("responsiblePlayer").Value);
            bool userDefinedEvent = bool.Parse(currentNode.Attributes.GetNamedItem("userDefined").Value);
            Frame startFrame = frameDict[startFrameNumber];
            Frame endFrame = frameDict[endFrameNumber];
            Player resposiblePlayer = null;
            Player actingPlayer = startFrame.GetPlayerById(actingPlayerId);
            Player targetPlayer = null;
            Player responsiblePlayer = null;
            if(targetPlayerId !=-1)
                targetPlayer = endFrame.GetPlayerById(targetPlayerId);
            if (responsiblePlayerId != -1)
                responsiblePlayer = startFrame.GetPlayerById(responsiblePlayerId);
            if (currentNode.Name == "SimplePos")
            {
                result = new SimplePossession(actingPlayer, startFrame, endFrame, userDefinedEvent);
            }
            else if (currentNode.Name == "Pass")
            {
              
                result = new Pass(actingPlayer, targetPlayer, startFrame, endFrame, userDefinedEvent);
            }
            else if (currentNode.Name == "ThrowIn")
            {
                result = new ThrowIn(responsiblePlayer,actingPlayer,targetPlayer, startFrame, endFrame, userDefinedEvent);
            }
            else if (currentNode.Name == "OtherFixture")
            {
                result = new OtherFixture(responsiblePlayer,actingPlayer,targetPlayer, startFrame, endFrame, userDefinedEvent);
            }
            else if (currentNode.Name == "CornerKick")
            {
                result = new CornerKick(responsiblePlayer,actingPlayer,targetPlayer, startFrame, endFrame, userDefinedEvent);
            }
            else if (currentNode.Name == "UnknownEvent")
            {
                result = new UnknownEvent(startFrame, endFrame);
            }
            else
                throw new ArgumentException(string.Format("Unrecognized {0} tag in the football events!", currentNode.Name.ToString()));
            return new VisualFootballEvent(result);
        }
        private bool checkSRAArea(Ball ball, double x1, double y1, double x2, double y2)
        {
            return ball.XCoord >= x1 && ball.XCoord <= x2 &&
                ball.YCoord <= y1 && ball.YCoord >= y2;
        }
        private VisualFootballEvent createNewEvent(FootballEventTypes type, int startFrameIndex, int endFrameIndex, int actingPlayerId, int targetPlayerId, int responsiblePlayerId)
        {
            VisualFootballEvent newVisualEvent = null;
            FootballEvent newEvent = null;
            Frame startFrame = this.Data[startFrameIndex];
            Frame endFrame = this.Data[endFrameIndex];
            Player actingPlayer=null;
            if(actingPlayerId !=-1)
                actingPlayer = startFrame.GetPlayerById(actingPlayerId) as Player;
            Player targetPlayer= null;
            if (targetPlayerId != -1)
                targetPlayer = this.Data[endFrameIndex].GetPlayerById(targetPlayerId);
            Player responsiblePlayer = null;
            if(responsiblePlayerId !=-1)
                responsiblePlayer = this.Data[endFrameIndex].GetPlayerById(responsiblePlayerId);
            switch (type)
            {
                case FootballEventTypes.Pass:
                    newEvent = new Pass(actingPlayer, targetPlayer, startFrame, endFrame, true);
                    break;
                case FootballEventTypes.SimplePossession:
                    newEvent = new SimplePossession(actingPlayer, startFrame, endFrame, true);
                    break;
                case FootballEventTypes.OtherFixture:
                    newEvent = new OtherFixture(responsiblePlayer,actingPlayer,targetPlayer, startFrame, endFrame, true);
                    break;

                case FootballEventTypes.CornerKick:
                    newEvent = new CornerKick(responsiblePlayer,actingPlayer,targetPlayer, startFrame, endFrame, true);
                    break;

                case FootballEventTypes.ThrowIn:
                    newEvent = new ThrowIn(responsiblePlayer,actingPlayer,targetPlayer, startFrame, endFrame, true);
                    break;
                case FootballEventTypes.UnknownEvent:
                    newEvent = new UnknownEvent(startFrame, endFrame);
                    break;
                case FootballEventTypes.GoalKick:
                    newEvent = new GoalKick(responsiblePlayer, actingPlayer, startFrame, endFrame,true);
                    break;
                case FootballEventTypes.Shot:
                    newEvent = new Shot(actingPlayer,startFrame,endFrame,false,true);
                    break;
                case FootballEventTypes.Offside:
                    newEvent = new Offside(responsiblePlayer,actingPlayer,startFrame,endFrame);
                    break;
            }
            newVisualEvent = new VisualFootballEvent(newEvent);
            return newVisualEvent;
        }
        #endregion
        #region Public methods
        public Player ActingPlayer
        {
            set
            {
                this.actingPlayer = value;
            }
        }
        public Player TargetPlayer
        {
            set
            {
                this.targetPlayer = value;
            }
        }
        public void ShowAllEvents()
        {
            if (this.footballEvents == null || this.footballEvents.Count == 0)
                this.parseEvents();
            this.DisplayedEvents.Clear();
            foreach (var fEvent in this.footballEvents)
            {
                this.DisplayedEvents.Add((fEvent));
            }
        }
        public void FilterPlayerToPlayerPasses()
        {
            if (this.actingPlayer == null ||
                this.targetPlayer == null)
                throw new Exception("Both players need to be set before completing this action!");
            this.DisplayedEvents.Clear();
            foreach (var footEvent in this.footballEvents)
            {
                if (footEvent.Event.EventType == FootballEventTypes.Pass)
                {
                    var pass = footEvent.Event as Pass;
                    if ((pass.ActingPlayer.ItemId == this.actingPlayer.ItemId)
                        && (this.targetPlayer.ItemId == pass.TargetPlayer.ItemId))
                    {
                        this.DisplayedEvents.Add(footEvent);
                    }
                }
            }
        }
        public void FilterPassesByLength(double minLength, double maxPasslength)
        {
            this.checkActingPlayer();
            this.DisplayedEvents.Clear();
            foreach (var footEvent in this.footballEvents)
            {
                if (footEvent.Event is Pass)
                {
                    var pass = footEvent.Event as Pass;
                    if (pass.ActingPlayer.ItemId == this.actingPlayer.ItemId
                        && pass.PassLength > minLength 
                        && maxPasslength > pass.PassLength)
                    {
                        this.DisplayedEvents.Add(footEvent);
                    }
                }
            }
        }
        public void FilterPassesByPlayerId()
        {
            this.checkActingPlayer();
            this.DisplayedEvents.Clear();
            foreach (var footEvent in this.footballEvents)
            {
                if (footEvent.Event is Pass)
                {
                    var pass = footEvent.Event as Pass;
                    if (pass.ActingPlayer.ItemId == this.actingPlayer.ItemId)
                    {
                        this.DisplayedEvents.Add(footEvent);
                    }
                }
            }
        }
        /// <summary>
        /// Filters the passes according to a given player and a specified rectangular area (SRA). 
        /// The area should be specified by its upper left corner (as you look at the screen) and 
        /// the lower right corner. (i.e. the NW corner and the SE corner).
        /// </summary>
        /// <param name="x1">X coordinate of the NW corner</param>
        /// <param name="y1">Y coordinate of the NW corner</param>
        /// <param name="x2">X coordinate of the SE corner</param>
        /// <param name="y2">Y coordinate of the SE corner</param>
        public void FilterPlayerToSRAPasses(double x1, double y1, double x2, double y2)
        {
            this.checkSRAValidity(x1, y1, x2, y2);
            this.checkActingPlayer();
            this.DisplayedEvents.Clear();
            foreach (var footEvent in this.footballEvents)
            {
                if (footEvent.Event.EventType == FootballEventTypes.Pass)
                {
                    var pass = footEvent.Event as Pass;
                    Ball ball = pass.EndFrame.GetBall();
                    if ((pass.ActingPlayer.ItemId == this.actingPlayer.ItemId) && this.checkSRAArea(ball, x1, y1, x2, y2))
                    {
                        this.DisplayedEvents.Add(footEvent);
                    }
                }
            }
        }
        /// <summary>
        ///  Filters the passes according to two given SRAs. The passes will always
        ///  originate in the first SRA and go to the secnod one.
        /// </summary>
        /// <param name="x11">X coordinate of the NW corner of the first SRA </param>
        /// <param name="y11">Y coordinate of the NW corner of the first SRA</param>
        /// <param name="x12">X coordinate of the SE corner of the first SRA</param>
        /// <param name="y12">Y coordinate of the SE corner of the first SRA</param>
        /// <param name="x21">X coordinate of the NW corner of the second SRA</param>
        /// <param name="y21">Y coordinate of the NW corner of the second SRA</param>
        /// <param name="x22">X coordinate of the SE corner of the second SRA</param>
        /// <param name="y22">Y coordinate of the SE corner of the second SRA</param>
        public void FilterSRAtoSRAPasses(double x11, double y11, double x12, double y12, double x21, double y21, double x22, double y22)
        {
            this.checkSRAValidity(x11, y11, x12, y12);
            this.checkSRAValidity(x21, y21, x22, y22);
            this.DisplayedEvents.Clear();
            foreach (var footEvent in this.footballEvents)
            {
                if (footEvent.Event.EventType == FootballEventTypes.Pass)
                {
                    var pass = footEvent.Event as Pass;
                    Ball ballStart = pass.StartFrame.GetBall();
                    Ball ballEnd = pass.EndFrame.GetBall();
                    if (this.checkSRAArea(ballStart, x11, y11, x12, y12) && this.checkSRAArea(ballEnd, x21, y21, x22, y22))
                    {
                        this.DisplayedEvents.Add(footEvent);
                    }
                }
            }
        }
      
       
        /// <summary>
        /// Filters the display to only show other fixtures found
        /// </summary>
        public void ApplyFilters(IList<FootballEventTypes> types)
        {
            this.DisplayedEvents.Clear();
            foreach (var footballEvent in this.footballEvents)
            {
                if (types.Contains(footballEvent.Event.EventType))
                    this.DisplayedEvents.Add(footballEvent);
            }
        }
        /// <summary>
        /// This method takes an event and transforms it to an "Unknown event". This is 
        /// the only form of "delete" I would like to include. 
        /// The workflow for the user would be:
        /// 1) Mark the event as an unknown event;
        /// 2) Every unknown event can be then transformed in to any event that the user 
        /// wants to transform it to...
        /// 
        /// In this way we can assure that a contiguous flow of events happen on the field, i.e.
        /// there are no frames that are not assigned to any event (Note: events can be left as unknown 
        /// and this will exclude them from the machine learning part).
        ///
        /// </summary>
        /// <param name="toDelete"> The event that will be transformed to an Unknown event, aka "deleted" </param>
        public void TransformToUnknown(VisualFootballEvent toModify)
        {
            int originalEventIndex = this.footballEvents.IndexOf(toModify);
            //Check the neighbours to see if they are Unknown as well:
            VisualFootballEvent leftNeighbour = originalEventIndex - 1 >= 0 ? this.footballEvents[originalEventIndex - 1] : null;
            VisualFootballEvent rightNeighbour = originalEventIndex + 1 < this.footballEvents.Count ? this.footballEvents[originalEventIndex + 1] : null;
            FootballEvent newUnknownEvent;
            //Make a single unknown event out of neighbouring unknown events
            if (leftNeighbour !=null && rightNeighbour !=null &&
                leftNeighbour.Event.EventType == FootballEventTypes.UnknownEvent
                && rightNeighbour.Event.EventType == FootballEventTypes.UnknownEvent)
            {
                newUnknownEvent = new UnknownEvent(leftNeighbour.Event.StartFrame, rightNeighbour.Event.EndFrame);
                this.footballEvents.Remove(rightNeighbour);
                this.footballEvents.Remove(leftNeighbour);
                
            }
            else if (leftNeighbour != null &&
                leftNeighbour.Event.EventType == FootballEventTypes.UnknownEvent)
            {
                newUnknownEvent = new UnknownEvent(leftNeighbour.Event.StartFrame, toModify.Event.EndFrame);
                this.footballEvents.Remove(leftNeighbour);
            }
            else if( rightNeighbour !=null 
                && rightNeighbour.Event.EventType == FootballEventTypes.UnknownEvent)
            {
                newUnknownEvent = new UnknownEvent(toModify.Event.StartFrame, rightNeighbour.Event.EndFrame);
                this.footballEvents.Remove(rightNeighbour);
            }
            else
            {
                newUnknownEvent = new UnknownEvent(toModify.Event.StartFrame, toModify.Event.EndFrame);
               
            }
            toModify.Event = newUnknownEvent as FootballEvent;
        }
        /// <summary>
        /// Displays only Unknown Events, i.e. used deleted events.
        /// </summary>
        public void FilterUnknownEvents()
        {
            this.DisplayedEvents.Clear();
            foreach (var footballEvent in this.footballEvents)
            {
                if (footballEvent.Event.EventType == FootballEventTypes.UnknownEvent)
                    this.DisplayedEvents.Add(footballEvent);
            }
        }

        /// <summary>
        /// Inserts a new user defined event into the event list. It does so by 
        /// making sure that the chain of events remain contiguous, i.e. we don't have any
        /// frames unassigned to any event. 
        /// </summary>
        /// <param name="originalEvent">The event that will be deleted from the event list</param>
        /// <param name="type">The type of the new event, e.g.: Pass, ThrowIn, etc.</param>
        /// <param name="startFrameId">The starting frame number for the new user defined event</param>
        /// <param name="endFrameId">The end frame number for the new user defined event </param>
        /// <param name="actingPlayerId">The player doing the event</param>
        /// <param name="passToId">The person that recieved the pass (if this was a pass event)</param>
        public void NewUserDefinedEvent(VisualFootballEvent originalEvent, FootballEventTypes type, int startFrameIndex, int endFrameIndex, int startFrameId, int endFrameId, int actingPlayerId, int targetPlayerId, int resposiblePlayerId)
        {
            
            int originalEventIndex = this.footballEvents.IndexOf(originalEvent);
            int originalStartFrameIndex = this.Data.IndexOf(originalEvent.Event.StartFrame);
            int originalEndFrameIndex = this.Data.IndexOf(originalEvent.Event.EndFrame);
            this.footballEvents.Remove(originalEvent);
            VisualFootballEvent newEvent = this.createNewEvent(type, startFrameIndex, endFrameIndex, actingPlayerId,targetPlayerId,resposiblePlayerId);
            VisualFootballEvent newUnknownEvent;
            this.footballEvents.Insert(originalEventIndex, newEvent);
            //Ensure continuity
            if (originalEvent.Event.StartFrame.FrameNumber == startFrameId)
            {
                if (originalEvent.Event.EndFrame.FrameNumber != endFrameId)
                {
                    //I am checking but it should never happen that this is true...
                    startFrameIndex = endFrameIndex + 1 < this.Data.Count ? endFrameIndex + 1 : endFrameIndex;
                    newUnknownEvent = this.createNewEvent(FootballEventTypes.UnknownEvent,
                                                                            startFrameIndex,
                                                                            originalEndFrameIndex,-1,-1,-1);
                    this.footballEvents.Insert(originalEventIndex + 1, newUnknownEvent);
                }
            }
            else if (originalEvent.Event.EndFrame.FrameNumber == endFrameId)
            {
                //I am checking but it should never happen that this is true...
                endFrameIndex = startFrameIndex - 1 > 0 ? startFrameIndex - 1 : startFrameIndex;
                newUnknownEvent = this.createNewEvent(FootballEventTypes.UnknownEvent,
                                                                           originalStartFrameIndex,
                                                                           endFrameIndex,-1,-1,-1);
                this.footballEvents.Insert(originalEventIndex, newUnknownEvent);
            }
            else
            {
                int newStartFrameIndex = endFrameIndex + 1 < this.Data.Count ? endFrameIndex + 1 : endFrameIndex;
                newUnknownEvent = this.createNewEvent(FootballEventTypes.UnknownEvent,
                                                                        newStartFrameIndex,
                                                                        originalEndFrameIndex,-1,-1,-1);
                this.footballEvents.Insert(originalEventIndex + 1, newUnknownEvent);
                int newEndFrameIndex = startFrameIndex - 1 > 0 ? startFrameIndex - 1 : startFrameIndex;
                newUnknownEvent = this.createNewEvent(FootballEventTypes.UnknownEvent,
                                                                           originalStartFrameIndex,
                                                                           newEndFrameIndex,-1,-1,-1);
                this.footballEvents.Insert(originalEventIndex, newUnknownEvent);
            }
        }
        public VisualFootballEvent FindCurrentDisplayedEvent(int frameNumber)
        {
            foreach (var item in this.DisplayedEvents)
            {
                if (item.ContainsFrame(frameNumber))
                    return item;
            }
            return null;
        }
        public void DisplayComplexEvents()
        {
            this.DisplayedEvents.Clear();
            foreach (var item in this.complexFootballEvents)
            {
                this.DisplayedEvents.Add(item);
            }
        }

        public IList<VisualFootballEvent> Events
        {
            get
            {
                return this.footballEvents;
            }
        }
        #endregion
    }
}

