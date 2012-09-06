using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.Algorithms.WaveFrontShortestPath
{
    public class NorthTravellingPolarWaveFront: PolarWaveFront
    {
        public override WaveDirection Direction
        {
            get { return WaveDirection.N; }
        }

        protected override WaveDirection AdjacentDirectionOnEasternEdge
        {
            get { return WaveDirection.NE; }
        }

        protected override WaveDirection AdjacentDirectionOnWesternEdge
        {
            get { return WaveDirection.NW; }
        }

        protected override int YEastAdjustment
        {
            get { return -1; }
        }

        protected override int YWestAdjustment
        {
            get { return -1; }
        }

        protected override WaveDirection DirectionOfReflectedPolarWaveFront
        {
            get { return WaveDirection.S; }
        }
    }
}
