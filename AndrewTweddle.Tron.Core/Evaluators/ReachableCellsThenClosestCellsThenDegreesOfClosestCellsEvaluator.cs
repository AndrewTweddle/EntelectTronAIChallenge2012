using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.Evaluators
{
    public class ReachableCellsThenClosestCellsThenDegreesOfClosestCellsEvaluator: MetricsEvaluator
    {
        private static MetricsEvaluator instance;

        public static MetricsEvaluator Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ReachableCellsThenClosestCellsThenDegreesOfClosestCellsEvaluator();
                }
                return instance;
            }
        }

        public override double Evaluate(Metrics metrics)
        {
            return metrics.NumberOfCellsReachableByPlayer * 1000000
                + metrics.NumberOfCellsClosestToPlayer * 1000
                + metrics.TotalDegreesOfCellsClosestToPlayer;
        }
    }
}
