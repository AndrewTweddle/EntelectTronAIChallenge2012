﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using AndrewTweddle.Tron.Core.Algorithms.WaveFrontShortestPath;

namespace AndrewTweddle.Tron.Core.Algorithms
{
    public static class Dijkstra
    {
        public static void Perform(GameState gameState, bool calculateDistancesFromOpponent = true)
        {
            BaseVoronoiAlgorithm algorithm = new WaveFrontAlgorithm();
            algorithm.Perform(gameState, calculateDistancesFromOpponent);
        }
    }
}
