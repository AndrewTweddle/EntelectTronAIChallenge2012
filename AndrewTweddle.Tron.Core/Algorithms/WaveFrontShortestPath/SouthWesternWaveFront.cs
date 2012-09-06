using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.Algorithms.WaveFrontShortestPath
{
    public class SouthWesternWaveFront: WaveFront
    {
        public override WaveDirection Direction
        {
            get { return WaveDirection.SW; }
        }

        protected override WaveDirection AdjacentDirectionOnEasternEdge
        {
            get { return WaveDirection.SE; }
        }

        protected override WaveDirection AdjacentDirectionOnWesternEdge
        {
            get { return WaveDirection.NW; }
        }

        protected override WaveDirection DirectionOfReflectedPolarWaveFront
        {
            get { return WaveDirection.N; }
        }

        protected override int ChangeInYAsXIncreases
        {
            get
            {
                return 1;
            }
        }

        protected override int XWestAdjustment
        {
            get
            {
                return -1;
            }
        }

        protected override int YWestAdjustment
        {
            get
            {
                return 0;
            }
        }

        protected override int XEastAdjustment
        {
            get
            {
                return 0;
            }
        }

        protected override int YEastAdjustment
        {
            get
            {
                return 1;
            }
        }
    }
}
