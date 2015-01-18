using System;

namespace VISParser.FieldObjects
{
    public static class PitchDimensionsHelper
    {
        public const int xLength = 1050, yLength = 680;
        public const double GoalHeight = 2.44;
        //The goal is 7.32 meters in width...
        public static readonly double GoalNorthBar = yLength/2 - 36.6;
        public static readonly double GoalSouthBar = yLength/2 + 36.6;
        public static readonly double CornerKickAreaDefinition = 0.1;
        public static readonly double SmallPenaltyAreaNorthBorder = GoalNorthBar - 55;
        public static readonly double SmallPenaltyAreaSouthBorder = GoalSouthBar + 55;
        static System.Drawing.Point origin = new System.Drawing.Point(xLength / 2, yLength / 2);
        static public int RelativePosX(double xCoord)
        {
            if (xCoord < -1)
                xCoord = -1;
            if (xCoord > 1)
                xCoord = 1;
            int newCoord = (int)(PitchDimensionsHelper.origin.X * (1d + xCoord));
            return newCoord;

        }
        static public int RelativePosY(double yCoord)
        {
            if (yCoord < -1)
                yCoord = -1;
            if (yCoord > 1)
                yCoord = 1;
            int newCoord = (int)(PitchDimensionsHelper.origin.Y * (1d + yCoord));
            return newCoord;
        }
        //Takes the x,y of some object and calculates the distance according to 
        //the length and width of the pitch
        public static double CalculateDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(PitchDimensionsHelper.RelativePosX(x1)
                                    - PitchDimensionsHelper.RelativePosX(x2), 2) +
                                      Math.Pow(PitchDimensionsHelper.RelativePosY(y1) -
                                            PitchDimensionsHelper.RelativePosY(y2), 2));
        }

        public static bool IsCornerPossition(Ball ball)
        {
            return
                Math.Abs(ball.XCoord) > 1 - PitchDimensionsHelper.CornerKickAreaDefinition &&
                Math.Abs(ball.YCoord) > 1 - PitchDimensionsHelper.CornerKickAreaDefinition;
        }
    }
}
