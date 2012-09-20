using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using AndrewTweddle.Tron.Core.Algorithms;
using AndrewTweddle.Tron.Core.Evaluators;

namespace AndrewTweddle.Tron.Core
{
    public abstract class BaseNegaMaxSolver: BaseSolver
    {
        #region Abstract methods

        protected abstract void Evaluate(SearchNode searchNode, GameState gameStateAfterMove);

        #endregion


        #region Private member variables

        private bool isIterativeDeepeningEnabled = true;
        private int currentDepth;
        private int maxDepth;
        private SearchNode rootNode;
        private int lastDepthCompleted;
        private Dictionary<int, int> numberOfEvaluationsByDepth = new Dictionary<int, int>();

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

        public Dictionary<int, int> NumberOfEvaluationsByDepth
        {
            get
            {
                return numberOfEvaluationsByDepth;
            }
        }

        public int LastDepthCompleted
        {
            get
            {
                return lastDepthCompleted;
            }
            set
            {
                lastDepthCompleted = value;
                OnPropertyChanged("LastDepthCompleted");
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
#if DEBUG
            Stopwatch swatch = Stopwatch.StartNew();
            numberOfEvaluationsByDepth.Clear();
#endif
            LastDepthCompleted = 0;

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

#if DEBUG
            swatch.Stop();
            int numberOfCalculationsAtAllDepths = NumberOfEvaluationsByDepth.Values.Sum();
            string annotation = String.Format("{0} evals, depth {1}, {2} seconds",
                numberOfCalculationsAtAllDepths, LastDepthCompleted, swatch.Elapsed.TotalMilliseconds / 1000.0);
            Coordinator.BestMoveSoFar.Annotation = annotation;
            System.Diagnostics.Debug.WriteLine("***** TOTAL EVALUATIONS: {0}", annotation);
            System.Diagnostics.Debug.WriteLine("=================================");
#endif
        }

        private void RunNegaMaxAtAParticularDepth()
        {
            SearchNode newRootNode = new SearchNode(Coordinator.CurrentGameState);
            double evaluation = Negamax(newRootNode);

            if (SolverState == SolverState.Stopping)
            {
                newRootNode.EvaluationStatus = EvaluationStatus.Stopped;
            }
            else
            {
                RootNode = newRootNode;
                LastDepthCompleted = CurrentDepth;

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
#if DEBUG
                    // Perform algorithms on chosen node, since search nodes no longer do this (except at leaf nodes):
                    Dijkstra.Perform(chosenChildNode.GameState);
                    BiconnectedComponentsAlgorithm bcAlg = new BiconnectedComponentsAlgorithm();
                    bcAlg.Calculate(chosenChildNode.GameState, ReachableCellsThenClosestCellsThenDegreesOfClosestCellsEvaluator.Instance);
#endif
                    Coordinator.SetBestMoveSoFar(chosenChildNode.GameState);
                }
                else
                {
                    Debug.WriteLine("NegaMax found no best child");
                }

#if DEBUG
                int numberOfEvaluationsAtThisDepth = numberOfEvaluationsByDepth.ContainsKey(CurrentDepth) ? numberOfEvaluationsByDepth[CurrentDepth] : 0;
                System.Diagnostics.Debug.WriteLine("*** NegaMax performed {0} evaluations at depth {1}", numberOfEvaluationsAtThisDepth, CurrentDepth);
#endif
            }
        }

        #endregion

        #region Private methods

        private double Negamax(SearchNode searchNode, int depth = 0, double alpha = double.NegativeInfinity, 
            double beta = double.PositiveInfinity)
        {
            GameState gameState = searchNode.GameState;

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
                // Perform the algorithms on the game state, before running the evaluation:
                Dijkstra.Perform(gameState);
                BiconnectedComponentsAlgorithm bcAlg = new BiconnectedComponentsAlgorithm();
                bcAlg.Calculate(searchNode.GameState, ReachableCellsThenClosestCellsThenDegreesOfClosestCellsEvaluator.Instance);

                // This has a possible race condition, because searchNode.GameState could be held by a weak reference.
                // So the calculations above might be invalidated when searchNode.GameState is retrieved in the Evaluate() method.
                // TODO: Modify Evaluate to take the game state as a parameter.
                Evaluate(searchNode, gameState);

                int evaluationsAtThisDepth = numberOfEvaluationsByDepth.ContainsKey(depth) ? numberOfEvaluationsByDepth[depth] : 0;
                numberOfEvaluationsByDepth[depth] = evaluationsAtThisDepth + 1;

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
                    if (gameState.IsGameOver)
                    {
                        if (gameState.PlayerToMoveNext == PlayerType.You)
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
                        throw new ApplicationException("Game state should be over if there are no child search nodes!");
                    }
                    return multiplier * searchNode.Evaluation;
                }
                else
                {
                    double max = double.NegativeInfinity;
                    bool pruning = false;

                    foreach (SearchNode childNode in searchNode.ChildNodes)
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
