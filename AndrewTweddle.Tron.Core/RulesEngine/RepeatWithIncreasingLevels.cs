using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.RulesEngine
{
    public class RepeatWithIncreasingLevels<TState> : CompositeStep<TState> where TState: class
    {
        public int InitialLevel { get; set; }
        public int FinalLevel { get; set; }

        /// <summary>
        /// If GetInitialLevel is set, then the InitialLevel property is ignored.
        /// </summary>
        public Func<int, int> GetInitialLevel { get; set; }

        /// <summary>
        /// If GetFinalLevel is set, then the FinalLevel property is ignored.
        /// </summary>
        public Func<int, int> GetFinalLevel { get; set; }

        public override RuleOutcome ReplayAndChooseNextMove(GameState gameStateBeforeStep, GameState currentGameState)
        {
            // Allow chaining of levels:
            int previousLevel = 0;
            if (ParentStep != null)
            {
                previousLevel = ParentStep.Level;
            }

            int initialLevel = InitialLevel;
            if (GetInitialLevel != null)
            {
                initialLevel = GetInitialLevel(previousLevel);
            }

            int finalLevel = FinalLevel;
            if (GetFinalLevel != null)
            {
                finalLevel = GetFinalLevel(previousLevel);
            }

            for (int level = initialLevel; level <= finalLevel; level++)
            {
                foreach (Step<TState> step in Steps)
                {
                    step.Level = level;
                    RuleOutcome outcome = step.ReplayAndChooseNextMove(gameStateBeforeStep, currentGameState);
                    switch (outcome.Status)
                    {
                        case RuleStatus.DeviationFromRuleDetected:
                        case RuleStatus.NextMoveSuggested:
                        case RuleStatus.OpponentBlockedMove:
                        case RuleStatus.ExceptionThrown:
                            return outcome;
                        // Otherwise RuleStatus.NoFurtherSuggestions
                    }
                }
            }

            return new RuleOutcome
            {
                Status = RuleStatus.NoFurtherSuggestions
            };
        }
    }
}
