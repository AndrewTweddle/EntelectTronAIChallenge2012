using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;
using AndrewTweddle.Tron.Bots;
using AndrewTweddle.Tron.UI.GameBoard;

namespace AndrewTweddle.Tron.UI
{
    public class MainViewModel: BaseViewModel
    {
        private GameStateViewModel gameStateViewModel;
        private ISolver player1Solver;
        private ISolver player2Solver;
        private Coordinator player1Coordinator;
        private Coordinator player2Coordinator;
        private bool isTurnOfPlayer1 = true;
        private bool isPaused = false;
        private bool isInProgress = false;
        private bool isTurnInProgress = false;

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
            GameStateViewModel = new GameStateViewModel();
            GameStateViewModel.GameState = new GameState();
        }

        public void StartGame()
        {
            IsTurnOfPlayer1 = true;
            IsInProgress = true;
            IsPaused = false;

            GameState gameState = GameState.InitializeNewGameState();
            GameStateViewModel.GameState.CopyFrom(gameState);
            
            /* Set up player 1 (red): */
            Player1Solver = new RandomSolver();
            Player1Coordinator = new Coordinator(Player1Solver)
            {
                IsInDebugMode = true,
                IgnoreTimer = true
            };

            /* Set up player 2 (blue): */
            Player2Solver = new NegaMaxSolver();
            Player2Coordinator = new Coordinator(Player2Solver)
            {
                IsInDebugMode = true,
                IgnoreTimer = true
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
                }
                else
                {
                    coordinator = Player2Coordinator;
                }
                coordinator.CurrentGameState = GameStateViewModel.GameState.Clone();
                if (!IsTurnOfPlayer1)
                {
                    coordinator.CurrentGameState.FlipGameState();
                }

                // Run following in BackgroundWorkerThread:
                coordinator.Run();
                lock (coordinator.BestMoveLock)
                {
                    GameState newGameState = coordinator.BestMoveSoFar;
                    if (!isTurnOfPlayer1)
                    {
                        newGameState.FlipGameState();
                    }
                    GameStateViewModel.GameState.CopyFrom(newGameState);
                    if (newGameState.IsGameOver)
                    {
                        StopGame();
                    }
                    else
                    {
                        IsTurnOfPlayer1 = !IsTurnOfPlayer1;
                    }
                }
            }
            finally
            {
                IsTurnInProgress = false;
            }
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
    }
}
