using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;
using AndrewTweddle.Tron.Core.RulesEngine;
using AndrewTweddle.Tron.Core.Algorithms;

namespace AndrewTweddle.Tron.Bots
{
    /// <summary>
    /// An experimental solver, using a rules engine to get the normal solver off to a good start.
    /// This is because the player who keeps their "gap" along the equator open for 2 way traffic seems to usually beat the solver which closes its gap.
    /// Unfortunately time ran out on me, so there was no opportunity to use this in the competition.
    /// <remarks>
    /// Interestingly enough, my initial rules engine was a framework, but that quickly became too complicated. 
    /// Converting the code to act like a library was much easier to code against (albeit with a method that would have grown into a monster had I continued).
    /// My suspicion is that, with complex algorithms, the navigability benefits of "sequential code" seem to outweigh the benefits of "lots of small methods".
    /// Unfortunately I didn't get a chance to see how well this scales as the method size grows, and where the tipping point lies.
    /// </remarks>
    /// </summary>
    public class PendulumSolver: BaseSolver
    {
        protected override void DoSolve()
        {
            RuleOutcome outcome;
            GameState actualGameState = Coordinator.CurrentGameState.Clone();
            Position initialPosition = actualGameState.YourOriginalCell.Position;
            int initialY = initialPosition.Y;
            GameState mutableGameState = RulesEngineHelper.GetInitialGameState(actualGameState);

            try
            {
                if (initialY <= 14)  // TODO: Remove hard-coding!
                {
                    outcome = SolveWhenStartingInNorthernHemisphere(mutableGameState, actualGameState);
                }
                else
                {
                    // TODO: outcome = SolveWhenStartingInSouthernHemisphere(mutableGameState, newGameState);
                    outcome = new RuleOutcome
                    {
                        Status = RuleStatus.NoFurtherSuggestions
                    };
                }
            }
            catch (Exception exc)
            {
                outcome = new RuleOutcome
                {
                    Status = RuleStatus.ExceptionThrown,
                    ExceptionThrown = exc
                };
            }

            if (outcome.Status == RuleStatus.NextMoveSuggested)
            {
                actualGameState.MakeMove(outcome.SuggestedMove);
                Coordinator.SetBestMoveSoFar(actualGameState);
            }
            else
            {
                NegaMaxSolver solver = new NegaMaxSolver();
                DelegateSolvingToAnotherSolver(solver);
            }

            return;
        }

        private RuleOutcome SolveWhenStartingInNorthernHemisphere(GameState mutableGameState, GameState actualGameState)
        {
            RuleOutcome outcome;
            Position nextPosition = mutableGameState.YourOriginalCell.Position;
            int nextMoveNumber = 0;

            Block leftBlock = GetInitialLeftBlock(mutableGameState);
            Block rightBlock = GetInitialRightBlock(mutableGameState);

            /* Move to the equator: */
            int distance = 14 - nextPosition.Y;     // Remove hard-coding
            for (int i = 0; i < distance; i++)
            {
                nextMoveNumber++;
                nextPosition = new Position(nextPosition.X, nextPosition.Y + 1);
                outcome = RulesEngineHelper.MoveToNextPosition(mutableGameState, actualGameState, nextPosition, nextMoveNumber);
                if (outcome.Status != RuleStatus.NoFurtherSuggestions)
                {
                    return outcome;
                }

                // TODO: Update leftBlock and rightBlock
            }

            // Set up a state object to track various information:
            Position position = mutableGameState.YourCell.Position;
            NorthernPendulumBlock filledBlock = new NorthernPendulumBlock
            {
                LeftX = position.X,
                RightX = position.X,
                TopLeftY = position.Y,
                TopRightY = position.Y,
                BottomY = position.Y
            };

            // Swing the pendulum back and forth, increasing its length with each iteration:
            for (int pendulumLevel = 0; pendulumLevel < 13; pendulumLevel++)
            {
                Position targetPosition;
                int targetX, targetY;

                // *** Move to first position:
                Dijkstra.Perform(mutableGameState);

                // Only move to first and second position if this reduces the opponent's gap:
                // bool isOpponentCloserToGapOnLeft = 

                if (pendulumLevel % 2 == 0)
                {
                    // Going left:
                    targetX = Helper.NormalizedX(filledBlock.LeftX - 1);
                    targetY = filledBlock.TopLeftY - 1;
                }
                else
                {
                    // Going right:
                    targetX = Helper.NormalizedX(filledBlock.RightX + 1);
                    targetY = filledBlock.TopRightY - 1;
                }
                targetPosition = new Position(targetX, targetY);
                outcome = RulesEngineHelper.MoveToPositionAlongAnyRoute(mutableGameState, actualGameState, ref nextMoveNumber, targetPosition);
                if (outcome.Status != RuleStatus.NoFurtherSuggestions)
                {
                    return outcome;
                }

                // Update filled block:
                if (pendulumLevel % 2 == 0)
                {
                    // Going left:
                    filledBlock.LeftX = targetX;
                    filledBlock.TopLeftY = targetY;
                }
                else
                {
                    // Going right:
                    filledBlock.RightX = targetX;
                    filledBlock.TopRightY = targetY;
                }

                // *** Move to second position:
                Dijkstra.Perform(mutableGameState);

                if (pendulumLevel % 2 == 0)
                {
                    // Going left:
                    targetX = Helper.NormalizedX(filledBlock.LeftX - 1);
                }
                else
                {
                    // Going right:
                    targetX = Helper.NormalizedX(filledBlock.RightX + 1);
                }
                targetY = filledBlock.BottomY + 1;
                targetPosition = new Position(targetX, targetY);
                outcome = RulesEngineHelper.MoveToPositionAlongAnyRoute(mutableGameState, actualGameState, ref nextMoveNumber, targetPosition);
                if (outcome.Status != RuleStatus.NoFurtherSuggestions)
                {
                    return outcome;
                }

                // Update filled block:
                if (pendulumLevel % 2 == 0)
                {
                    // Going left:
                    filledBlock.LeftX = targetX;
                }
                else
                {
                    // Going right:
                    filledBlock.RightX = targetX;
                }
                filledBlock.BottomY = targetY;

                // *** Move to third position:
                Dijkstra.Perform(mutableGameState);

                if (pendulumLevel % 2 == 0)
                {
                    // Going right:
                    targetX = filledBlock.RightX;
                }
                else
                {
                    // Going left:
                    targetX = filledBlock.LeftX;
                }
                targetY = filledBlock.BottomY; 
                targetPosition = new Position(targetX, targetY);
                outcome = RulesEngineHelper.MoveToPositionAlongAnyRoute(mutableGameState, actualGameState, ref nextMoveNumber, targetPosition);
                if (outcome.Status != RuleStatus.NoFurtherSuggestions)
                {
                    return outcome;
                }
            }

            // No more pre-programmed moves to make... hand over to normal solver now:
            outcome = new RuleOutcome
            {
                Status = RuleStatus.NoFurtherSuggestions
            };

            return outcome;
        }

        private static Block GetInitialRightBlock(GameState mutableGameState)
        {
            Block rightBlock = new Block();
            rightBlock.LeftX = Helper.NormalizedX(mutableGameState.YourOriginalCell.Position.X + 1);
            int rightX1 = Helper.NormalizedX(mutableGameState.OpponentsOriginalCell.Position.X - 1);
            int rightX2 = Helper.NormalizedX(mutableGameState.OpponentsCell.Position.X - 1);
            if ((rightX2 == 0 && rightX1 == Constants.Columns - 1) || (rightX1 == Constants.Columns - 1 && rightX2 == 0))
            {
                rightBlock.RightX = Constants.Columns - 1;
            }
            else
            {
                rightBlock.RightX = Math.Min(rightX1, rightX2);
            }
            rightBlock.TopY = mutableGameState.YourOriginalCell.Position.Y;
            rightBlock.BottomY = rightBlock.TopY;
            return rightBlock;
        }

        private static Block GetInitialLeftBlock(GameState mutableGameState)
        {
            Block leftBlock = new Block();
            int leftX1 = Helper.NormalizedX(mutableGameState.OpponentsOriginalCell.Position.X + 1);
            int leftX2 = Helper.NormalizedX(mutableGameState.OpponentsCell.Position.X + 1);
            if ((leftX2 == 0 && leftX1 == Constants.Columns - 1) || (leftX1 == Constants.Columns - 1 && leftX2 == 0))
            {
                leftBlock.LeftX = 0;
            }
            {
                leftBlock.LeftX = Math.Max(leftX1, leftX2);
            }
            leftBlock.RightX = Helper.NormalizedX(mutableGameState.YourOriginalCell.Position.X - 1);
            leftBlock.TopY = mutableGameState.YourOriginalCell.Position.Y;
            leftBlock.BottomY = leftBlock.TopY;
            return leftBlock;
        }

    }
}
