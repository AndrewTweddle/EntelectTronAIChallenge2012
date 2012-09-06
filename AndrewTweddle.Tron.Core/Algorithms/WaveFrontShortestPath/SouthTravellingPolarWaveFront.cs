using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.Algorithms.WaveFrontShortestPath
{
    public class SouthTravellingPolarWaveFront: PolarWaveFront
    {
        public override WaveDirection Direction
        {
            get { return WaveDirection.S; }
        }

        protected override WaveDirection AdjacentDirectionOnEasternEdge
        {
            get { return WaveDirection.SE; }
        }

        protected override WaveDirection AdjacentDirectionOnWesternEdge
        {
            get { return WaveDirection.SW; }
        }

        protected override int YEastAdjustment
        {
            get { return 1; }
        }

        protected override int YWestAdjustment
        {
            get { return 1; }
        }

        protected override WaveDirection DirectionOfReflectedPolarWaveFront
        {
            get { return WaveDirection.N; }
        }
    }
}
