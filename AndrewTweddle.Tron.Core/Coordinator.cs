using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Timers;
using System.Threading;

namespace AndrewTweddle.Tron.Core
{
    public class Coordinator
    {
        public static readonly int MAX_DURATION_IN_MILLISECONDS = 4000;
        public static readonly int MAX_GRACE_PERIOD_TO_STOP_IN = 500;

        private object solverTimerLock = new object();
        private System.Threading.Timer solverTimer;

        public bool IsInDebugMode { get; set; }
        public bool IgnoreTimer { get; set; }
        public object BestMoveLock { get; private set; }
        public GameState LastGameStateOfPreviousGame { get; set; }
        public GameState CurrentGameState { get; set; }
        public GameState BestMoveSoFar { get; private set; }  // TODO: Replace with Move not GameState?
        public ISolver Solver { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan MaximumDuration { get; private set; }
        public Thread SolverThread { get; private set; }
        public ManualResetEvent OutputTriggeringEvent { get; private set; }
        public string BinaryGameStateFilePath { get; private set; }
        public string XmlGameStateFilePath { get; private set; }

        public System.Threading.Timer SolverTimer
        {
            get
            {
                return solverTimer;
            }
            private set
            {
                if (solverTimer != value)
                {
                    if (solverTimer != null)
                    {
                        solverTimer.Dispose();
                    }
                    solverTimer = value;
                }
            }
        }

        private Coordinator()
        {
        }

        public Coordinator(ISolver solver, DateTime startTime)
        {
            BestMoveLock = new object();
            solver.Coordinator = this;
            Solver = solver;
            MaximumDuration = TimeSpan.FromMilliseconds(MAX_DURATION_IN_MILLISECONDS);
        }

        public Coordinator(ISolver solver): this(solver, DateTime.Now)
        {
        }

        ~Coordinator()
        {
            if (SolverTimer != null)
            {
                SolverTimer = null;  // This causes the timer to be disposed
            }
        }

        public void SetBestMoveSoFar(GameState bestMove)
        {
            lock (BestMoveLock)
            {
                BestMoveSoFar = bestMove;
            }
        }

        public void SignalATimeout(object state)
        {
            if (OutputTriggeringEvent != null)
            {
                OutputTriggeringEvent.Set();
            }
        }

        private void RunTheSolver()
        {
            ManualResetEvent triggerOutputEvent = OutputTriggeringEvent;
            try
            {
                if (Solver != null)
                {
                    Solver.Solve();
                }
            }
            catch (Exception exc)
            {
                throw;  // Just to have somewhere to put a breakpoint
            }
            finally
            {
                /* Signal that the solver has run: */
                if (triggerOutputEvent == null)
                {
                    triggerOutputEvent.Set();
                }
            }
        }

        public void Run(string tronGameFilePath)
        {
            Process process = Process.GetCurrentProcess();
            StartTime = process.StartTime;
            DateTime now = DateTime.Now;
            if (StartTime + MaximumDuration < now)
            {
                // Process start time has probably not been set, so guess a start time of 1 second ago:
                StartTime = now.Subtract(TimeSpan.FromSeconds(1));
            }

            IEnumerable<RawCellData> cells = GameState.LoadRawCellDataFromTronGameFile(tronGameFilePath);
            Run(cells);
            lock (BestMoveLock)
            {
                SaveGameState();
                if (BestMoveSoFar != null)
                {
                    BestMoveSoFar.SaveTronGameFile(tronGameFilePath);
                }
            }
        }

        public void Run(IEnumerable<RawCellData> cells)
        {
            /* Set up the thread signalling event: */
            OutputTriggeringEvent = new ManualResetEvent(false /* Not signalled yet*/);

            /* Set up a timer to signal when a move must be chosen by: */
            if (!IgnoreTimer)
            {
                DateTime endTime = StartTime + MaximumDuration;
                DateTime now = DateTime.Now;
                TimeSpan remainingTime = endTime - now;
                if (remainingTime <= TimeSpan.Zero)
                {
                    SignalATimeout(null);
                }
                else
                {
                    System.Threading.Timer timer = new System.Threading.Timer(SignalATimeout);
                    timer.Change((long)remainingTime.TotalMilliseconds, Timeout.Infinite);
                }
            }

            /* Determine file paths: */
            string exeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            BinaryGameStateFilePath = Path.Combine(exeFolder, "GameState.bin");
            XmlGameStateFilePath = Path.Combine(exeFolder, "GameState.xml");

            /* Determine previous game state (this will be saved when a new game starts): */
            if (File.Exists(BinaryGameStateFilePath))
            {
                LastGameStateOfPreviousGame = GameState.LoadGameState(BinaryGameStateFilePath, FileType.Binary);
            }
            else
                if (File.Exists(XmlGameStateFilePath))
                {
                    LastGameStateOfPreviousGame = GameState.LoadGameState(XmlGameStateFilePath, FileType.Xml);
                }
                else
                {
                    LastGameStateOfPreviousGame = null;
                }

            /* Backup existing files: */
            if (IsInDebugMode)
            {
                if (File.Exists(BinaryGameStateFilePath))
                {
                    string backupFileName = Path.ChangeExtension(BinaryGameStateFilePath, ".bak.bin");
                    File.Copy(BinaryGameStateFilePath, backupFileName, true /*overWrite*/);
                }

                if (File.Exists(XmlGameStateFilePath))
                {
                    string backupFileName = Path.ChangeExtension(XmlGameStateFilePath, ".bak.xml");
                    File.Copy(XmlGameStateFilePath, backupFileName, true /*overWrite*/);
                }
            }

            /* Create the initial game state: */
            CurrentGameState = LastGameStateOfPreviousGame == null ? new GameState() : LastGameStateOfPreviousGame.Clone();

            /* Run the solver: */
            CurrentGameState.NewGameDetected += ArchiveGameStateOfPreviousGame;
            try
            {
                /* Set up the new game state: */
                CurrentGameState.LoadRawCellData(cells);
                Run();
            }
            finally
            {
                /* Ensure no memory leaks due to dangling events: */
                CurrentGameState.NewGameDetected -= ArchiveGameStateOfPreviousGame;
            }
        }

        public void Run(GameState currentGameState)
        {
            CurrentGameState = currentGameState;
            Run();
        }

        public void RunWithTimer()
        {
            if (!IgnoreTimer)
            {
                /* Set up the thread signalling event: */
                if (OutputTriggeringEvent == null)
                {
                    OutputTriggeringEvent = new ManualResetEvent(false /* Not signalled yet*/);
                }

                DateTime now = DateTime.Now;
                StartTime = now;
                DateTime endTime = StartTime + MaximumDuration;
                TimeSpan remainingTime = endTime - DateTime.Now;

                if (remainingTime <= TimeSpan.Zero)
                {
                    SignalATimeout(null);
                }
                else
                {
                    lock (solverTimerLock)
                    {
                        SolverTimer = new System.Threading.Timer(SignalATimeout);
                        SolverTimer.Change((long)remainingTime.TotalMilliseconds, Timeout.Infinite);
                    }
                }
            }

            Run();
        }

        public void Run()
        {
            if (IgnoreTimer)
            {
                /* Run solver in the same thread: */
                if (Solver != null)
                {
                    Solver.Solve();
                }
            }
            else
            {
                /* Run solver in a separate thread: */
                SolverThread = new Thread(RunTheSolver);
                SolverThread.Start();
                try
                {
                    /* Wait for solver to finish running, or for a timeout to occur: */
                    OutputTriggeringEvent.WaitOne();
                }
                finally
                {
                    // Stop the timer if it's still running:
                    lock (solverTimerLock)
                    {
                        SolverTimer.Change(-1, -1);
                        SolverTimer = null;
                    }

                    // Stop the solver algorithm:
                    Solver.Stop();
                    try
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(MAX_GRACE_PERIOD_TO_STOP_IN));  // Wait 0.1 seconds before aborting

                        if (Solver.SolverState != SolverState.NotRunning)
                        {
                            /* Stop the solver thread if it's still running: */
                            if (SolverThread != null && SolverThread.IsAlive)
                            {
                                SolverThread.Abort();
                            }
                        }
                    }
                    finally
                    {
                        SolverThread = null;
                    }
                }
            }
        }

        /// <summary>
        /// The caller is responsible for ensuring that SaveGameState() is called within a lock(bestMoveLock) block.
        /// </summary>
        public void SaveGameState()
        {
            /* Ensure there is a best move: */
            if (BestMoveSoFar == null)
            {
                CellState anyPossibleMove = CurrentGameState.YourCell.GetAdjacentCellStates().Where(
                    cs => cs.OccupationStatus == OccupationStatus.Clear).FirstOrDefault();
                if (anyPossibleMove != null)
                {
                    GameState bestMoveSoFar = CurrentGameState.Clone();
                    bestMoveSoFar.MoveToPosition(anyPossibleMove.Position);
                    BestMoveSoFar = bestMoveSoFar;
                }
            }

            /* BestMoveSoFar could be null if we have just lost the game: */
            if (BestMoveSoFar != null)
            {
                BestMoveSoFar.SaveGameState(BinaryGameStateFilePath, FileType.Binary);

                if (IsInDebugMode)
                {
                    BestMoveSoFar.SaveGameState(XmlGameStateFilePath, FileType.Xml);
                    BestMoveSoFar.CheckThatGameStateIsValid(CurrentGameState);
                }
            }
        }

        public void ArchiveGameStateOfPreviousGame(GameState newState)
        {
            if (IsInDebugMode && LastGameStateOfPreviousGame != null)
            {
                /* Save the previous game state, as it was the end of the last game: */
                Guid gameGuid = Guid.NewGuid();
                string fileName = String.Format("GameState.{0}.ext", gameGuid);
                string historyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string historyFolderName = String.Format("{0}History", Solver.GetType().Name);
                historyFolder = Path.Combine(historyFolder, historyFolderName);

                if (!Directory.Exists(historyFolder))
                {
                    Directory.CreateDirectory(historyFolder);
                }

                string BinaryGameStateFilePath = Path.ChangeExtension(Path.Combine(historyFolder, fileName), ".bin");
                string XmlGameStateFilePath = Path.ChangeExtension(Path.Combine(historyFolder, fileName), ".xml");
                LastGameStateOfPreviousGame.SaveGameState(BinaryGameStateFilePath, FileType.Binary);
                LastGameStateOfPreviousGame.SaveGameState(XmlGameStateFilePath, FileType.Xml);
            }
        }
    }
}
