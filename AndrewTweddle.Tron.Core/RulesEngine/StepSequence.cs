using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.RulesEngine
{
    public class StepSequence<TState>: CompositeStep<TState>
        where TState: class
    {
        public override RuleOutcome ReplayAndChooseNextMove(GameState gameStateBeforeStep, GameState currentGameState)
        {
            foreach (Step<TState> step in Steps)
            {
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
            return new RuleOutcome
            {
                Status = RuleStatus.NoFurtherSuggestions
            };
        }
    }
}
