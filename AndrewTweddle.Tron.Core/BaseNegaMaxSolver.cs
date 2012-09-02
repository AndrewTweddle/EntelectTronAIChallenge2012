using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace AndrewTweddle.Tron.Core
{
    public abstract class BaseNegaMaxSolver: BaseSolver
    {
        #region Abstract methods

        protected abstract void Evaluate(SearchNode searchNode);

        #endregion


        #region Private member variables

        private bool isIterativeDeepeningEnabled = true;
        private int currentDepth;
        private int maxDepth;
        private SearchNode rootNode;

        #endregion


        #region Public properties

        public bool IsIterativeDeepeningEnabled
        {
            get
            {
                return isIterativeDeepeningEnabled;
            }
            set
            {
                isIterativeDeepeningEnabled = value;
                OnPropertyChanged("IsIterativeDeepeningEnabled");
            }
        }

        public int CurrentDepth
        {
            get
            {
                return currentDepth;
            }
            private set
            {
                currentDepth = value;
                OnPropertyChanged("CurrentDepth");
            }
        }

        public int MaxDepth
        {
            get
            {
                return maxDepth;
            }
            private set
            {
                maxDepth = value;
                OnPropertyChanged("MaxDepth");
            }
        }

        public SearchNode RootNode
        {
            get
            {
                return rootNode;
            }
            private set
            {
                rootNode = value;
                OnPropertyChanged("RootNode");
            }
        }

        #endregion


        #region Constructors

        public BaseNegaMaxSolver(bool isIterativeDeepeningEnabled = true, int maxDepth = 30): base()
        {
            IsIterativeDeepeningEnabled = isIterativeDeepeningEnabled;
            MaxDepth = maxDepth;
        }

        #endregion


        #region Protected methods

        protected override void DoSolve()
        {
            RootNode = new SearchNode(Coordinator.CurrentGameState);

            if (IsIterativeDeepeningEnabled)
            {
                CurrentDepth = 0;
                while (CurrentDepth <= MaxDepth && SolverState == SolverState.Running)
                {
                    CurrentDepth = CurrentDepth + 1;
                    RunNegaMaxAtAParticularDepth();
                }
            }
            else
            {
                CurrentDepth = MaxDepth;
                RunNegaMaxAtAParticularDepth();
            }
        }

        private void RunNegaMaxAtAParticularDepth()
        {
            // TODO: Modify this to store the old and new search tree (old depth and new depth).
            // The challenge is to be able to use the old search tree to sort the new tree by the most promising branch first.
            double evaluation = Negamax(RootNode);

            if (SolverState == SolverState.Stopping)
            {
                RootNode.EvaluationStatus = EvaluationStatus.Stopped;
            }
            else
            {
                // A solution was found at this depth. Choose a best solution:
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
        }

        #endregion

        #region Private methods

        private double Negamax(SearchNode searchNode, int depth = 0, double alpha = double.NegativeInfinity, 
            double beta = double.PositiveInfinity)
        {
            int multiplier;
            if (searchNode.ParentNode == null)
            {
                multiplier = searchNode.GameState.PlayerToMoveNext == PlayerType.You ? 1 : -1;
            }
            else
            {
                // A more efficient way, since it doesn't cause game states to be re-generated:
                multiplier = searchNode.Move.PlayerType == PlayerType.Opponent ? 1 : -1;
            }

            if (depth >= CurrentDepth)
            {
                Evaluate(searchNode);
                return multiplier * searchNode.Evaluation;
            }
            else
            {
                searchNode.Expand();

                if (SolverState == SolverState.Stopping)
                {
                    return 0.0;  // Calling code will check solver state and ignore the result
                }

                if (!searchNode.ChildNodes.Any())
                {
                    /* Game is terminal: */
                    Evaluate(searchNode);
                    return multiplier * searchNode.Evaluation;
                }
                else
                {
                    double max = double.NegativeInfinity;
                    bool pruning = false;

                    foreach (SearchNode childNode in searchNode.ChildNodes.OrderByDescending(snode => snode.Evaluation))
                    {
                        if (SolverState == SolverState.Stopping)
                        {
                            childNode.EvaluationStatus = EvaluationStatus.Stopped;
                        }
                        else
                            if (pruning)
                            {
                                childNode.EvaluationStatus = EvaluationStatus.Pruned;
                            }
                            else
                            {
                                double evaluation = -Negamax(childNode, depth + 1, -beta, -alpha);
                                if (SolverState == SolverState.Stopping)
                                {
                                    childNode.EvaluationStatus = EvaluationStatus.Stopped;
                                    return 0.0;
                                }
                                else
                                {
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
                    }
                    if (pruning)
                    {
                        return alpha;
                    }
                    return max;
                }
            }
        }

        #endregion
    }
}
