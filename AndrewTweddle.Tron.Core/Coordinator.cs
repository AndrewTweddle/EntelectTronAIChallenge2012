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
        public static readonly int MAX_DURATION_IN_MILLISECONDS = 5000;
        public static readonly int TIME_TO_WAIT_UNTIL_STOPPING_SOLVER_IN_MILLISECONDS = 4000;
        public static readonly int MAX_GRACE_PERIOD_TO_STOP_IN = 500;

        public bool IsInDebugMode { get; set; }
        public bool IgnoreTimer { get; set; }
        public object BestMoveLock { get; private set; }
        public GameState LastGameStateOfPreviousGame { get; set; }
        public GameState CurrentGameState { get; set; }
        public GameState BestMoveSoFar { get; private set; }  // TODO: Replace with Move not GameState?
        public ISolver Solver { get; set; }
        public DateTime StartTime { get; set; }
        public Thread SolverThread { get; private set; }
        public ManualResetEvent OutputTriggeringEvent { get; private set; }
        public string BinaryGameStateFilePath { get; private set; }
        public string XmlGameStateFilePath { get; private set; }

        private Coordinator()
        {
        }

        public Coordinator(ISolver solver, DateTime startTime)
        {
            BestMoveLock = new object();
            solver.Coordinator = this;
            Solver = solver;
        }

        public Coordinator(ISolver solver): this(solver, DateTime.Now)
        {
        }

        public void SetBestMoveSoFar(GameState bestMove)
        {
            lock (BestMoveLock)
            {
                BestMoveSoFar = bestMove;
            }
        }

        /// <summary>
        /// This method is used to run the solver in a separate thread
        /// </summary>
        private void RunTheSolver()
        {
            /* Save the property value in a local variable, so that if the property is later changed, 
             * this method won't affect the newly created ManualResetEvent:
             */
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
                if (triggerOutputEvent != null)
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
            if (StartTime + TimeSpan.FromMilliseconds(MAX_DURATION_IN_MILLISECONDS) < now)
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

        /// <summary>
        /// The StartTime property should be set before calling Run() (unless IgnoreTimer is set to true).
        /// </summary>
        public void Run(IEnumerable<RawCellData> cells)
        {
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
            StartTime = DateTime.Now;
            CurrentGameState = currentGameState;
            Run();
        }

        /// <summary>
        /// The StartTime property should be set before calling Run() (unless IgnoreTimer is set to true).
        /// The CurrentGameState must also be set.
        /// </summary>
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
                /* Create an event which can be triggered to signal that a move has been chosen by the solver: */
                if (OutputTriggeringEvent == null)
                {
                    OutputTriggeringEvent = new ManualResetEvent(false /* Not signalled yet*/);
                }
                else
                {
                    OutputTriggeringEvent.Reset();
                }

                /* Run solver in a separate thread: */
                SolverThread = new Thread(RunTheSolver);
                SolverThread.Start();
                try
                {
                    /* Wait for solver to finish running, or for a timeout to occur: */
                    DateTime timeAtWhichToStopSolver = StartTime.AddMilliseconds(TIME_TO_WAIT_UNTIL_STOPPING_SOLVER_IN_MILLISECONDS);
                    TimeSpan remainingTime = timeAtWhichToStopSolver - DateTime.Now;
                    TimeSpan minimumTimeToProvide = TimeSpan.FromMilliseconds(100);  // TODO: remove hard-coding of 0.1 seconds to make a move
                    if (remainingTime <= minimumTimeToProvide)
                    {
                        remainingTime = minimumTimeToProvide;
                    }

                    /* Wait for the solver to finish running: */
                    OutputTriggeringEvent.WaitOne(remainingTime);

                    /* Stop the solver algorithm: */
                    Solver.Stop();
                }
                finally
                {
                    SolverThread = null;
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
