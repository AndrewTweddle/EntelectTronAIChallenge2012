using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;
using AndrewTweddle.Tron.Bots;
using AndrewTweddle.Tron.UI.GameBoard;
using System.Collections.ObjectModel;
using AndrewTweddle.Tron.UI.SearchTree;

namespace AndrewTweddle.Tron.UI
{
    public class MainViewModel: BaseViewModel
    {
        private GameStateViewModel gameStateViewModel;
        private SearchTreeViewModel searchTreeViewModel;
        private ISolver player1Solver;
        private ISolver player2Solver;
        private Coordinator player1Coordinator;
        private Coordinator player2Coordinator;
        private bool isPlayer1Human;
        private bool isPlayer2Human;
        private bool isTurnOfPlayer1 = true;
        private bool isPaused = false;
        private bool isInProgress = false;
        private bool isTurnInProgress = false;

        private ObservableCollection<Type> solverTypes;
        private Type player1SolverType;
        private Type player2SolverType;

        public ObservableCollection<Type> SolverTypes
        {
            get
            {
                return solverTypes;
            }
        }

        public Type Player1SolverType
        {
            get
            {
                return player1SolverType;
            }
            set
            {
                player1SolverType = value;
                OnPropertyChanged("Player1SolverType");
            }
        }

        public Type Player2SolverType
        {
            get
            {
                return player2SolverType;
            }
            set
            {
                player2SolverType = value;
                OnPropertyChanged("Player2SolverType");
            }
        }

        public bool IsPlayer1Human
        {
            get
            {
                return isPlayer1Human;
            }
            set
            {
                isPlayer1Human = value;
                OnPropertyChanged("IsPlayer1Human");
            }
        }

        public bool IsPlayer2Human
        {
            get
            {
                return isPlayer2Human;
            }
            set
            {
                isPlayer2Human = value;
                OnPropertyChanged("IsPlayer2Human");
            }
        }

        public bool IsPaused
        {
            get
            {
                return isPaused;
            }
            set
            {
                if (isPaused != value)
                {
                    isPaused = value;
                    OnPropertyChanged("IsPaused");
                }
            }
        }

        public bool IsInProgress
        {
            get
            {
                return isInProgress;
            }
            set
            {
                if (isInProgress != value)
                {
                    isInProgress = value;
                    OnPropertyChanged("IsInProgress");
                }
            }
        }

        public bool IsTurnInProgress
        {
            get
            {
                return isTurnInProgress;
            }
            set
            {
                if (isTurnInProgress != value)
                {
                    isTurnInProgress = value;
                    OnPropertyChanged("IsTurnInProgress");
                }
            }
        }

        public GameStateViewModel GameStateViewModel
        {
            get
            {
                return gameStateViewModel;
            }
            set
            {
                gameStateViewModel = value;
                OnPropertyChanged("GameStateViewModel");
            }
        }

        public SearchTreeViewModel SearchTreeViewModel
        {
            get
            {
                return searchTreeViewModel;
            }
            set
            {
                searchTreeViewModel = value;
                OnPropertyChanged("SearchTreeViewModel");
            }
        }

        public bool IsTurnOfPlayer1
        {
            get
            {
                return isTurnOfPlayer1;
            }
            set
            {
                if (isTurnOfPlayer1 != value)
                {
                    isTurnOfPlayer1 = value;
                    OnPropertyChanged("IsTurnOfPlayer1");
                }
            }
        }

        public ISolver Player1Solver
        {
            get
            {
                return player1Solver;
            }
            set
            {
                if (player1Solver != value)
                {
                    player1Solver = value;
                    OnPropertyChanged("Player1Solver");
                }
            }
        }

        public ISolver Player2Solver
        {
            get
            {
                return player2Solver;
            }
            set
            {
                if (player2Solver != value)
                {
                    player2Solver = value;
                    OnPropertyChanged("Player2Solver");
                }
            }
        }

        public Coordinator Player1Coordinator
        {
            get
            {
                return player1Coordinator;
            }
            set
            {
                if (player1Coordinator != value)
                {
                    player1Coordinator = value;
                    OnPropertyChanged("Player1Coordinator");
                }
            }
        }

        public Coordinator Player2Coordinator
        {
            get
            {
                return player2Coordinator;
            }
            set
            {
                if (player2Coordinator != value)
                {
                    player2Coordinator = value;
                    OnPropertyChanged("Player2Coordinator");
                }
            }
        }

        public MainViewModel()
        {
            solverTypes = new ObservableCollection<Type>();
            solverTypes.Add(typeof(NegaMaxSolver));
            solverTypes.Add(typeof(RandomSolver));
            solverTypes.Add(typeof(HumanSolver));

            GameStateViewModel = new GameStateViewModel();
            GameStateViewModel.GameState = new GameState();

            SearchTreeViewModel = new SearchTreeViewModel();
        }

        public void StartGame()
        {
            GameState gameState = GameState.InitializeNewGameState();
            GameStateViewModel.GameState.CopyFrom(gameState);
            IsTurnOfPlayer1 = true;
            ContinueWithANewOrLoadedGame();
            IsPaused = false;
        }

        private void ContinueWithANewOrLoadedGame()
        {
            IsInProgress = true;

            /* Set up player 1 (red): */
            Player1Solver = Activator.CreateInstance(Player1SolverType) as ISolver;
            IsPlayer1Human = (Player1Solver is HumanSolver);
            Player1Coordinator = new Coordinator(Player1Solver)
            {
                IsInDebugMode = true,
                IgnoreTimer = IsPlayer1Human
            };

            /* Set up player 2 (blue): */
            Player2Solver = Activator.CreateInstance(Player2SolverType) as ISolver;
            IsPlayer2Human = (Player2Solver is HumanSolver);
            Player2Coordinator = new Coordinator(Player2Solver)
            {
                IsInDebugMode = true,
                IgnoreTimer = IsPlayer2Human
            };
        }

        public void TakeNextTurn()
        {
            if (IsTurnInProgress)
            {
                throw new InvalidOperationException(
                    "A new turn cannot be started if a current turn is still in progress");
            }
            IsTurnInProgress = true;
            try
            {
                Coordinator coordinator;
                if (IsTurnOfPlayer1)
                {
                    coordinator = Player1Coordinator;
                    if (IsPlayer1Human)
                    {
                        GameStateViewModel.SelectedCellActivated += GameStateViewModel_Player1SelectedCellActivated;
                    }
                }
                else
                {
                    coordinator = Player2Coordinator;
                    if (IsPlayer2Human)
                    {
                        GameStateViewModel.SelectedCellActivated += GameStateViewModel_Player2SelectedCellActivated;
                    }
                }
                coordinator.CurrentGameState = GameStateViewModel.GameState.Clone();
                if (!IsTurnOfPlayer1)
                {
                    coordinator.CurrentGameState.FlipGameState();
                }

                try
                {
                    coordinator.StartTime = DateTime.Now;
                    coordinator.Run();
                }
                catch(Exception exc)
                {
                    System.Diagnostics.Debug.WriteLine(exc);
                }
                lock (coordinator.BestMoveLock)
                {
                    GameState newGameState = coordinator.BestMoveSoFar;
                    if (newGameState == null)
                    {
                        throw new ApplicationException("The solver did not choose a move");
                    }
                    if (IsTurnOfPlayer1)
                    {
                        if (IsPlayer1Human)
                        {
                            GameStateViewModel.SelectedCellActivated -= GameStateViewModel_Player1SelectedCellActivated;
                        }
                    }
                    else
                    {
                        newGameState.FlipGameState();
                        if (IsPlayer2Human)
                        {
                            GameStateViewModel.SelectedCellActivated -= GameStateViewModel_Player2SelectedCellActivated;
                        }
                    }
                    GameStateViewModel.GameState.CopyFrom(newGameState);
                    IsTurnOfPlayer1 = !IsTurnOfPlayer1;
                    if (newGameState.IsGameOver)
                    {
                        StopGame();
                    }
                }
            }
            finally
            {
                IsTurnInProgress = false;
            }
        }

        void GameStateViewModel_Player1SelectedCellActivated(object sender, EventArgs e)
        {
            (Player1Solver as HumanSolver).PositionMovedTo = GameStateViewModel.SelectedCellStateViewModel.CellState.Position;
        }

        void GameStateViewModel_Player2SelectedCellActivated(object sender, EventArgs e)
        {
            (Player2Solver as HumanSolver).PositionMovedTo = GameStateViewModel.SelectedCellStateViewModel.CellState.Position;
        }

        public void StopGame()
        {
            IsInProgress = false;
            IsPaused = false;
        }

        public void Pause()
        {
            IsPaused = true;
        }

        public void Resume()
        {
            IsPaused = false;
        }

        public void LoadGameState(string filePath, FileType fileType = FileType.Binary)
        {
            GameState loadedGameState = GameState.LoadGameState(filePath, fileType);
            GameStateViewModel.GameState.CopyFrom(loadedGameState);
            IsTurnOfPlayer1 = loadedGameState.PlayerToMoveNext == PlayerType.You;
            IsPaused = true;
            ContinueWithANewOrLoadedGame();
        }

        public void SaveGameState(string filePath, FileType fileType = FileType.Binary)
        {
            GameStateViewModel.GameState.SaveGameState(filePath, fileType);
        }

        public void DisplaySearchTree()
        {
            if (!IsTurnOfPlayer1)
            {
                if (Player1Solver is BaseNegaMaxSolver)
                {
                    BaseNegaMaxSolver nega = (BaseNegaMaxSolver)Player1Solver;
                    SearchTreeViewModel.RootNode = nega.RootNode;
                }
            }
            else
            {
                if (Player2Solver is BaseNegaMaxSolver)
                {
                    BaseNegaMaxSolver nega = (BaseNegaMaxSolver)Player2Solver;
                    SearchTreeViewModel.RootNode = nega.RootNode;
                }
            }
        }
    }
}
