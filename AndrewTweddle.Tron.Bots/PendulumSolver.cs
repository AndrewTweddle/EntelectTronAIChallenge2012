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

            // TODO: Original test code... remove this... simpler to not use a framework

            int initialX = initialPosition.X;

            int equatorY;
            int distance;
            bool flipY = false;

            if (initialY > 14)
            {
                equatorY = 15;
                flipY = true;
                distance = initialY - equatorY;
            }
            else
            {
                equatorY = 14;
                flipY = false;
                distance = equatorY - initialY;
            }

            /* Original...
            Step<PendulumState> program = new StepSequence<PendulumState> {
                FlipX = FlipSetting.No,
                FlipY = (flipY ? FlipSetting.Yes : FlipSetting.No),
                Steps = new Step<PendulumState>[] {
                    new MoveInALine<PendulumState> {
                        Name = "MoveToEquator",
                        Direction = Direction.Down,
                        Distance = distance,
                        AfterStep = InitializePendulumState
                    },
                    new RepeatWithIncreasingLevels<PendulumState> {
                        Name = "Pendulum",
                        GetInitialLevel = previousLevel => 1,
                        GetFinalLevel = previousLevel => (Constants.Rows - 2)/4,  // Lower hemisphere => divide by 2; go down 2 blocks per turn => divide by 2 again
                        Steps = new Step<PendulumState>[] {
                            new MoveInALine<PendulumState>{ Name = "1_L", Direction = Direction.Left,  GetDistanceFromLevel = level => level },         // N left
                            new MoveInALine<PendulumState>{ Name = "2_U", Direction = Direction.Up,    GetDistanceFromLevel = level => 2 * level - 1},  // Nth odd number up
                            new MoveInALine<PendulumState>{ Name = "3_L", Direction = Direction.Left,  Distance = 1},                                   // 1 left
                            new MoveInALine<PendulumState>{ Name = "4_D", Direction = Direction.Down,  GetDistanceFromLevel = level => 2 * level},  // Nth odd number +1 down
                            new MoveInALine<PendulumState>{ Name = "5_R", Direction = Direction.Right, GetDistanceFromLevel = level => level + 2},  // N+2 right - split into N+1 right, then evaluate whether to backtrack or expand right
                            new MoveInALine<PendulumState>{ Name = "6_U", Direction = Direction.Up,    GetDistanceFromLevel = level => level + 1},  // N+1 up
                            new MoveInALine<PendulumState>{ Name = "7_R", Direction = Direction.Right, Distance = 1},                               // 1 right
                            new MoveInALine<PendulumState>{ Name = "8_D", Direction = Direction.Down,  GetDistanceFromLevel = level => level + 2},  // N+2 down
                            new MoveInALine<PendulumState>{ Name = "9_L", Direction = Direction.Left,  GetDistanceFromLevel = level => level + 2}   // Move back under the vertical line
                        }
                    }
                }
            };
             */

            Step<PendulumState> program = new StepSequence<PendulumState>
            {
                FlipX = FlipSetting.No,
                FlipY = (flipY ? FlipSetting.Yes : FlipSetting.No),
                Steps = new Step<PendulumState>[] {
                    new MoveInALine<PendulumState> {
                        Name = "MoveToEquator",
                        Direction = Direction.Down,
                        Distance = distance,
                        AfterStep = InitializePendulumState
                    },
                    new StepSequence<PendulumState> {
                        Name = "Pendulum",
                        Repeat = true,
                        Steps = new Step<PendulumState>[] {
                            new MoveToAPoint<PendulumState>
                            { 
                                Name = "1_L", 
                                GetPosition = GetFirstPosition,
                                AfterEachMove = AfterFirstPosition
                            },         // N left
                            new MoveInALine<PendulumState>{ Name = "2_U", Direction = Direction.Up,    GetDistanceFromLevel = level => 2 * level - 1},  // Nth odd number up
                            new MoveInALine<PendulumState>{ Name = "3_L", Direction = Direction.Left,  Distance = 1},                                   // 1 left
                            new MoveInALine<PendulumState>{ Name = "4_D", Direction = Direction.Down,  GetDistanceFromLevel = level => 2 * level},  // Nth odd number +1 down
                            new MoveInALine<PendulumState>{ Name = "5_R", Direction = Direction.Right, GetDistanceFromLevel = level => level + 2},  // N+2 right - split into N+1 right, then evaluate whether to backtrack or expand right
                            new MoveInALine<PendulumState>{ Name = "6_U", Direction = Direction.Up,    GetDistanceFromLevel = level => level + 1},  // N+1 up
                            new MoveInALine<PendulumState>{ Name = "7_R", Direction = Direction.Right, Distance = 1},                               // 1 right
                            new MoveInALine<PendulumState>{ Name = "8_D", Direction = Direction.Down,  GetDistanceFromLevel = level => level + 2},  // N+2 down
                            new MoveInALine<PendulumState>{ Name = "9_L", Direction = Direction.Left,  GetDistanceFromLevel = level => level + 2}   // Move back under the vertical line
                        }
                    }
                }
            };

            outcome = program.RunRulesEngine(actualGameState);
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
        }

        public void InitializePendulumState(Step<PendulumState> step, GameState gameStateAfterStep, GameState currentGameState, RuleOutcome outcome)
        {
            if (outcome.Status != RuleStatus.NextMoveSuggested && outcome.Status == RuleStatus.NoFurtherSuggestions)
            {
                return;
            }

            Position position = gameStateAfterStep.YourCell.Position;
            Block filledBlock = new Block
            {
                LeftX = position.X,
                RightX = position.X,
                TopY = position.Y,
                BottomY = position.Y
            };
            step.RootStep.State = new PendulumState
            {
                Level = 1,
                FilledBlock = filledBlock
                // TODO: LeftGap = 
                // TODO: RightGap = 
            };
        }

        public Position GetFirstPosition (Step<PendulumState> step, GameState mutableGS, GameState currentGS)
        {
            PendulumState state = step.RootStep.State;
            if (state.Level % 2 == 1)
            {
                // Going left:
                int x = state.FilledBlock.LeftX - 1;
                int y = state.FilledBlock.TopY - 1;
                return new Position(x, y);
            }
            else
            {
                // Going right:
                int x = state.FilledBlock.RightX + 1;
                int y = state.FilledBlock.TopY - 1;
                return new Position(x, y);
            }
        }

        public void AfterFirstPosition(Step<PendulumState> step, GameState mutableGS, GameState currentGS)
        {
            Position yourPos = mutableGS.YourCell.Position;
            PendulumState rootState = step.RootStep.State;
            Block filledBlock = rootState.FilledBlock;
            rootState.FilledBlock.LeftX = yourPos.X;
            if (filledBlock.TopY > yourPos.Y)
            {
                filledBlock.TopY = yourPos.Y;
            }
        }

        public Position GetSecondPosition(Step<PendulumState> step, GameState mutableGS, GameState currentGS)
        {
            PendulumState state = step.RootStep.State;
            int x = state.FilledBlock.LeftX - 1;
            int y = state.FilledBlock.BottomY + 1;
            return new Position(x, y);
        }

        public Position GetThirdPosition(Step<PendulumState> step, GameState mutableGS, GameState currentGS)
        {
            PendulumState state = step.RootStep.State;
            int x = state.FilledBlock.LeftX - 1;
            int y = state.FilledBlock.TopY - 1;
            return new Position(x, y);
        }

        private RuleOutcome SolveWhenStartingInNorthernHemisphere(GameState mutableGameState, GameState actualGameState)
        {
            RuleOutcome outcome;
            Position nextPosition = mutableGameState.YourOriginalCell.Position;
            int nextMoveNumber = 0;

            /* Move to the equator: */
            int distance = 14 - nextPosition.Y;
            for (int i = 0; i < distance; i++)
            {
                nextMoveNumber++;
                nextPosition = new Position(nextPosition.X, nextPosition.Y + 1);
                outcome = RulesEngineHelper.MoveToNextPosition(mutableGameState, actualGameState, nextPosition, nextMoveNumber);
                if (outcome.Status != RuleStatus.NoFurtherSuggestions)
                {
                    return outcome;
                }
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

    }
}
