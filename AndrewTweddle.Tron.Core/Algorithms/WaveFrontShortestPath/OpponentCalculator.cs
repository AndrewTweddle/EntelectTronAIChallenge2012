using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.Algorithms.WaveFrontShortestPath
{
    public class OpponentCalculator: PlayerCalculator
    {
        public override bool IsCellOccupied(CellState cellState)
        {
            switch (cellState.OccupationStatus)
            {
                case OccupationStatus.Clear:
                case OccupationStatus.Opponent:
                    return false;
                default:
                    return true;
            }
        }

        public override int GetExistingDistance(CellState cellState)
        {
            return cellState.DistanceFromOpponent;
        }

        public override void SetDistance(CellState cellState, int distance, HashSet<CellState> reachableCells)
        {
            cellState.DistanceFromOpponent = distance;
            cellState.CompartmentStatus |= CompartmentStatus.InOpponentsCompartment;
            base.SetDistance(cellState, distance, reachableCells);
        }
    }
}
