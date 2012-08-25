using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;
using System.Threading;

namespace AndrewTweddle.Tron.Bots
{
    public class HumanSolver: BaseSolver
    {
        private Position positionMovedTo;

        private ManualResetEvent moveMadeEvent = new ManualResetEvent(false);

        public Position PositionMovedTo
        {
            get
            {
                return positionMovedTo;
            }
            set
            {
                positionMovedTo = value;
                if (positionMovedTo != null)
                {
                    moveMadeEvent.Set();
                }
            }
        }

        protected override void DoSolve()
        {
            if (!Coordinator.CurrentGameState.IsGameOver)
            {
                GameState newGameState = Coordinator.CurrentGameState.Clone();
                PositionMovedTo = null;
                while (PositionMovedTo == null)
                {
                    moveMadeEvent.Reset();

                    // Wait for UI to signal that a move has been chosen:
                    moveMadeEvent.WaitOne();
                    if (PositionMovedTo != null && newGameState.IsValidPositionToMoveTo(PositionMovedTo))
                    {
                        newGameState.MoveToPosition(PositionMovedTo);
                        Coordinator.SetBestMoveSoFar(newGameState);
                    }
                    else
                    {
                        PositionMovedTo = null;
                    }
                }
            }
        }
    }
}
