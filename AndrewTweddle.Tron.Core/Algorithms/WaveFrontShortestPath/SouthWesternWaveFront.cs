using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.Algorithms.WaveFrontShortestPath
{
    public class SouthWesternWaveFront: WaveFront
    {
        public override WaveDirection Direction
        {
            get { return WaveDirection.SW; }
        }

        protected override WaveDirection AdjacentDirectionOnEasternEdge
        {
            get { return WaveDirection.SE; }
        }

        protected override WaveDirection AdjacentDirectionOnWesternEdge
        {
            get { return WaveDirection.NW; }
        }

        protected override WaveDirection DirectionOfReflectedPolarWaveFront
        {
            get { return WaveDirection.N; }
        }

        protected override int ChangeInYAsXIncreases
        {
            get
            {
                return 1;
            }
        }

        protected override int XWestAdjustment
        {
            get
            {
                return -1;
            }
        }

        protected override int YWestAdjustment
        {
            get
            {
                return 0;
            }
        }

        protected override int XEastAdjustment
        {
            get
            {
                return 0;
            }
        }

        protected override int YEastAdjustment
        {
            get
            {
                return 1;
            }
        }

        public override IEnumerable<Position> GetPointsFromWestToEast()
        {
            if (EasternPoint == WesternPoint)
            {
                yield return WesternPoint;
            }
            else
            {
                Position nextPos = null;

                while (nextPos != EasternPoint)
                {
                    if (nextPos == null)
                    {
                        nextPos = WesternPoint;
                    }
                    else
                    {
                        int nextX = nextPos.X + 1;
                        if (nextX == Constants.Columns)
                        {
                            nextX = 0;
                        }
                        int nextY = nextPos.Y + ChangeInYAsXIncreases;
                        nextPos = new Position(nextX, nextY);
                    }
                    yield return nextPos;
                }
            }
        }

        protected override WaveFront CreateAPointWaveOnTheWesternEdge(Position position)
        {
            WaveFront adjacentFrontOnWesternSide = WaveFrontFactory.CreateWaveFront(AdjacentDirectionOnWesternEdge);
            adjacentFrontOnWesternSide.WesternPoint = position;
            adjacentFrontOnWesternSide.EasternPoint = position;

            // TODO: *** Following depends on the direction of the adjacent edge
            adjacentFrontOnWesternSide.IsWesternPointShared = false;
            adjacentFrontOnWesternSide.IsEasternPointShared = true;
            return adjacentFrontOnWesternSide;
        }

        protected override WaveFront CreateAPointWaveOnTheEasternEdge(Position position)
        {
            WaveFront newFront = WaveFrontFactory.CreateWaveFront(AdjacentDirectionOnEasternEdge);
            newFront.WesternPoint = position;
            newFront.EasternPoint = position;

            // TODO: *** Following depends on the direction of the adjacent edge
            newFront.IsWesternPointShared = true;
            newFront.IsEasternPointShared = false;
            return newFront;
        }

        protected override WaveFront CreateAReflectedPolarWaveFront(Position position)
        {
            WaveFront polarWaveFront = WaveFrontFactory.CreateWaveFront(DirectionOfReflectedPolarWaveFront);
            polarWaveFront.WesternPoint = position;
            polarWaveFront.EasternPoint = position;
            return polarWaveFront;
        }
    }
}
