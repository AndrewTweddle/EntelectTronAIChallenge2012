using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    public abstract class BaseSolver: ISolver
    {
        protected abstract void DoSolve();

        public void Solve()
        {
            if (Coordinator == null)
            {
                throw new ApplicationException("The solver has no coordinator");
            }
            DoSolve();
        }
        
        public Coordinator Coordinator { get; set; }
    }
}
