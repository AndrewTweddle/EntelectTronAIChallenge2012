using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace AndrewTweddle.Tron.Core
{
    public class Chamber
    {
        private HashSet<CellState> cells = new HashSet<CellState>();
        private HashSet<CellState> cutVertices = new HashSet<CellState>();
        private HashSet<Chamber> enemyChambers = new HashSet<Chamber>();

        public int ChamberNumber
        {
            get;
            private set;
        }

        #region Constructors

        protected Chamber()
        {
        }

        public Chamber(int chamberNumber)
        {
            ChamberNumber = chamberNumber;
        }

        #endregion

        public void AddCell(CellState cellState)
        {
            cells.Add(cellState);
        }

        public IEnumerable<CellState> GetCells()
        {
            return cells;
        }

        public IEnumerable<CellState> GetCutVertices()
        {
            return cutVertices;
        }

        public void AddCutVertex(CellState cellState)
        {
            cutVertices.Add(cellState);
        }

        public void AddAdjacentEnemyChamber(Chamber enemyChamber)
        {
            enemyChambers.Add(enemyChamber);
        }

        public IEnumerable<Chamber> GetAdjacentEnemyChambers()
        {
            return enemyChambers;
        }

        public int GetNumberOfCellsExcludingCutVertices()
        {
            return cells.Count - cutVertices.Count;
        }
    }
}
