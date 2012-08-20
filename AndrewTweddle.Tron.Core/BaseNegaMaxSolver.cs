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

        public int MaxDepth { get; private set; }
        public SearchNode RootNode { get; private set; }

        protected BaseNegaMaxSolver(): base()
        {
            MaxDepth = 6;
        }

        public BaseNegaMaxSolver(int depth): base()
        {
            MaxDepth = depth;
        }

        protected override void DoSolve()
        {
            RootNode = new SearchNode(Coordinator.CurrentGameState);
            double evaluation = Negamax(RootNode);
            RootNode.Evaluation = evaluation;

            List<SearchNode> bestChildNodes = RootNode.ChildNodes.Where(
                childNode => childNode.EvaluationStatus == EvaluationStatus.Evaluated && childNode.Evaluation == evaluation
            ).ToList();

            SearchNode chosenChildNode = null;
            if (bestChildNodes.Count > 1)
            {
                Random rnd = new Random();
                int randomMoveIndex = rnd.Next(bestChildNodes.Count);
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
                Debug.WriteLine("NegaMax found no best child");
            }
        }

        private double Negamax(SearchNode searchNode, int depth = 0, double alpha = double.NegativeInfinity, 
            double beta = double.PositiveInfinity)
        {
            int multiplier = searchNode.GameState.PlayerToMoveNext == PlayerType.You ? 1 : -1;

            if (depth >= MaxDepth)
            {
                Evaluate(searchNode);
                return multiplier * searchNode.Evaluation;
            }
            else
            {
                searchNode.Expand();

                if (!searchNode.ChildNodes.Any())
                {
                    /* Game is terminal: */
                    Evaluate(searchNode);  // TODO: Why not just return PositiveInfinity?
                    return multiplier * searchNode.Evaluation;
                }
                else
                {
                    double max = double.NegativeInfinity;
                    bool pruning = false;

                    foreach (SearchNode childNode in searchNode.ChildNodes)
                    {
                        if (pruning)
                        {
                            childNode.EvaluationStatus = EvaluationStatus.Pruned;
                        }
                        else
                        {
                            double evaluation = -Negamax(childNode, depth + 1, -beta, -alpha);
                            childNode.Evaluation = evaluation;

                            // Alpha-beta pruning:
                            if (evaluation > max)
                            {
                                max = evaluation;
                            }
                            if (evaluation > alpha)
                            {
                                alpha = evaluation;
                            }
                            if (alpha >= beta)
                            {
                                pruning = true;
                            }
                        }
                    }
                    if (pruning)
                    {
                        return alpha;
                    }
                    return max;
                }
            }
        }
    }
}
