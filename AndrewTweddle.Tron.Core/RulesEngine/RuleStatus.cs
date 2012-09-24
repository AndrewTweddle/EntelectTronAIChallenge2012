using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.RulesEngine
{
    public enum RuleStatus
    {
        NextMoveSuggested,
        NoFurtherSuggestions,
        DeviationFromRuleDetected,
        OpponentBlockedMove,
        ExceptionThrown
    }
}
