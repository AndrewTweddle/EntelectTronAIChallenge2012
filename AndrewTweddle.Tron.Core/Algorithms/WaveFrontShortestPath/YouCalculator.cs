using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.Algorithms.WaveFrontShortestPath
{
    public class YouCalculator: PlayerCalculator
    {
        public override bool IsCellOccupied(CellState cellState)
        {
            switch (cellState.OccupationStatus)
            {
                case OccupationStatus.Clear:
                case OccupationStatus.You:
                    return false;
                case OccupationStatus.Opponent:
                    // Mark that the opponent has been found and is reachable by You
                    cellState.GameState.OpponentIsInSameCompartment = true;
                    return true;
                default:
                    return true;
            }
        }

        public override int GetExistingDistance(CellState cellState)
        {
            return cellState.DistanceFromYou;
        }

        public override void SetDistance(CellState cellState, int distance, LinkedList<CellState> reachableCells)
        {
            cellState.DistanceFromYou = distance;
            cellState.CompartmentStatus |= CompartmentStatus.InYourCompartment;
            base.SetDistance(cellState, distance, reachableCells);
        }
    }
}
