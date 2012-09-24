using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core.RulesEngine
{
    public abstract class CompositeStep<TState>: Step<TState>
        where TState: class
    {
        private Step<TState>[] steps;

        public Step<TState>[] Steps 
        {
            get
            {
                return steps;
            }
            set
            {
                if (steps != value)
                {
                    steps = value;
                    if (steps != null)
                    {
                        foreach (Step<TState> step in steps)
                        {
                            step.ParentStep = this;
                        }
                    }
                }
            }
        }
    }
}
