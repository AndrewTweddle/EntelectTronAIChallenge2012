using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.Algorithms.WaveFrontShortestPath
{
    public class NorthEasternWaveFront: WaveFront
    {
        public override WaveDirection Direction
        {
            get { return WaveDirection.NE; }
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
            get { return WaveDirection.S; }
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
                return 0;
            }
        }

        protected override int YWestAdjustment
        {
            get
            {
                return -1;
            }
        }

        protected override int XEastAdjustment
        {
            get
            {
                return 1;
            }
        }

        protected override int YEastAdjustment
        {
            get
            {
                return 0;
            }
        }

    }
}
