using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.Algorithms.WaveFrontShortestPath
{
    public abstract class PlayerCalculator
    {
        public int NumberOfCellsReachable { get; set; }
        public int TotalDegreesOfCellsReachable { get; set; }
        
        public abstract bool IsCellOccupied(CellState cellState);
        public abstract int GetExistingDistance(CellState cellState);

        public virtual void SetDistance(CellState cellState, int distance, HashSet<CellState> reachableCells)
        {
            if (reachableCells != null)
            {
                reachableCells.Add(cellState);
            }
            NumberOfCellsReachable = NumberOfCellsReachable + 1;
            TotalDegreesOfCellsReachable = TotalDegreesOfCellsReachable + cellState.DegreeOfVertex;
        }
    }
}
