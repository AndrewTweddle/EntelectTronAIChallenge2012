using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.Algorithms.WaveFrontShortestPath
{
    public class SouthEasternWaveFront: WaveFront
    {
        public override WaveDirection Direction
        {
            get { return WaveDirection.SE; }
        }

        protected override Position ExpandWesternPoint()
        {
            int newWestY = WesternPoint.Y + 1;
            Position newWesternPoint = new Position(WesternPoint.X, newWestY);
            return newWesternPoint;
        }

        protected override Position ExpandEasternPoint()
        {
            int newEastX = NormalizedX(EasternPoint.X + 1);
            Position newEasternPoint = new Position(newEastX, EasternPoint.Y);
            return newEasternPoint;
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
                        int nextY = nextPos.Y - 1;
                        nextPos = new Position(nextX, nextY);
                    }
                    yield return nextPos;
                }
            }
        }

        protected override WaveFront CreateWaveFrontWithSameDirection()
        {
            WaveFront waveFront = new SouthEasternWaveFront();
            return waveFront;
        }

        protected override WaveFront CreateAPointWaveOnTheWesternEdge(Position position)
        {
            WaveFront adjacentFrontOnWesternSide = new SouthWesternWaveFront();
            adjacentFrontOnWesternSide.WesternPoint = position;
            adjacentFrontOnWesternSide.EasternPoint = position;
            adjacentFrontOnWesternSide.IsWesternPointShared = false;
            adjacentFrontOnWesternSide.IsEasternPointShared = true;
            return adjacentFrontOnWesternSide;
        }

        protected override WaveFront CreateAPointWaveOnTheEasternEdge(Position position)
        {
            WaveFront newFront = new NorthEasternWaveFront();
            newFront.WesternPoint = position;
            newFront.EasternPoint = position;
            newFront.IsWesternPointShared = false;
            newFront.IsEasternPointShared = true;
            return newFront;
        }

        protected override WaveFront CreateAReflectedPolarWaveFront(Position position)
        {
            WaveFront polarWaveFront = new NorthTravellingPolarWaveFront();
            polarWaveFront.WesternPoint = position;
            polarWaveFront.EasternPoint = position;
            return polarWaveFront;
        }
    }
}
