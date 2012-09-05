using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AndrewTweddle.Tron.Core.Algorithms
{
    public static class Dijkstra
    {
        public static void Perform(GameState gameState, bool calculateDistancesFromOpponent = true)
        {
            ShortestPathAlgorithmUsingBFSQueue.Perform(gameState, calculateDistancesFromOpponent);
        }
    }
}
