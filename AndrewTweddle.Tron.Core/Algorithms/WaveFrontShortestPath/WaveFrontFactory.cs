using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.Algorithms.WaveFrontShortestPath
{
    public static class WaveFrontFactory
    {
        public static WaveFront CreateWaveFront(WaveDirection direction)
        {
            switch (direction)
            {
                case WaveDirection.NE:
                    return new NorthEasternWaveFront();
                case WaveDirection.SE:
                    return new SouthEasternWaveFront();
                case WaveDirection.SW:
                    return new SouthWesternWaveFront();
                case WaveDirection.NW:
                    return new NorthWesternWaveFront();
                case WaveDirection.N:
                    return new NorthTravellingPolarWaveFront();
                case WaveDirection.S:
                    return new SouthTravellingPolarWaveFront();
            }
            return null;  // To prevent compiler warning
        }
    }
}
