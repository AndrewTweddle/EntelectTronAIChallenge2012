using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.RulesEngine
{
    public abstract class Step<TState> 
        where TState:class
    {
        public abstract RuleOutcome ReplayAndChooseNextMove(GameState gameStateBeforeStep, GameState currentGameState);

        public TState State { get; set; }

        public Step<TState> ParentStep { get; set; }
        public Step<TState> RootStep
        {
            get
            {
                Step<TState> rootStep = this;
                while (rootStep.ParentStep != null)
                {
                    rootStep = ParentStep;
                }
                return rootStep;
            }
        }

        public string Name { get; set; }
        public FlipSetting FlipX { get; set; }
        public FlipSetting FlipY { get; set; }
        public double FlipXAxis { get; set; }
        public double FlipYAxis { get; set; }
        public bool Repeat { get; set; }

        public Action<Step<TState>, GameState, GameState> AfterEachMove;
        public Action<Step<TState>, GameState, GameState, RuleOutcome> AfterEachRepetition;
        public Action<Step<TState>, GameState, GameState, RuleOutcome> AfterStep;
        
        /// <summary>
        /// Some derived classes may make use of this enumeration variable
        /// </summary>
        public int Level { get; set; }

        public Step()
        {
            FlipYAxis = 14.5;
        }

        public RuleOutcome RunRulesEngine(GameState currentGameState)
        {
            try
            {
                RuleOutcome outcome;
                GameState gameStateBeforeStep = RulesEngineHelper.GetInitialGameState(currentGameState);
                do
                {
                    outcome = ReplayAndChooseNextMove(gameStateBeforeStep, currentGameState);
                    if (AfterEachRepetition != null)
                    {
                        AfterEachRepetition(this, gameStateBeforeStep, currentGameState, outcome);
                    }
                }
                while (Repeat && outcome.Status == RuleStatus.NoFurtherSuggestions);

                if (outcome.Status == RuleStatus.NextMoveSuggested || outcome.Status == RuleStatus.NoFurtherSuggestions)
                {
                    AfterStep(this, gameStateBeforeStep, currentGameState, outcome);
                }
                return outcome;
            }
            catch (Exception exc)
            {
                RuleOutcome outcome = new RuleOutcome
                {
                    Status = RuleStatus.ExceptionThrown,
                    ExceptionThrown = exc
                };
                return outcome;
            }
        }

        protected int GetNormalizedX(int x)
        {
            if (x < 0 || x >= Constants.Columns)
            {
                x = x % Constants.Columns;
            }
            if (x < 0)
            {
                x += Constants.Columns;
            }
            return x;
        }

        protected int GetFlippedXOffset(int x)
        {
            return GetFlippedX(x, flipXAxis: 0, shouldNormalize: false);
        }

        protected int GetFlippedX(int x, bool shouldNormalize = true)
        {
            return GetFlippedX(x, FlipXAxis, shouldNormalize);
        }

        protected int GetFlippedX(int x, double flipXAxis, bool shouldNormalize = true)
        {
            if ((FlipX == FlipSetting.Inherit && ParentStep == null) || (FlipX == FlipSetting.No))
            {
                return x;
            }

            if (FlipX == FlipSetting.Yes)
            {
                int newX = (int)(2 * flipXAxis - x);
                if (shouldNormalize)
                {
                    newX = GetNormalizedX(newX);
                }
                return newX;
            }

            // Must be inherited:
            return ParentStep.GetFlippedX(x, flipXAxis);
        }

        protected int GetFlippedYOffset(int y)
        {
            return GetFlippedY(y, flipYAxis: 0);
        }

        protected int GetFlippedY(int y)
        {
            return GetFlippedY(y, FlipYAxis);
        }

        protected int GetFlippedY(int y, double flipYAxis)
        {
            if ((FlipY == FlipSetting.Inherit && ParentStep == null) || (FlipY == FlipSetting.No))
            {
                return y;
            }

            if (FlipY == FlipSetting.Yes)
            {
                int newY = (int) (2 * flipYAxis - y);
                return newY;
            }

            // Must be inherited:
            return ParentStep.GetFlippedY(y, flipYAxis);
        }

        protected TState GetState()
        {
            if (State != null)
            {
                return State;
            }
            if (ParentStep != null)
            {
                return ParentStep.State;
            }
            return null;
        }

        /// <summary>
        /// A helper method for moving through a sequence of points for a particular step
        /// </summary>
        /// <param name="gameStateBeforeStep"></param>
        /// <param name="currentGameState"></param>
        /// <param name="nextPosition"></param>
        /// <param name="nextMoveNumber"></param>
        /// <returns></returns>
        public RuleOutcome MoveToNextPosition(GameState gameStateBeforeStep, GameState currentGameState, Position nextPosition, int nextMoveNumber)
        {
            CellState actualCellState = currentGameState[nextPosition];

            // Check whether the correct move was made to this position:
            OccupationStatus statusOfNextPosition = actualCellState.OccupationStatus;

            switch (statusOfNextPosition)
            {
                case OccupationStatus.YourWall:
                case OccupationStatus.You:
                    // Check move number:
                    if (actualCellState.MoveNumber != nextMoveNumber)
                    {
                        return new RuleOutcome
                        {
                            Status = RuleStatus.DeviationFromRuleDetected
                        };
                    }
                    break;

                case OccupationStatus.Opponent:
                case OccupationStatus.OpponentWall:
                    return new RuleOutcome
                    {
                        Status = RuleStatus.DeviationFromRuleDetected
                    };
                case OccupationStatus.Clear:
                    return new RuleOutcome
                    {
                        Status = RuleStatus.NextMoveSuggested,
                        SuggestedMove = new Move(PlayerType.You, nextMoveNumber, nextPosition)
                    };
            }

            // Apply chosen move:
            gameStateBeforeStep.MoveToPosition(nextPosition);

            if (AfterEachMove != null)
            {
                AfterEachMove(this, gameStateBeforeStep, currentGameState);
            }

            // Apply opponent's move to current position:
            int opponentsMoveNumber = currentGameState.PlayerWhoMovedFirst == PlayerType.You ? nextMoveNumber : nextMoveNumber + 1;
            CellState opponentsActualMove = currentGameState.GetCellByMoveNumber(PlayerType.Opponent, opponentsMoveNumber);
            gameStateBeforeStep.MoveToPosition(opponentsActualMove.Position);

            return new RuleOutcome
            {
                Status = RuleStatus.NoFurtherSuggestions
            };
        }
    }
}
