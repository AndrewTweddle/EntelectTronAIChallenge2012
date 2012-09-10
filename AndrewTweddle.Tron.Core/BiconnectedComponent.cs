using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    public class BiconnectedComponent
    {
        HashSet<CellState> includedCells = new HashSet<CellState>();

        // TODO: List<BiconnectedComponent> adjacentComponents = new List<BiconnectedComponent>();

        public void AddCell(CellState cellState)
        {
            includedCells.Add(cellState);
        }
    }
}
