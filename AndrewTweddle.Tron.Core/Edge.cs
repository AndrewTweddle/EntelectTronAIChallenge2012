using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    public class Edge
    {
        public CellState StartVertex { get; set; }
        public CellState EndVertex { get; set; }

        public static bool operator ==(Edge edge1, Edge edge2)
        {
            if (edge1.StartVertex == edge2.StartVertex && edge1.EndVertex == edge2.EndVertex)
            {
                return true;
            }
            /* Incorrect - equality has an ordering as well:
            if (edge1.StartVertex == edge2.EndVertex && edge1.EndVertex == edge2.StartVertex)
            {
                return true;
            }
             */
            return false;
        }

        public static bool operator !=(Edge edge1, Edge edge2)
        {
            if (edge1.StartVertex == edge2.StartVertex && edge1.EndVertex == edge2.EndVertex)
            {
                return false;
            }
            /* Incorrect - equality has an ordering as well
            if (edge1.StartVertex == edge2.EndVertex && edge1.EndVertex == edge2.StartVertex)
            {
                return false;
            }
             */
            return true;
        }
    }
}
