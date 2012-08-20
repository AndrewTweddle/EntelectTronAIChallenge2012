using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;

namespace AndrewTweddle.Tron.Bots
{
    public class RandomSolver: BaseSolver
    {
        protected override void DoSolve()
        {
            GameState newGameState = Coordinator.CurrentGameState.Clone();
            Move[] possibleMoves = newGameState.GetPossibleNextMoves().ToArray();
            if (possibleMoves.Length == 0)
            {
                throw new ApplicationException("RandomSolver has no moves left - the game has been lost");
            }
            Move chosenMove;
            if (possibleMoves.Length == 1)
            {
                chosenMove = possibleMoves[0];
            }
            else
            {
                Random rnd = new Random();
                int chosenMoveIndex = rnd.Next(possibleMoves.Length);
                chosenMove = possibleMoves[chosenMoveIndex];
            }
            newGameState.MakeMove(chosenMove);
            Coordinator.SetBestMoveSoFar(newGameState);
        }
    }
}
