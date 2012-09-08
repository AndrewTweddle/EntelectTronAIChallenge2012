using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.Algorithms.WaveFrontShortestPath
{
    public abstract class WaveFront
    {
        public abstract WaveDirection Direction { get; }

        public bool IsWesternPointShared { get; set; }
        public bool IsEasternPointShared { get; set; }
        public Position WesternPoint { get; set; }
        public Position EasternPoint { get; set; }

        public abstract IEnumerable<Position> GetPointsFromWestToEast();
        protected abstract WaveFront CreateWaveFrontWithSameDirection();
        protected abstract WaveFront CreateAPointWaveOnTheWesternEdge(Position position);
        protected abstract WaveFront CreateAPointWaveOnTheEasternEdge(Position position);
        protected abstract WaveFront CreateAReflectedPolarWaveFront(Position position);
        protected abstract Position ExpandWesternPoint();
        protected abstract Position ExpandEasternPoint();

        public override string ToString()
        {
            if (WesternPoint == EasternPoint)
            {
                return String.Format("{0} at {1}", Direction, WesternPoint);
            }
            return String.Format("{0} between {1} and {2}", Direction, WesternPoint, EasternPoint);
        }

        protected int NormalizedX(int x)
        {
            if (x == -1)
            {
                return Constants.Columns - 1;
            }
            if (x == Constants.Columns)
            {
                return 0;
            }
            return x;
        }

        // The expand method is virtual since a wave starting at a pole will need to expand differently:
        public virtual WaveFront Expand()
        {
            Position newWesternPoint = ExpandWesternPoint();
            Position newEasternPoint = ExpandEasternPoint();

            // Create a new wave front with the same direction:
            WaveFront waveFront = CreateWaveFrontWithSameDirection();
            waveFront.WesternPoint = newWesternPoint;
            waveFront.EasternPoint = newEasternPoint;
            waveFront.IsWesternPointShared = IsWesternPointShared;
            waveFront.IsEasternPointShared = IsEasternPointShared;
            return waveFront;
        }

        public IEnumerable<WaveFront> ContractAndCalculate(GameState gameState, int distance, PlayerCalculator calculator, HashSet<CellState> reachableCells)
        {
            bool isWesternMostPoint = true;
            WaveFront newFront = null;
            Position easternMostPositionOfNewFront = null;

            foreach (Position position in GetPointsFromWestToEast())
            {
                CellState cellState = gameState[position];
                bool isPointOnNewFront = true;  // A flag to indicate whether to keep this point on the new front
                if (calculator.IsCellOccupied(cellState))
                {
                    isPointOnNewFront = false;
                }
                else
                {
                    // See if the point has already been visited:
                    int existingDistance = calculator.GetExistingDistance(cellState);
                    if (existingDistance <= distance)
                    {
                        isPointOnNewFront = (existingDistance == distance) && !position.IsPole
                            && (   (isWesternMostPoint && IsWesternPointShared)
                                || (position == EasternPoint && IsEasternPointShared)
                               );
                        /* This will be false except when this is the shared point on the Western-most edge.
                         * If it's a shared point then the adjacent front may have already set the distance.
                         * So we assume that the distance has already been set by the adjacent front.
                         */
                    }
                    else
                    {
                        calculator.SetDistance(cellState, distance, reachableCells);
                    }
                }

                bool isWesternPointShared = false;

                if (isWesternMostPoint)
                {
                    isWesternPointShared = IsWesternPointShared;

                    if (isPointOnNewFront)
                    {
                        if (position.IsPole)
                        {
                            // Create a polar wave front going in the opposite direction:
                            WaveFront polarWaveFront = CreateAReflectedPolarWaveFront(position);
                            yield return polarWaveFront;
                            isPointOnNewFront = false;
                            isWesternPointShared = false;
                        }
                        else
                            if (!IsWesternPointShared)
                            {
                                // Create a new wave front to expand around any barriers:
                                WaveFront adjacentFrontOnWesternSide = CreateAPointWaveOnTheWesternEdge(position);
                                yield return adjacentFrontOnWesternSide;
                                isWesternPointShared = true;
                            }
                    }
                }

                if (isPointOnNewFront)
                {
                    if (position.IsPole)
                    {
                        /* Note: The following code assumes that this is a diagonal front.
                         * There is a very small chance that this is a polar front that has reached the opposite pole.
                         * Since this is a very rare event, we can afford the cost of testing every single point in the front.
                         * So don't worry about fixing this.
                         * /

                        /* End off current front (if there is one): */
                        if (newFront != null)
                        {
                            newFront.EasternPoint = easternMostPositionOfNewFront;
                            newFront.IsEasternPointShared = false;
                            yield return newFront;
                            newFront = null;
                        }

                        /* Create a polar wave front going in the opposite direction: */
                        WaveFront polarWaveFront = CreateAReflectedPolarWaveFront(position);
                        yield return polarWaveFront;
                        isPointOnNewFront = false;
                    }
                    else
                    {
                        if (newFront == null)
                        {
                            /* Create a new front in the same direction for this next segment of the original wave front: */
                            newFront = WaveFrontFactory.CreateWaveFront(Direction);
                            newFront.WesternPoint = position;
                            newFront.IsWesternPointShared = isWesternPointShared;
                            isWesternPointShared = false;
                        }
                        easternMostPositionOfNewFront = position;
                    }
                }
                else
                {
                    /* End the front currently being built: */
                    if (newFront != null)
                    {
                        newFront.EasternPoint = easternMostPositionOfNewFront;
                        newFront.IsEasternPointShared = false;
                        yield return newFront;
                        newFront = null;
                    }
                }

                // No longer the western-most point
                isWesternMostPoint = false;
            }

            if (newFront != null)
            {
                bool isEasternPointShared = IsEasternPointShared;

                // End off the Eastern-most front:
                newFront.EasternPoint = EasternPoint;
                newFront.IsEasternPointShared = true;
                yield return newFront;

                if (!IsEasternPointShared)
                {
                    // Create a new wave front to expand around any barriers:
                    newFront = CreateAPointWaveOnTheEasternEdge(EasternPoint);
                    yield return newFront;
                }
                newFront = null;
            }
        }
    }
}
