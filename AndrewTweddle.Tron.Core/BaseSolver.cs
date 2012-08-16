using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    public abstract class BaseSolver: ISolver
    {
        public Coordinator Coordinator { get; private set; }

        private BaseSolver()
        {
        }

        public BaseSolver(Coordinator coordinator)
        {
            Coordinator = coordinator;
        }

        public void Solve()
        {
        }
    }
}
