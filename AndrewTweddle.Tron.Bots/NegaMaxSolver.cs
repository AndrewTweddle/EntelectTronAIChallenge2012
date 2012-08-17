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
                    searchNode.Value = double.NegativeInfinity;
                }
                else
                {
                    searchNode.Value = double.PositiveInfinity;
                }
            }
            else
            {
                searchNode.Value = searchNode.GameState.TotalDegreesOfCellsClosestToYou + searchNode.GameState.NumberOfCellsClosestToYou
                    - searchNode.GameState.TotalDegreesOfCellsClosestToOpponent - searchNode.GameState.NumberOfCellsClosestToOpponent;
            }
        }
    }
}
