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

        public virtual void SetDistance(CellState cellState, int distance, LinkedList<CellState> reachableCells)
        {
            reachableCells.AddLast(cellState);
            NumberOfCellsReachable = NumberOfCellsReachable + 1;
            TotalDegreesOfCellsReachable = TotalDegreesOfCellsReachable + cellState.DegreeOfVertex;
        }
    }
}
