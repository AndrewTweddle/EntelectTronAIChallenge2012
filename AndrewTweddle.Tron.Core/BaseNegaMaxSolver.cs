using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AndrewTweddle.Tron.Core
{
    public abstract class BaseNegaMaxSolver: BaseSolver
    {
        protected abstract void Evaluate(SearchNode searchNode);

        public int Depth { get; private set; }
        public SearchNode RootNode { get; private set; }

        protected BaseNegaMaxSolver(): base()
        {
            Depth = 6;
        }

        public BaseNegaMaxSolver(int depth): base()
        {
            Depth = depth;
        }

        protected override void DoSolve()
        {
            RootNode = new SearchNode(null /*parentNode*/, Coordinator.CurrentGameState);
            double value = Negamax(RootNode, Depth);
            RootNode.Value = value;

            List<SearchNode> bestChildNodes = RootNode.ChildNodes.Where(sn => sn.Value == value).ToList();
            SearchNode chosenChildNode = null;

            if (bestChildNodes.Count > 1)
            {
                Random rnd = new Random();
                int randomMoveIndex = rnd.Next(bestChildNodes.Count - 1);
                chosenChildNode = bestChildNodes[randomMoveIndex];
            }
            else
                if (bestChildNodes.Count == 1)
                {
                    chosenChildNode = bestChildNodes[0];
                }

            if (chosenChildNode != null)
            {
                Coordinator.SetBestMoveSoFar(chosenChildNode.GameState);
            }
            else
            {
                Debug.WriteLine("NegaMax found no child");
            }
        }

        private double Negamax(SearchNode searchNode, int depth, double alpha = double.NegativeInfinity, 
            double beta = double.PositiveInfinity, int color = 1)
        {
            if (depth == 0)
            {
                Evaluate(searchNode);
                return color * searchNode.Value;
            }
            else
            {
                searchNode.Expand();

                if (!searchNode.ChildNodes.Any())
                {
                    /* Game is terminal: */
                    Evaluate(searchNode);
                    return color * searchNode.Value;
                }
                else
                {
                    foreach (SearchNode childNode in searchNode.ChildNodes)
                    {
                        double value = -Negamax(childNode, depth - 1, -beta, -alpha, -color);
                        childNode.Value = value;

                        // Alpha-beta pruning:
                        if (value >= beta)
                        {
                            break;
                        }
                        if (value > alpha)
                        {
                            alpha = value;
                        }
                    }
                    return alpha;
                }
            }
        }
    }
}
