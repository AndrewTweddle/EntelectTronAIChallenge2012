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
        private object bestMoveLock = new object();

        public bool IsInDebugMode { get; set; }
        public GameState LastGameStateOfPreviousGame { get; set; }
        public GameState BestMoveSoFar { get; private set; }  // TODO: Replace with Move not GameState
        public ISolver Solver { get; set; }
        public DateTime StartTime { get; private set; }
        public TimeSpan MaximumDuration { get; private set; }
        public Thread SolverThread { get; private set; }
        public ManualResetEvent OutputTriggeringEvent { get; private set; }

        private Coordinator()
        {
        }

        public Coordinator(ISolver solver)
        {
            Solver = solver;
            Process process = Process.GetCurrentProcess();
            StartTime = process.StartTime;
            DateTime now = DateTime.Now;
            if (StartTime + MaximumDuration < now)
            {
                // Process start time has probably not been set, so guess a start time of 1 second ago:
                StartTime = now.Subtract(TimeSpan.FromSeconds(1));
            }
            MaximumDuration = TimeSpan.FromMilliseconds(4500);   // 4.5 seconds
        }

        public void SetBestMoveSoFar(GameState bestMove)
        {
            lock (bestMoveLock)
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
            if (Solver != null)
            {
                Solver.Solve();
            }

            /* Signal that the solver has run: */
            OutputTriggeringEvent.Set();
        }

        public void Run(string tronGameFilePath)
        {
            IEnumerable<RawCellData> cells = GameState.LoadRawCellDataFromTronGameFile(tronGameFilePath);
            Run(cells);
            lock (bestMoveLock)
            {
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
            System.Threading.Timer timer = new System.Threading.Timer(SignalATimeout);
            DateTime endTime = StartTime + MaximumDuration;
            DateTime now = DateTime.Now;
            TimeSpan remainingTime = now - endTime;
            timer.Change((long)remainingTime.TotalMilliseconds, Timeout.Infinite);

            /* Start the solver running: */
            string exeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string binaryGameStateFilePath = Path.Combine(exeFolder, "GameState.bin");
            string xmlGameStateFilePath = Path.Combine(exeFolder, "GameState.xml");

            /* Determine previous game state (this will be saved when a new game starts): */
            if (File.Exists(binaryGameStateFilePath))
            {
                LastGameStateOfPreviousGame = GameState.LoadGameState(binaryGameStateFilePath, FileType.Binary);
            }
            else
                if (File.Exists(xmlGameStateFilePath))
                {
                    LastGameStateOfPreviousGame = GameState.LoadGameState(xmlGameStateFilePath, FileType.Xml);
                }
                else
                {
                    LastGameStateOfPreviousGame = null;
                }

            /* Backup existing files: */
            if (IsInDebugMode)
            {
                if (File.Exists(binaryGameStateFilePath))
                {
                    string backupFileName = Path.ChangeExtension(binaryGameStateFilePath, ".bak.bin");
                    File.Copy(binaryGameStateFilePath, backupFileName, true /*overWrite*/);
                }

                if (File.Exists(xmlGameStateFilePath))
                {
                    string backupFileName = Path.ChangeExtension(xmlGameStateFilePath, ".bak.xml");
                    File.Copy(xmlGameStateFilePath, backupFileName, true /*overWrite*/);
                }
            }

            /* Run the solver: */
            GameState currentGameState = LastGameStateOfPreviousGame == null ? new GameState() : LastGameStateOfPreviousGame.Clone();
            currentGameState.NewGameDetected += ArchiveGameStateOfPreviousGame;
            try
            {
                currentGameState.LoadRawCellData(cells);

                /* Run solver in a separate thread: */
                SolverThread = new Thread(RunTheSolver);
                SolverThread.Start();

                /* Wait for solver to finish running, or for a timeout to occur: */
                OutputTriggeringEvent.WaitOne();

                /* Stop the solver thread if it's still running: */
                if (SolverThread.IsAlive)
                {
                    SolverThread.Abort();
                }
                
                /* Save new game state: */
                lock (bestMoveLock)
                {
                    if (BestMoveSoFar == null)
                    {
                        /* We are out of time. Choose any move: */
                        CellState anyPossibleMove = currentGameState.YourCell.GetAdjacentCellStates().FirstOrDefault();
                        if (anyPossibleMove != null)
                        {
                            GameState bestMoveSoFar = currentGameState.Clone();
                            bestMoveSoFar.YourCell.OccupationStatus = OccupationStatus.YourWall;
                            bestMoveSoFar[anyPossibleMove.Position].OccupationStatus = OccupationStatus.You;
                            BestMoveSoFar = bestMoveSoFar;
                        }
                    }

                    /* BestMoveSoFar could still be null if we have just lost the game: */
                    if (BestMoveSoFar != null)
                    {
                        BestMoveSoFar.SaveGameState(binaryGameStateFilePath, FileType.Binary);

                        if (IsInDebugMode)
                        {
                            BestMoveSoFar.SaveGameState(xmlGameStateFilePath, FileType.Xml);
                        }
                    }
                }
            }
            finally
            {
                /* Ensure no memory leaks due to dangling events: */
                currentGameState.NewGameDetected -= ArchiveGameStateOfPreviousGame;
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
                string historyFolderName = String.Format("{0}History", this.GetType().Name);
                historyFolder = Path.Combine(historyFolder, historyFolderName);

                if (!Directory.Exists(historyFolder))
                {
                    Directory.CreateDirectory(historyFolder);
                }

                string binaryGameStateFilePath = Path.ChangeExtension(Path.Combine(historyFolder, fileName), ".bin");
                string xmlGameStateFilePath = Path.ChangeExtension(Path.Combine(historyFolder, fileName), ".xml");
                LastGameStateOfPreviousGame.SaveGameState(binaryGameStateFilePath, FileType.Binary);
                LastGameStateOfPreviousGame.SaveGameState(xmlGameStateFilePath, FileType.Xml);
            }
        }
    }
}
