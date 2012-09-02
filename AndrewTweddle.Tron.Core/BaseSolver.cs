using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;

namespace AndrewTweddle.Tron.Core
{
    public abstract class BaseSolver: ISolver, INotifyPropertyChanged
    {
        protected abstract void DoSolve();

        private SolverState solverState;
        private object solverStateLock = new object();
        private object solverStopLock = new object();

        public SolverState SolverState
        {
            get 
            { 
                return solverState; 
            }
            set
            {
                lock (solverStateLock)
                {
                    solverState = value;
                    OnPropertyChanged("SolverState");
                }
            }
        }

        public virtual void Stop()
        {
            lock (solverStopLock)
            {
                if (SolverState == SolverState.Running)
                {
                    SolverState = SolverState.Stopping;
                }
            }
        }

        public virtual void Solve()
        {
            if (Coordinator == null)
            {
                throw new ApplicationException("The solver has no coordinator");
            }
            try
            {
                SolverState = SolverState.Running;
                try
                {
                    DoSolve();
                }
                finally
                {
                    SolverState = SolverState.NotRunning;
                }
            }
            catch (ThreadAbortException tax)
            {
                // Swallow this - it's only for when the stopping method doesn't work timeously
                Thread.ResetAbort();
            }
        }
        
        public Coordinator Coordinator { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }
    }
}
