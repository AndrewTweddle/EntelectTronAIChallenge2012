using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;

namespace AndrewTweddle.Tron.Bots
{
    public class NegaMaxSolver: BaseNegaMaxSolver
    {
        protected override void Evaluate(SearchNode searchNode)
        {
            if (!searchNode.GameState.GetPossibleNextStates().Any())
            {
                if (searchNode.GameState.PlayerToMoveNext == PlayerType.You)
                {
                    searchNode.Evaluation = double.NegativeInfinity;
                }
                else
                {
                    searchNode.Evaluation = double.PositiveInfinity;
                }
            }
            else
            {
                searchNode.Evaluation = searchNode.GameState.TotalDegreesOfCellsClosestToYou + searchNode.GameState.NumberOfCellsClosestToYou
                    - searchNode.GameState.TotalDegreesOfCellsClosestToOpponent - searchNode.GameState.NumberOfCellsClosestToOpponent;
            }
        }
    }
}
