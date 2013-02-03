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
        // Lock timeouts in milliseconds - at all costs avoid a deadlock which could cause the solver to time out...
        public static readonly int SOLVER_STATE_LOCK_TIMEOUT = 10;
        public static readonly int SOLVER_STOP_LOCK_TIMEOUT = 100;
        public static readonly int DELEGATED_SOLVER_LOCK_TIMEOUT = 10;

        protected abstract void DoSolve();

        private SolverState solverState;
        private object solverStateLock = new object();
        private object solverStopLock = new object();
        private object delegatedSolverLock = new object();

        private ISolver delegatedSolver;

        public string Name
        {
            get
            {
                return GetType().Name;
            }
        }

        public SolverState SolverState
        {
            get 
            { 
                return solverState; 
            }
            set
            {
                bool isSolverStateLocked = Monitor.TryEnter(solverStateLock, SOLVER_STATE_LOCK_TIMEOUT);
                try
                {
#if DEBUG
                    if (!isSolverStateLocked)
                    {
                        System.Diagnostics.Debug.WriteLine("Lock timeout with solver state lock");
                    }
#endif
                    solverState = value;
                    OnPropertyChanged("SolverState");
                    if (delegatedSolver != null)
                    {
                        bool isDelegatedSolverLocked = Monitor.TryEnter(delegatedSolverLock, DELEGATED_SOLVER_LOCK_TIMEOUT);
                        try
                        {
#if DEBUG
                            if (!isDelegatedSolverLocked)
                            {
                                System.Diagnostics.Debug.WriteLine("Lock timeout with delegated solver lock");
                            }
#endif
                            if (delegatedSolver != null)
                            {
                                delegatedSolver.SolverState = solverState;
                            }
                        }
                        finally
                        {
                            if (isDelegatedSolverLocked)
                            {
                                Monitor.Exit(delegatedSolverLock);
                            }
                        }
                    }
                }
                finally
                {
                    if (isSolverStateLocked)
                    {
                        Monitor.Exit(solverStateLock);
                    }
                }
            }
        }

        public virtual void Stop()
        {
            bool isSolverStopLocked = Monitor.TryEnter(solverStopLock, SOLVER_STOP_LOCK_TIMEOUT);
            try
            {
#if DEBUG
                if (!isSolverStopLocked)
                {
                    System.Diagnostics.Debug.WriteLine("Lock timeout with solver stop lock");
                }
#endif
                if (SolverState == SolverState.Running)
                {
                    SolverState = SolverState.Stopping;
                }
            }
            finally
            {
                if (isSolverStopLocked)
                {
                    Monitor.Exit(solverStopLock);
                }
            }
            if (delegatedSolver != null)
            {
                bool isDelegatedSolverLocked = Monitor.TryEnter(delegatedSolverLock, DELEGATED_SOLVER_LOCK_TIMEOUT);
                try
                {
#if DEBUG
                    if (!isDelegatedSolverLocked)
                    {
                        System.Diagnostics.Debug.WriteLine("Lock timeout with delegated solver lock");
                    }
#endif
                    if (delegatedSolver != null)
                    {
                        delegatedSolver.Stop();
                    }
                }
                finally
                {
                    if (isDelegatedSolverLocked)
                    {
                        Monitor.Exit(delegatedSolverLock);
                    }
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

        protected void DelegateSolvingToAnotherSolver(ISolver solver)
        {
            bool isDelegatedSolverLocked;
            solver.Coordinator = this.Coordinator;
            try
            {
                bool isSolverStopLocked = Monitor.TryEnter(solverStopLock, SOLVER_STOP_LOCK_TIMEOUT);
                try
                {
#if DEBUG
                    if (!isSolverStopLocked)
                    {
                        System.Diagnostics.Debug.WriteLine("Lock timeout with solver stop lock");
                    }
#endif
                    if (SolverState == SolverState.Running)
                    {
                        isDelegatedSolverLocked = Monitor.TryEnter(delegatedSolverLock, DELEGATED_SOLVER_LOCK_TIMEOUT);
                        try
                        {
#if DEBUG
                            if (!isDelegatedSolverLocked)
                            {
                                System.Diagnostics.Debug.WriteLine("Lock timeout with delegated solver lock");
                            }
#endif
                            if (SolverState == SolverState.Running)
                            {
                                delegatedSolver = solver;
                            }
                        }
                        finally
                        {
                            if (isDelegatedSolverLocked)
                            {
                                Monitor.Exit(delegatedSolverLock);
                            }
                        }
                    }
                    else
                    {
                        solver.SolverState = this.SolverState;
                    }
                }
                finally
                {
                    if (isSolverStopLocked)
                    {
                        Monitor.Exit(solverStopLock);
                    }
                }

                if (delegatedSolver != null && delegatedSolver.SolverState != SolverState.Stopping)
                {
                    try
                    {
                        delegatedSolver.Solve();
                    }
                    catch (NullReferenceException exc)
                    {
                        System.Diagnostics.Debug.WriteLine(exc);
                        // Swallow exception, since it may be caused by a race condition, but we can't risk deadlocks
                    }
                }
            }
            finally
            {
                /* Set delegatedSolver to null inside a lock: */
                isDelegatedSolverLocked = Monitor.TryEnter(delegatedSolverLock, DELEGATED_SOLVER_LOCK_TIMEOUT);
                try
                {
#if DEBUG
                    if (!isDelegatedSolverLocked)
                    {
                        System.Diagnostics.Debug.WriteLine("Lock timeout with delegated solver lock");
                    }
#endif
                    delegatedSolver = null;
                }
                finally
                {
                    if (isDelegatedSolverLocked)
                    {
                        Monitor.Exit(delegatedSolverLock);
                    }
                }
            }
        }
    }
}
