using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VISParser.FieldObjects
{
    //Semi-deprechated. Don't know if this will be part of the last
    //tool...
    public sealed class Field
    {

        private readonly int fieldLength;
        private readonly int fieldWidth;
        private IList<System.Drawing.PointF> queryPointGrid;
        public IList<System.Drawing.PointF> QueryPointGrid
        {
            private set
            {
                this.queryPointGrid = value;
            }
            get
            {
                return this.queryPointGrid;
            }
        }
        private int squareToIndex(int squareIndex)
        {
            throw new NotImplementedException();
        }
        private double distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y2 - y1, 2));
        }
        private int indexToSquare(int index)
        {
            throw new NotImplementedException();
        }
        System.Drawing.Point origin;
       
       
     
        public Field(int numberOfDivisions=1024, int fieldLength = 1050, int fieldWidth = 680)
        {
          
            this.fieldLength = fieldLength;
            this.fieldWidth = fieldWidth;
            double pitchRatio = (double)fieldLength / (double)fieldWidth;
            double queryPointArea = (double)(fieldWidth * fieldLength) / numberOfDivisions;
            double queryPointSide = Math.Sqrt(queryPointArea);
            var squareCenter = new System.Drawing.PointF((float)queryPointSide / 2, (float)queryPointSide / 2);
            this.origin = new System.Drawing.Point(fieldLength / 2, fieldWidth / 2);
            this.QueryPointGrid = new List<System.Drawing.PointF>();
            int row = 0;
            int col = 0;
            while (squareCenter.X + (float)queryPointSide * row < fieldWidth)
            {
                col = 0;
                
                while (squareCenter.Y + (float)queryPointSide * col < fieldLength)
                {
                    this.QueryPointGrid.Add(new System.Drawing.PointF(squareCenter.X + (float)queryPointSide * col,
                                                     squareCenter.Y + (float)queryPointSide * row));
                    col++;
                }
                row++;
            }

        }
        
        private int absolutePosX(double xCoord)
        {
            if (xCoord < -1)
                xCoord = -1;
            if (xCoord > 1)
                xCoord = 1;
            int newCoord = (int)(this.origin.X * (1d + xCoord));
            return newCoord;

        }
        private int absolutePosY(double yCoord)
        {

            if (yCoord < -1)
                yCoord = -1;
            if (yCoord > 1)
                yCoord = 1;
            int newCoord = (int)(this.origin.Y * (1d + yCoord));
            return newCoord;
        }
        private ulong[] calculateDominance(List<Info> playerPositions, Teams team)
        {
            ulong[] result = new ulong[this.QueryPointGrid.Count / 64 + 1];
            int resultIndex = 0;
            for (int index = 0; index < this.QueryPointGrid.Count; index++)
            {
                resultIndex = index / 64;
                Teams closestTeam = this.closestTeam(this.QueryPointGrid[index], playerPositions);
                if (closestTeam == team)
                {
                    
                    result[resultIndex] += 1;
                    if(index%64 != 63)
                        result[resultIndex] <<= 1;
                }
                else
                {
                    if(index%64 != 63)
                        result[resultIndex] <<= 1;
                }
            
            }
            int shift = (63 - queryPointGrid.Count % 64);
            result[result.Length - 1] <<= shift;
            return result;
        }

        private Teams closestTeam(System.Drawing.PointF pointF, List<Info> playerPositions)
        {
            double minDistance = double.MaxValue;
            var result = Teams.None;
            foreach (var item in playerPositions)
            {
                if (item is Player)
                {
                    var currentPlayer = item as Player;
                    double distance = this.distance(this.absolutePosX(currentPlayer.XCoord), this.absolutePosY(currentPlayer.YCoord),
                                            pointF.X, pointF.Y);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        result = currentPlayer.Team;
                    }
                }
            }
            return result;
        }


        public SortedDictionary<int, ulong[]> CalculateDominance(Dictionary<int, Frame> dataDict, Teams team, bool paralel = true)
        {
            var result = new SortedDictionary<int, ulong[]>();
            if (this.queryPointGrid == null || this.queryPointGrid.Count == 0)
                throw new ArgumentException("The query point grid was not initialized");
            if (paralel)
                Parallel.ForEach(dataDict, data =>
                    {
                        try
                        {
                            result.Add(data.Key, this.calculateDominance(data.Value.Objects, team));
                        }
                        catch
                        {
                            Console.WriteLine("A problem has occured with item {0}", data.Key);
                        }

                    });
            else
            {
                foreach (var data in dataDict)
                {
                    try
                    {
                        result.Add(data.Key, this.calculateDominance(data.Value.Objects, team));
                    }
                    catch
                    {
                        Console.WriteLine("A problem has occured with item {0}", data.Key);
                    }

                }
            }
            return result;
        }
        public double[] ConvertToAbsoluteCoordinates(Info item)
        {
            var result = new double[2];
            result[0] = this.absolutePosX(item.XCoord);
            result[1] = this.absolutePosY(item.YCoord);
            return result;
        }
    
    }
}
