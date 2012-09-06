﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.Algorithms.WaveFrontShortestPath
{
    public abstract class WaveFront
    {
        public abstract WaveDirection Direction { get; }
        protected abstract WaveDirection AdjacentDirectionOnWesternEdge { get; }
        protected abstract WaveDirection AdjacentDirectionOnEasternEdge { get; }
        protected abstract WaveDirection DirectionOfReflectedPolarWaveFront { get; }

        /// <summary>
        /// This is used to determine the Y coordinate of the next point in the method GetPointsFromWestToEast()
        /// </summary>
        protected abstract int ChangeInYAsXIncreases { get; }

        protected abstract int XWestAdjustment { get; }
        protected abstract int YWestAdjustment { get; }
        protected abstract int XEastAdjustment { get; }
        protected abstract int YEastAdjustment { get; }

        public bool IsWesternPointShared { get; set; }
        public bool IsEasternPointShared { get; set; }
        public Position WesternPoint { get; set; }
        public Position EasternPoint { get; set; }

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
            int newWestX = NormalizedX(WesternPoint.X + XWestAdjustment);
            int newWestY = WesternPoint.Y + YWestAdjustment;
            Position newWesternPoint = new Position(newWestX, newWestY);

            int newEastX = NormalizedX(EasternPoint.X + XEastAdjustment);
            int newEastY = EasternPoint.Y + YEastAdjustment;
            Position newEasternPoint = new Position(newEastX, newEastY);

            // Create a new wave front with the same direction:
            WaveFront waveFront = WaveFrontFactory.CreateWaveFront(Direction);
            waveFront.WesternPoint = newWesternPoint;
            waveFront.EasternPoint = newEasternPoint;
            waveFront.IsWesternPointShared = IsWesternPointShared;
            waveFront.IsEasternPointShared = IsEasternPointShared;
            return waveFront;
        }

        public IEnumerable<Position> GetPointsFromWestToEast()
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
                        isPointOnNewFront = (isWesternMostPoint && IsWesternPointShared && (existingDistance == distance));
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
                            WaveFront polarWaveFront = WaveFrontFactory.CreateWaveFront(DirectionOfReflectedPolarWaveFront);
                            polarWaveFront.WesternPoint = position;
                            polarWaveFront.EasternPoint = position;
                            yield return polarWaveFront;
                            isPointOnNewFront = false;
                        }
                        else
                            if (!IsWesternPointShared)
                            {
                                // Create a new wave front to expand around any barriers:
                                WaveFront adjacentFrontOnWesternSide = WaveFrontFactory.CreateWaveFront(AdjacentDirectionOnWesternEdge);
                                adjacentFrontOnWesternSide.WesternPoint = position;
                                adjacentFrontOnWesternSide.EasternPoint = position;
                                adjacentFrontOnWesternSide.IsWesternPointShared = false;
                                adjacentFrontOnWesternSide.IsEasternPointShared = true;

                                isWesternPointShared = true;
                            }
                    }
                    isWesternMostPoint = false;
                }

                if (isPointOnNewFront)
                {
                    if (newFront == null)
                    {
                        /* Create a new front in the same direction for this next segment of the original wave front: */
                        newFront = WaveFrontFactory.CreateWaveFront(Direction);
                        newFront.WesternPoint = position;
                        newFront.IsWesternPointShared = isWesternPointShared;
                        isWesternPointShared = false;
                    }
                    else
                    {
                        easternMostPositionOfNewFront = position;
                    }
                }
                else
                {
                    if (newFront != null)
                    {
                        newFront.EasternPoint = easternMostPositionOfNewFront;
                        newFront.IsEasternPointShared = false;
                        yield return newFront;
                        newFront = null;
                    }
                }
            }

            if (newFront != null)
            {
                // End off the Eastern-most front:
                newFront.EasternPoint = EasternPoint;
                newFront.IsEasternPointShared = IsEasternPointShared;
                yield return newFront;

                if (!IsEasternPointShared)
                {
                    /* Create a new single-point wave front on the Eastern-most point of the last wave front on the east: */
                    newFront = WaveFrontFactory.CreateWaveFront(AdjacentDirectionOnEasternEdge);
                    newFront.WesternPoint = this.EasternPoint;
                    newFront.EasternPoint = this.EasternPoint;
                    newFront.IsWesternPointShared = false;
                    newFront.IsEasternPointShared = true;
                }
            }
        }
    }
}
