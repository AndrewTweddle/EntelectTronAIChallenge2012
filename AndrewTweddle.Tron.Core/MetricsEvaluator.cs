using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    public abstract class MetricsEvaluator
    {
        public abstract double Evaluate(Metrics metrics);
    }
}
