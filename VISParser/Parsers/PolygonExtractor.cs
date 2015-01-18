using System;
using System.Collections.Generic;
using BenTools.Mathematics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Globalization;
using VISParser.FieldObjects;
namespace VISParser.Parsers
{
    public sealed class PolygonExtractor
    {
        //This class is actually a copy/paste of a part of the ImageBuilder class. 
        //I will need to redo the procedure calls there, when I have time, in order to use this code and not have doubled code.
        const int xLength = 1050, yLength = 680;
        private Dictionary<int, List<PointF>> convertToPolygons(VoronoiGraph vorGraph)
        {
            var polygons = new Dictionary<int, List<PointF>>();
            Dictionary<int, List<VoronoiEdge>> playerEdges = this.extractVoronoiEdges(vorGraph);
            for (int pId = 1; pId <= VISAPI.PlayerCount; pId++)
            {
                polygons.Add(pId, new List<PointF>());
                var flags = new bool[playerEdges[pId].Count];
                int iterationCount = 0;
                do
                {
                    for (int index = 0; index < playerEdges[pId].Count; index++)
                    {

                        var edge = playerEdges[pId][index];
                        List<PointF> newLine = this.convertToLine(edge);
                        if (flags[index])
                            continue;
                        if (polygons[pId].Contains(newLine[0])
                            && polygons[pId].Contains(newLine[1]))
                        {
                            flags[index] = true;
                            break;
                        }
                        if (polygons[pId].Contains(newLine[0]))
                        {
                            int insertIndex = polygons[pId].IndexOf(newLine[0]);
                            if (insertIndex == 0)
                            {
                                polygons[pId].Insert(insertIndex, newLine[1]);


                            }
                            else if (insertIndex == polygons[pId].Count - 1)
                            {
                                polygons[pId].Add(newLine[1]);
                            }
                            else
                                throw new Exception("This else should not be reached, ever!!!");
                            flags[index] = true;
                        }
                        else if (polygons[pId].Contains(newLine[1]))
                        {
                            int insertIndex = polygons[pId].IndexOf(newLine[1]);
                            if (insertIndex == 0)
                            {
                                polygons[pId].Insert(insertIndex, newLine[0]);

                            }
                            else if (insertIndex == polygons[pId].Count - 1)
                            {
                                polygons[pId].Add(newLine[0]);
                            }
                            else
                                throw new Exception("This else should not be reached, ever!!!");
                            flags[index] = true;
                        }
                        else
                        {
                            if (polygons[pId].Count == 0)
                            {
                                polygons[pId].Add(newLine[0]);
                                polygons[pId].Add(newLine[1]);
                                flags[index] = true;
                            }
                        }
                    }
                    iterationCount++;
                    if (iterationCount > 10)
                        throw new Exception(string.Format("Image {0} got stuck in an infinite loop", iterationCount));

                }
                while (flags.Count(item => item == false) > 0);
            }
            return polygons;
        }
        private Dictionary<int, List<VoronoiEdge>> extractVoronoiEdges(VoronoiGraph vorGraph)
        {
            var result = new Dictionary<int, List<VoronoiEdge>>();
            int upperLeftId = -1;
            int lowerLeftId = -1;
            int upperRightId = -1;
            int lowerRightId = -1;
            var upperLeft = new VoronoiEdge { VVertexA = new Vector(2) };
            upperLeft.VVertexA[0] = xLength;
            upperLeft.VVertexA[1] = 0;
            upperLeft.VVertexB = new Vector("(0;0)");
            var lowerLeft = new VoronoiEdge { VVertexA = new Vector(2) };
            lowerLeft.VVertexA[0] = xLength;
            lowerLeft.VVertexA[1] = yLength;
            lowerLeft.VVertexB = new Vector("(0;" + yLength.ToString(CultureInfo.InvariantCulture) + ")");
            var upperRight = new VoronoiEdge { VVertexA = new Vector(2) };
            upperRight.VVertexA[0] = 0;
            upperRight.VVertexB[1] = 0;
            upperRight.VVertexB = new Vector("(" + xLength.ToString(CultureInfo.InvariantCulture) + ";0)");
            var lowerRight = new VoronoiEdge { VVertexA = new Vector(2) };
            lowerRight.VVertexA[0] = 0;
            lowerRight.VVertexA[1] = yLength;
            lowerRight.VVertexB = new Vector("(" + xLength.ToString(CultureInfo.InvariantCulture) + ";" + yLength.ToString(CultureInfo.InvariantCulture) + ")");
            foreach (var edge in vorGraph.Edges)
            {
                if (result.ContainsKey(edge.RightDataId))
                {
                    result[edge.RightDataId].Add(edge);
                }
                else
                {
                    result.Add(edge.RightDataId, new List<VoronoiEdge>());
                    result[edge.RightDataId].Add(edge);
                }
                if (result.ContainsKey(edge.LeftDataId))
                {
                    result[edge.LeftDataId].Add(edge);
                }
                else
                {
                    result.Add(edge.LeftDataId, new List<VoronoiEdge>());
                    result[edge.LeftDataId].Add(edge);
                }
                //UpperLeft and UpperRight
                if (edge.VVertexA[1] == 0)
                {
                    if (edge.VVertexA[0] <= upperLeft.VVertexA[0])
                    {
                        upperLeft.VVertexA[0] = edge.VVertexA[0];
                        upperLeftId = edge.LeftData[0] < edge.RightData[0] ? edge.LeftDataId : edge.RightDataId;
                    }
                    if (edge.VVertexA[0] >= upperRight.VVertexA[0])
                    {
                        upperRight.VVertexA[0] = edge.VVertexA[0];
                        upperRightId = edge.LeftData[0] > edge.RightData[0] ? edge.LeftDataId : edge.RightDataId;
                    }
                }
                if (edge.VVertexB[1] == 0)
                {
                    if (edge.VVertexB[0] <= upperLeft.VVertexA[0])
                    {
                        upperLeft.VVertexA[0] = edge.VVertexB[0];
                        upperLeftId = edge.LeftData[0] < edge.RightData[0] ? edge.LeftDataId : edge.RightDataId;
                    }
                    if (edge.VVertexB[0] >= upperRight.VVertexA[0])
                    {
                        upperRight.VVertexA[0] = edge.VVertexB[0];
                        upperRightId = edge.LeftData[0] > edge.RightData[0] ? edge.LeftDataId : edge.RightDataId;
                    }
                }
                //LowerLeft and UpperLEft
                if (edge.VVertexA[1] == yLength)
                {
                    if (edge.VVertexA[0] <= lowerLeft.VVertexA[0])
                    {
                        lowerLeft.VVertexA[0] = edge.VVertexA[0];
                        lowerLeftId = edge.LeftData[0] < edge.RightData[0] ? edge.LeftDataId : edge.RightDataId;
                    }
                    if (edge.VVertexA[0] >= lowerRight.VVertexA[0])
                    {
                        lowerRight.VVertexA[0] = edge.VVertexA[0];
                        lowerRightId = edge.LeftData[0] > edge.RightData[0] ? edge.LeftDataId : edge.RightDataId;
                    }
                }
                if (edge.VVertexB[1] == yLength)
                {
                    if (edge.VVertexB[0] <= lowerLeft.VVertexA[0])
                    {
                        lowerLeft.VVertexA[0] = edge.VVertexB[0];
                        lowerLeftId = edge.LeftData[0] < edge.RightData[0] ? edge.LeftDataId : edge.RightDataId;
                    }
                    if (edge.VVertexB[0] >= lowerRight.VVertexA[0])
                    {
                        lowerRight.VVertexA[0] = edge.VVertexB[0];
                        lowerRightId = edge.LeftData[0] > edge.RightData[0] ? edge.LeftDataId : edge.RightDataId;
                    }
                }
            }
            result[lowerRightId].Add(lowerRight);
            result[upperRightId].Add(upperRight);
            result[lowerLeftId].Add(lowerLeft);
            result[upperLeftId].Add(upperLeft);
            return result;
        }
        private List<PointF> convertToLine(BenTools.Mathematics.VoronoiEdge edge)
        {
            var result = new List<PointF>();
            Point startPoint;
            Point endPoint;
            if (edge.IsPartlyInfinite)
            {
                //The first big number that came to mind :))
                double randNumber = 5000;
                Vector start = double.IsInfinity(edge.VVertexB.ElementSum) ? edge.VVertexA : edge.VVertexB;
                startPoint = new Point((int)start[0], (int)start[1]);
                endPoint = new Point((int)start[0] + (int)(randNumber * edge.DirectionVector[0]),
                                        (int)start[1] + (int)(randNumber * edge.DirectionVector[1]));
                result.Add(startPoint);
                result.Add(endPoint);
            }
            else
            {

                startPoint = new Point((int)edge.VVertexA[0], (int)edge.VVertexA[1]);
                endPoint = new Point((int)edge.VVertexB[0],
                                       (int)edge.VVertexB[1]);
                result.Add(startPoint);
                result.Add(endPoint);
            }
            return result;
        }
        public Dictionary<int, List<PointF>> ExtractPolygon(BenTools.Mathematics.VoronoiGraph vorGraph)
        {
            return 
                this.convertToPolygons(vorGraph);
        }
    }
}
