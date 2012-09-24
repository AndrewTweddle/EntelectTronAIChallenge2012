using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.RulesEngine
{
    public class RuleOutcome
    {
        public RuleStatus Status { get; set; }
        public Move SuggestedMove { get; set; }
        public Exception ExceptionThrown { get; set; }
    }
}
