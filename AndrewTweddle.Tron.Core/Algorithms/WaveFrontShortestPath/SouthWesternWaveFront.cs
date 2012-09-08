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

        protected override Position ExpandWesternPoint()
        {
            int newWestX = NormalizedX(WesternPoint.X - 1);
            Position newWesternPoint = new Position(newWestX, WesternPoint.Y);
            return newWesternPoint;
        }

        protected override Position ExpandEasternPoint()
        {
            int newEastY = EasternPoint.Y + 1;
            Position newEasternPoint = new Position(EasternPoint.X, newEastY);
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
                        int nextY = nextPos.Y + 1;
                        nextPos = new Position(nextX, nextY);
                    }
                    yield return nextPos;
                }
            }
        }

        protected override WaveFront CreateWaveFrontWithSameDirection()
        {
            WaveFront waveFront = new SouthWesternWaveFront();
            return waveFront;
        }

        protected override WaveFront CreateAPointWaveOnTheWesternEdge(Position position)
        {
            WaveFront adjacentFrontOnWesternSide = new NorthWesternWaveFront();
            adjacentFrontOnWesternSide.WesternPoint = position;
            adjacentFrontOnWesternSide.EasternPoint = position;
            adjacentFrontOnWesternSide.IsWesternPointShared = true;
            adjacentFrontOnWesternSide.IsEasternPointShared = false;
            return adjacentFrontOnWesternSide;
        }

        protected override WaveFront CreateAPointWaveOnTheEasternEdge(Position position)
        {
            WaveFront newFront = new SouthEasternWaveFront();
            newFront.WesternPoint = position;
            newFront.EasternPoint = position;
            newFront.IsWesternPointShared = true;
            newFront.IsEasternPointShared = false;
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
