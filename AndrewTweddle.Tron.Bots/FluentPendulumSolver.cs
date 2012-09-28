using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;
using AndrewTweddle.Tron.Core.RulesEngine;

namespace AndrewTweddle.Tron.Bots
{
    /// <summary>
    /// This was my first attempt at a rules engine. The engine was built as a framework, making it more complex to build and understand.
    /// </summary>
    public class FluentPendulumSolver : BaseSolver
    {
        protected override void DoSolve()
        {
            RuleOutcome outcome;
            GameState actualGameState = Coordinator.CurrentGameState.Clone();
            Position initialPosition = actualGameState.YourOriginalCell.Position;
            int initialY = initialPosition.Y;
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

        public Position GetFirstPosition(Step<PendulumState> step, GameState mutableGS, GameState currentGS)
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
    }
}
