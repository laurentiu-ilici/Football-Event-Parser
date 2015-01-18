using System;
using System.Collections.Generic;
using System.Linq;
using VISParser.Events;
using VISParser.FieldObjects;

namespace VISParser.Parsers
{
    sealed partial class EventParser
    {
        readonly IList<Frame> data;
        public static int StationaryBallLimit { get; set; }
        static EventParser()
        {
            if (EventParser.StationaryBallLimit == 0)
                EventParser.StationaryBallLimit = 20;
        }
        public EventParser(IList<Frame> data)
        {
            this.data = data;
        }
        #region Private Methods

        private BallState calculateBallState(int frameIndex)
        {
            var result = BallState.None;
            Ball ball = this.data[frameIndex].GetBall();
            result = ball.IsMoving ? result | BallState.IsMoving : result;
            result = this.data[frameIndex].PossessionId != -1 ? result | BallState.IsPossessd : result;
            result = ball.IsOffFieldLengthEast ? result | BallState.IsOffFieldLengthEast : result;
            result = ball.IsOffFieldLengthWest ? result | BallState.IsOffFieldLengthWest : result;
            result = ball.IsOutOfFieldWidth ? result | BallState.IsOffFieldWidth : result;
            return result;
        }
        private SortedList<int, BallStateEvent> parseBallEvents()
        {
            var result = new SortedList<int, BallStateEvent>();
            for (int index = 0; index < this.data.Count; index++)
            {
                int startFrameIndex = index;
                BallState currentBallState = this.calculateBallState(startFrameIndex);
         
                while (index + 1 < this.data.Count &&
                       (currentBallState ^ this.calculateBallState(index + 1)) == BallState.None)
                      // && this.data[index].PossessionId == this.data[index + 1].PossessionId)
                    index++;
                Player actingPlayer = this.data[index].GetPossessionPlayer();
                result.Add(this.data[startFrameIndex].FrameNumber, new BallStateEvent(actingPlayer, this.data[startFrameIndex], this.data[index], currentBallState));
            }
            return result;
        }

        private void parsePassess(SortedList<int, FootballEvent> result, SortedList<int, BallStateEvent> ballEvents, FootballEvent[] usedBallEvents)
        {
            for (int index = 0; index < ballEvents.Count; index++)
            {
                if (usedBallEvents[index] != null)
                    continue;
                var currBallStateEvent = ballEvents.ElementAt(index).Value;
                if (currBallStateEvent.IsMoving && currBallStateEvent.IsOnField
                    && index - 1 >= 0 && index + 1 < ballEvents.Count
                    && usedBallEvents[index - 1] != null
                    && usedBallEvents[index + 1] != null
                    && usedBallEvents[index - 1].EventType == FootballEventTypes.SimplePossession
                    && usedBallEvents[index + 1].EventType == FootballEventTypes.SimplePossession)
                {
                    Player actingPlayer = usedBallEvents[index - 1].ActingPlayer;
                    Player targetPlayer = usedBallEvents[index + 1].ActingPlayer;
                    var pass = new Pass(actingPlayer, targetPlayer,
                    currBallStateEvent.StartFrame, currBallStateEvent.EndFrame, false);
                    result.Add(pass.StartFrame.FrameNumber, pass);
                    usedBallEvents[index] = pass;


                }
            }
        }
        private IList<FootballEvent> parseEvents()
        {
            //Get the BallEvents first
            SortedList<int, BallStateEvent> ballEvents = this.parseBallEvents();
            //Based on them contruct the game relevant events.
            var result = new SortedList<int, FootballEvent>();
            var usedBallEvents = new FootballEvent[ballEvents.Count];
            //We rely on the fact that certain events are far more frequent than others.
            //We do multiple passes through the ball event list and at each pass we try to determine
            //another type of event, until we have used all the ball events at our disposal.
            //The way the algorithm work RELIES on a certain order of doing things as it builds 
            //upon event's from previous iterations...
            //First simple possesion and ball out of field events.
            for (int index = 0; index < ballEvents.Count; index++)
            {
                var currBallStateEvent = ballEvents.ElementAt(index).Value;
                if (currBallStateEvent.IsPossessed && currBallStateEvent.IsOnField)
                {
                    var simplePossession = new SimplePossession(currBallStateEvent.ActingPlayer,
                        currBallStateEvent.StartFrame, currBallStateEvent.EndFrame, false);
                    result.Add(simplePossession.StartFrame.FrameNumber, simplePossession);
                    usedBallEvents[index] = simplePossession;
                }
                if (currBallStateEvent.IsOffFieldWidth &&
                    (!currBallStateEvent.IsPossessed || !currBallStateEvent.IsMoving))
                {
                    var ballOffField = new BallOffField(null, null, currBallStateEvent.StartFrame,
                        currBallStateEvent.EndFrame);
                    result.Add(ballOffField.StartFrame.FrameNumber, ballOffField);
                    usedBallEvents[index] = ballOffField;
                }
            }
            //I will now compact the list, i.e. it could happen that we have several events from the previous
            //for loop that are the same because of certain corner cases in parsing the possession
            result = this.compressSimpleEvents(result);
            //Passes are the next more frequent events... If the pass happends from a field player
            //to the goalkeeper of the other team, we consider it a shot.
            this.parsePassess(result, ballEvents, usedBallEvents);
            //Next Throw-ins 
            result = this.parseThrowins(result, ballEvents, usedBallEvents);
            //Next shots...
            this.parseShots(result, ballEvents, usedBallEvents);
            //Lastly other fixtures
            this.parseOtherFixtures(result, ballEvents, usedBallEvents);
            //If anything got left behind add it as unknown:
            result= this.parseUnknownEvents(usedBallEvents);
            return result.Values.ToList();
        }

        private SortedList<int, FootballEvent> parseUnknownEvents(   FootballEvent[] usedBallEvents)
        {
            //Relies on the fact that the frames have keys that are IN ORDER. No missing values are allowed. 
            //This is intentional, and the data should be parsed to be in this way if we get data that does not
            //have sequential keys.
            var result = new SortedList<int, FootballEvent>();
            var newEvents =  new List<UnknownEvent>();
            foreach (var item in usedBallEvents)
            {
                if (item != null && !result.ContainsKey(item.EventStart))
                    result.Add(item.EventStart, item);
            }
            Dictionary<int, Frame> dataAsDict = this.data.ToDictionary(item => item.FrameNumber, item => item);
            for (int index = 1; index < result.Count; index++)
            {
               var currEvent = result.ElementAt(index).Value;
                var lastEvent = result.ElementAt(index - 1).Value;
                if (currEvent.EventStart != lastEvent.EventEnd + 1)
                {
                    newEvents.Add(new UnknownEvent(dataAsDict[lastEvent.EventEnd+1],dataAsDict[currEvent.EventStart-1]));
                }
            }
            foreach (var unknownEvent in newEvents)
            {
                result.Add(unknownEvent.EventStart,unknownEvent);
            }
            return result;
        }
        private void parseOtherFixtures(SortedList<int, FootballEvent> result, SortedList<int, BallStateEvent> ballEvents, FootballEvent[] usedBallEvents)
        {
            //Concatenate every unused event where the ball was not moving
            for (int index = 0; index < usedBallEvents.Length; index++)
            {
                if (usedBallEvents[index] != null || 
                    ballEvents.ElementAt(index).Value.IsMoving
                    || !ballEvents.ElementAt(index).Value.IsOnField) 
                    continue;
                int unusedStartIndex = index;
                while (index + 1 < usedBallEvents.Length &&
                        usedBallEvents[index+1]==null &&
                       !ballEvents.ElementAt(index+1).Value.IsMoving &&
                       ballEvents.ElementAt(index+1).Value.IsOnField)
                {
                    index++;
                }

                if (ballEvents.ElementAt(index).Value.EventEnd - ballEvents.ElementAt(unusedStartIndex).Value.EventStart < EventParser.StationaryBallLimit)
                {
                    var unknownEvent = new UnknownEvent(ballEvents.ElementAt(unusedStartIndex).Value.StartFrame,
                        ballEvents.ElementAt(index).Value.EndFrame);
                    for (int localIndex= unusedStartIndex; localIndex <= index; localIndex++)
                        usedBallEvents[index] = unknownEvent;
                    result.Add(unknownEvent.EventStart, unknownEvent);
                }
                else
                {
                    int fixtureStartIndex = unusedStartIndex-1;
                    while (fixtureStartIndex-1 >=0 
                           && (usedBallEvents[fixtureStartIndex] == null
                           || usedBallEvents[fixtureStartIndex].EventType != FootballEventTypes.SimplePossession))
                        fixtureStartIndex--;
                    Player responsiblePlayer = ballEvents.ElementAt(fixtureStartIndex - 1).Value.ActingPlayer;
                    int fixtureEndIndex = index;
                    while (fixtureEndIndex + 1 < usedBallEvents.Length
                           && (usedBallEvents[fixtureEndIndex + 1] == null
                               || usedBallEvents[fixtureEndIndex + 1].EventType != FootballEventTypes.Pass))
                        fixtureEndIndex++;
                    Player actingPlayer = usedBallEvents[fixtureEndIndex+ 1].ActingPlayer;
                    Player targetPlayer = usedBallEvents[fixtureEndIndex + 1].TargetPlayer;
                    FootballEvent newEvent;
                    Frame startFrame = ballEvents.ElementAt(fixtureStartIndex-1).Value.StartFrame;
                    Frame endFrame = ballEvents.ElementAt(fixtureEndIndex + 1).Value.EndFrame;
                    if (actingPlayer.Role == PlayerRoles.GoalKeeper)
                    {
                        newEvent = new GoalKick(responsiblePlayer, actingPlayer, startFrame,endFrame,false);

                    }
                    else if (PitchDimensionsHelper.IsCornerPossition(
                            ballEvents.ElementAt(fixtureEndIndex).Value.EndFrame.GetBall()))
                    {
                        newEvent = new CornerKick(responsiblePlayer,actingPlayer,targetPlayer,startFrame,endFrame,false);
                    }
                    else
                    {
                        newEvent = new OtherFixture(responsiblePlayer,actingPlayer,targetPlayer,startFrame,endFrame,false);
                    }
                    for (int localIndex = fixtureStartIndex-1; localIndex <= fixtureEndIndex+1; localIndex++)
                    {
                        usedBallEvents[localIndex] = newEvent;
                    }
                    if (result.ContainsKey(startFrame.FrameNumber))
                    {
                        result.Remove(startFrame.FrameNumber);
                    }
                    if (result.ContainsKey(endFrame.FrameNumber))
                        result.Remove(endFrame.FrameNumber);
                    result.Add(newEvent.EventStart, newEvent);
                }
            }
        }
        private void parseShots(SortedList<int, FootballEvent> partialResult, SortedList<int, BallStateEvent> ballEvents, FootballEvent[] usedBallEvents)
        {
            
            for (int index = 0; index < ballEvents.Count; index++)
            {
                var currBallStateEvent = ballEvents.ElementAt(index).Value;
                if (currBallStateEvent.IsOffFieldLength)
                {
                    var ballOffFieldStart = currBallStateEvent;
                    int startOffFIeldIndex = index;
                    int lastPossesionInField = index;
                    while (lastPossesionInField >= 0
                        && (!ballEvents.ElementAt(lastPossesionInField).Value.IsPossessed
                        || ballEvents.ElementAt(lastPossesionInField).Value.IsOffFieldLength))
                        lastPossesionInField--;
                    Player actingPlayer = usedBallEvents.ElementAt(lastPossesionInField).ActingPlayer;
                    var shot = new Shot(actingPlayer,
                        ballEvents.ElementAt(lastPossesionInField + 1).Value.StartFrame,
                        ballEvents.ElementAt(index - 1 > lastPossesionInField ? index - 1 : lastPossesionInField).Value.EndFrame, currBallStateEvent.IsOffFieldLengthEast, false);
                    for (int local = lastPossesionInField; local < index; local++)
                        usedBallEvents[local] = shot;
                    if (partialResult.ContainsKey(shot.EventStart))
                        partialResult.Remove(shot.EventStart);
                    partialResult.Add(shot.EventStart, shot);
                }
            }
            List<Shot> shots = new List<Shot>();
            foreach (var item in partialResult)
            {
                if (item.Value.EventType == FootballEventTypes.Pass
                    && item.Value.TargetPlayer.Role == PlayerRoles.GoalKeeper
                    && item.Value.ActingPlayer.Team != item.Value.TargetPlayer.Team)
                {
                    //TODO: revise this piece of code, because the ball can be travelling east...s
                    var shot = new Shot(item.Value.ActingPlayer, item.Value.StartFrame, item.Value.EndFrame, false,
                        false);
                    shots.Add(shot);
                }
            }
            foreach (var shot in shots)
            {
                partialResult[shot.EventStart] = shot;
            }

        }

        private SortedList<int, FootballEvent> parseThrowins(SortedList<int, FootballEvent> currentList, SortedList<int, BallStateEvent> ballEvents, FootballEvent[] usedBallEvents)
        {

            var result = new SortedList<int, FootballEvent>();
            for (int index = 0; index < currentList.Count; index++)
            {
                var currentEvent = currentList.ElementAt(index);
                if (currentEvent.Value.EventType != FootballEventTypes.BallOffField)
                {
                    result.Add(currentEvent.Key, currentEvent.Value);
                    continue;
                }
                int eventBallStateStartIndex = 0;
                for (; eventBallStateStartIndex < usedBallEvents.Length; eventBallStateStartIndex++)
                {
                    if (usedBallEvents[eventBallStateStartIndex] != null &&
                        usedBallEvents[eventBallStateStartIndex].StartFrame == currentEvent.Value.StartFrame)
                        break;
                }
                int eventBallStateEndIndex = eventBallStateStartIndex;
                Player actingPlayer = null;
                for (; eventBallStateEndIndex < usedBallEvents.Length; eventBallStateEndIndex++)
                {
                    //Set the player that throws the ball back in as the player who last "touched" the ball
                    if (usedBallEvents[eventBallStateEndIndex] != null &&
                        usedBallEvents[eventBallStateEndIndex].StartFrame.PossessionId != -1)
                        actingPlayer = usedBallEvents[eventBallStateEndIndex].StartFrame.GetPossessionPlayer();
                    if (usedBallEvents[eventBallStateEndIndex] != null &&
                        usedBallEvents[eventBallStateEndIndex].EventEnd == currentEvent.Value.EventEnd)
                        break;

                }
                //We need to look for the last possesion in order to establish who made the ball go out of the pitch
                int throwInStartIndex = eventBallStateStartIndex - 1;
                while (throwInStartIndex - 1 >= 0 &&
                       (usedBallEvents[throwInStartIndex] == null ||
                        usedBallEvents[throwInStartIndex].EventType != FootballEventTypes.SimplePossession))
                    throwInStartIndex--;
                int throwInEndIndex = eventBallStateEndIndex + 1;
                while (eventBallStateEndIndex < ballEvents.Count &&
                       (usedBallEvents[throwInEndIndex] == null ||
                        usedBallEvents[throwInEndIndex].EventType != FootballEventTypes.SimplePossession ||
                        actingPlayer != null &&
                        usedBallEvents[throwInEndIndex].ActingPlayer.ItemId == actingPlayer.ItemId))
                    throwInEndIndex++;
                Player responsiblePlayer = ballEvents.ElementAt(throwInStartIndex).Value.ActingPlayer;
                Player targetPlayer = ballEvents.ElementAt(throwInEndIndex).Value.ActingPlayer;
                var throwIn = new ThrowIn(responsiblePlayer, actingPlayer, targetPlayer,
                    ballEvents.ElementAt(throwInStartIndex + 1).Value.StartFrame,
                    ballEvents.ElementAt(throwInEndIndex - 1).Value.EndFrame, false);
                for (int localIndex = throwInStartIndex + 1; localIndex < throwInEndIndex; localIndex++)
                    usedBallEvents[localIndex] = throwIn;
                for (int localIndex = throwIn.EventStart; localIndex < throwIn.EventEnd; localIndex++)
                //Remove all the intermediary events that were used to construct the throw in. 
                if (result.ContainsKey(localIndex))
                {
                    result.Remove(localIndex);
                }
                    
                result.Add(throwIn.EventStart, throwIn);
            }
            return result;
        }

        private SortedList<int, FootballEvent> compressSimpleEvents(SortedList<int, FootballEvent> target)
        {
            var result = new SortedList<int, FootballEvent>();
            for (int index = 0; index < target.Count; index++)
            {
                try
                {
                   
                    FootballEvent currEvent = target.ElementAt(index).Value;
                    int startIndex = index;
                    while (index + 1 < target.Count &&
                           target.ElementAt(index + 1).Value.EventType == currEvent.EventType)
                    {
                        if (currEvent.EventType == FootballEventTypes.SimplePossession &&
                            target.ElementAt(index + 1).Value.ActingPlayer.ItemId == currEvent.ActingPlayer.ItemId)
                            index++;
                        else if (currEvent.EventType == FootballEventTypes.BallOffField)
                        {
                            index++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (startIndex != index)
                    {
                        switch (currEvent.EventType)
                        {
                            case FootballEventTypes.SimplePossession:
                                var possession = new SimplePossession(currEvent.ActingPlayer, currEvent.StartFrame,
                                    target.ElementAt(index).Value.EndFrame, false);
                                result.Add(currEvent.StartFrame.FrameNumber, possession);
                               
                                break;
                            case FootballEventTypes.BallOffField:
                                var ballOff = new BallOffField(currEvent.ActingPlayer, currEvent.TargetPlayer,
                                    currEvent.StartFrame, target.ElementAt(index).Value.EndFrame);
                                result.Add(currEvent.StartFrame.FrameNumber, ballOff);
                                break;
                        }
                    }
                    else
                    {
                        result.Add(currEvent.StartFrame.FrameNumber, currEvent);
                    }

                }
                catch (Exception)
                {

                    throw;
                }

                
            }
            return result;
        }

        #endregion
        #region Public Methods
        public IList<FootballEvent> ParseEvents()
        {
            IList<FootballEvent> result = this.parseEvents();
            //result = this.cleanupEvents(result);
            return result;
        }
        #endregion
    }

}
