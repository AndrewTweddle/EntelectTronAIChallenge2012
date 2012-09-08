using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.Algorithms.WaveFrontShortestPath
{
    public class NorthTravellingPolarWaveFront: WaveFront
    {
        public override WaveDirection Direction
        {
            get { return WaveDirection.N; }
        }

        public override WaveFront Expand()
        {
            if (WesternPoint.IsPole)
            {
                int newWestX = NormalizedX(EasternPoint.X + 1);
                int newY = WesternPoint.Y - 1;
                Position newWesternPoint = new Position(newWestX, newY);

                int newEastX = NormalizedX(WesternPoint.X - 1);
                Position newEasternPoint = new Position(newEastX, newY);

                // Create a new wave front with the same direction:
                WaveFront waveFront = CreateWaveFrontWithSameDirection();
                waveFront.WesternPoint = newWesternPoint;
                waveFront.EasternPoint = newEasternPoint;
                waveFront.IsWesternPointShared = false;
                waveFront.IsEasternPointShared = false;

                return waveFront;
            }
            return base.Expand();
        }

        protected override Position ExpandWesternPoint()
        {
            int newWestY = WesternPoint.Y - 1;
            Position newWesternPoint = new Position(WesternPoint.X, newWestY);
            return newWesternPoint;
        }

        protected override Position ExpandEasternPoint()
        {
            int newEastY = EasternPoint.Y - 1;
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
                        int nextY = nextPos.Y;
                        nextPos = new Position(nextX, nextY);
                    }
                    yield return nextPos;
                }
            }
        }

        protected override WaveFront CreateWaveFrontWithSameDirection()
        {
            WaveFront waveFront = new NorthTravellingPolarWaveFront();
            return waveFront;
        }

        protected override WaveFront CreateAPointWaveOnTheWesternEdge(Position position)
        {
            WaveFront adjacentFrontOnWesternSide = new NorthWesternWaveFront();
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

            newFront.IsWesternPointShared = true;
            newFront.IsEasternPointShared = false;
            return newFront;
        }

        protected override WaveFront CreateAReflectedPolarWaveFront(Position position)
        {
            WaveFront polarWaveFront = new SouthTravellingPolarWaveFront();
            polarWaveFront.WesternPoint = position;
            polarWaveFront.EasternPoint = position;
            return polarWaveFront;
        }
    }
}
