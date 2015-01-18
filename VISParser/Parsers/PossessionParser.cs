using System;
using System.Collections.Generic;
using System.Globalization;
using BenTools.Mathematics;
using VISParser.FieldObjects;

namespace VISParser.Parsers
{
    public sealed class PossessionParser
    {
        readonly List<Frame> data = new List<Frame>();
        //Height of the ball in decimeters
        public static int MaxBallHeight { get; set; }
        //The maximum angle that that we allow for posesion to not be changed in degrees. 
        public static int MaxAcceptedAngle { get; set; }
        //Distance from the ball to the player in decimeters.
        public static int MaxBallDistance { get; set; }
        private const double Tolerance = 0.00000001d;
        static PossessionParser()
        {
            if (PossessionParser.MaxAcceptedAngle == 0)
            {
                PossessionParser.MaxAcceptedAngle = 5;
            }
            if (PossessionParser.MaxBallDistance == 0)
            {
                PossessionParser.MaxBallDistance = 30;
            }
            if (PossessionParser.MaxBallHeight == 0)
            {
                PossessionParser.MaxBallHeight = 25;
            }
        }

        public PossessionParser(List<Frame> data)
        {
            this.data = data;

        }
        public void ParsePossession()
        {
            Player lastPlayer = null;
            Vector previousBallDirection = null;
            for (int index = 1; index < this.data.Count; index++)
            {
                Ball ball = this.data[index].GetBall();
                if (Math.Abs(ball.Speed) < PossessionParser.Tolerance)
                {
                    lastPlayer = this.calculatePossession(ball, this.data[index].Objects);
                    previousBallDirection = null;
                    if(lastPlayer != null)
                    {
                        lastPlayer.HasPossession = true;
                        this.data[index].PossessionId = lastPlayer.ItemId;
                    }
                }
                else
                {
                    Ball previousFrameBall = this.data[index - 1].GetBall();
                    var lastBallPos = new Vector(previousFrameBall.Coords);
                    var currentBallPos = new Vector(ball.Coords);
                    Vector ballDirection = currentBallPos - lastBallPos;
                    if (previousBallDirection == null)
                    {
                        previousBallDirection = ballDirection;
                        lastPlayer = this.calculatePossession(ball, this.data[index].Objects);
                        if (lastPlayer != null)
                        {
                            lastPlayer.HasPossession = true;
                            this.data[index].PossessionId = lastPlayer.ItemId;
                        }
                    }
                    else
                    {
                        if (Math.Abs(this.calculateAngle(ballDirection, previousBallDirection)) < PossessionParser.MaxAcceptedAngle
                            && lastPlayer != null)
                        {
                            Player currentPlayerPosition = this.data[index].GetPlayerById(lastPlayer.ItemId);
                            if (PitchDimensionsHelper.CalculateDistance(ball.XCoord,
                                ball.YCoord,
                                currentPlayerPosition.XCoord,
                                currentPlayerPosition.YCoord)
                                < PossessionParser.MaxBallDistance)
                            {
                                currentPlayerPosition.HasPossession = true;
                                lastPlayer = currentPlayerPosition;
                                this.data[index].PossessionId = lastPlayer.ItemId;
                            }
                            //TODO: remove debugging code
                            else
                            {
                                currentPlayerPosition.DebugPossesionInfo = "Ball loss => dist";
                            }
                        }
                        else
                        {
                            lastPlayer = this.calculatePossession(ball, this.data[index - 1].Objects, true);
                            if (lastPlayer != null)
                            {
                                lastPlayer.HasPossession = true;
                                this.data[index - 1].PossessionId = lastPlayer.ItemId;
                                Player currentPlayerPosition = this.data[index].GetPlayerById(lastPlayer.ItemId);
                                if (PitchDimensionsHelper.CalculateDistance(ball.XCoord,
                                   ball.YCoord,
                                   currentPlayerPosition.XCoord,
                                   currentPlayerPosition.YCoord)
                                   < PossessionParser.MaxBallDistance)
                                {
                                    currentPlayerPosition.HasPossession = true;
                                    lastPlayer = currentPlayerPosition;
                                    this.data[index].PossessionId = lastPlayer.ItemId;
                                }
                                //TODO: Remove debugging Code:
                                lastPlayer.DebugPossesionInfo += ",angle changed";
                            }
                        }
                        previousBallDirection = ballDirection;
                    }
                }
            }
            this.repairPossesionGaps();
        }
        private bool checkBound(int index)
        {
            return index < this.data.Count;
        }
        //When I parse the possesion it sometimes happens that you get a sequence
        //where a player has the ball, loses it for several frames, and then regains it without
        //any other player meeting the criteria of possesing the ball. This aditional method
        //aims to fill such gaps. I am doing it separately as we are not sure at the moment if
        //this is the permanent solution to the problem.
        //Not the best implementation possible, but the quickest that came to mind.
        private void repairPossesionGaps()
        {
            int index = 0;
            do
            {
                while (this.checkBound(index + 1) &&
                            this.data[index].PossessionId == -1)
                {
                    index++;
                }
                //If we've reached the end of the data points we exit the loop
                Frame frame = this.data[index];
                if (frame.PossessionId == -1)
                    break;
                int currentPossesionId = frame.PossessionId;
                int backwardsIndex = index - 1;
                while (backwardsIndex > 0 && this.data[backwardsIndex].PossessionId == -1)
                    backwardsIndex--;
                if (this.data[backwardsIndex].PossessionId == currentPossesionId)
                    while (backwardsIndex < index)
                    {
                        this.data[backwardsIndex].PossessionId = currentPossesionId;
                        this.data[backwardsIndex].GetPlayerById(currentPossesionId).HasPossession = true;
                        backwardsIndex++;
                        //Remove debugging info as there were "mistakes" along the way.
                        //this.data[backwardsIndex].GetPlayerById(currentPossesionId).DebugPossesionInfo = string.Empty;
                    }
                while (this.checkBound(index + 1) &&
                    this.data[index].PossessionId == currentPossesionId)
                {
                    index++;
                }
            }
            while (this.checkBound(index));
        }
        private double calculateAngle(Vector directionVector, Vector lastDirectionVector)
        {
            double dotProduct = directionVector * lastDirectionVector;
            double lengthProduct = Math.Sqrt(directionVector.SquaredLength) * Math.Sqrt(lastDirectionVector.SquaredLength);
            //For undefined angles just check for the nearest player again.
            if (Math.Abs(lengthProduct) < PossessionParser.Tolerance)
                return double.MaxValue;
            return Math.Acos(dotProduct / lengthProduct) * 180 / Math.PI;
        }
        private Player calculatePossession(Ball ball, List<Info> list, bool checkMinDist = true)
        {
            if (ball.ZCoord > PossessionParser.MaxBallHeight)
                return null;
            double minDistance = double.MaxValue;
            Player theClosest = null;
            foreach (var item in list)
            {
                if (item is Player)
                {
                    var player = item as Player;
                    double distance = PitchDimensionsHelper.CalculateDistance(player.XCoord, player.YCoord, ball.XCoord, ball.YCoord);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        theClosest = player;
                    }
                }
            }
            if (checkMinDist && minDistance < PossessionParser.MaxBallDistance)
            {
                //TODO: remove debugging coded
                theClosest.DebugPossesionInfo = String.Format("ID={2}, dist={0}, bSpeed={1}", minDistance.ToString(CultureInfo.InvariantCulture), ball.Speed, theClosest.ItemId);
                return theClosest;
            }
            return checkMinDist == false ? theClosest : null;
        }
        public List<Frame> Data
        {
            get
            {
                return this.data;
            }
        }
    }
}
