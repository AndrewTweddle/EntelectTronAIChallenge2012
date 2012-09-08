using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.Algorithms.WaveFrontShortestPath
{
    public abstract class PolarWaveFront: WaveFront
    {
        protected override int ChangeInYAsXIncreases
        {
            get { return 0; }
        }

        protected override int XEastAdjustment
        {
            get { return 0; }
        }

        protected override int XWestAdjustment
        {
            get { return 0; }
        }

        public override WaveFront Expand()
        {
            if (WesternPoint.IsPole)
            {
                int newWestX = NormalizedX(EasternPoint.X+1);
                int newWestY = WesternPoint.Y + YWestAdjustment;
                Position newWesternPoint = new Position(newWestX, newWestY);

                int newEastX = NormalizedX(WesternPoint.X-1);
                int newEastY = newWestY;
                Position newEasternPoint = new Position(newEastX, newEastY);

                // Create a new wave front with the same direction:
                WaveFront waveFront = WaveFrontFactory.CreateWaveFront(Direction);
                waveFront.WesternPoint = newWesternPoint;
                waveFront.EasternPoint = newEasternPoint;
                waveFront.IsWesternPointShared = false;
                waveFront.IsEasternPointShared = false;
                return waveFront;
            }
            return base.Expand();
        }
    }
}
