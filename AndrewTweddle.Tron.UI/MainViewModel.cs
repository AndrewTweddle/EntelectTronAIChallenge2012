using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;
using AndrewTweddle.Tron.Bots;
using AndrewTweddle.Tron.UI.GameBoard;

namespace AndrewTweddle.Tron.UI
{
    public class MainViewModel
    {
        private GameStateViewModel gameStateViewModel = new GameStateViewModel();

        public GameStateViewModel GameStateViewModel
        {
            get
            {
                return gameStateViewModel;
            }
        }

        public void StartGame()
        {
            GameState gameState = GameState.InitializeNewGameState();
            GameStateViewModel.GameState = gameState;
            
            /* Set up player 1 (red): */
            ISolver randomSolver = new RandomSolver();
            Coordinator redPlayerCoordinator = new Coordinator(randomSolver)
            {
                IsInDebugMode = true,
                IgnoreTimer = true
            };

            /* Set up player 2 (red): */
            ISolver negaMaxSolver = new NegaMaxSolver();
            Coordinator bluePlayerCoordinator = new Coordinator(negaMaxSolver)
            {
                IsInDebugMode = true,
                IgnoreTimer = true
            };

            Coordinator[] coordinators = new Coordinator[2] { redPlayerCoordinator, bluePlayerCoordinator };
            int nextPlayerIndex = 0;

            while (gameState.GetPossibleNextMoves().Any())
            {
                Coordinator coordinator = coordinators[nextPlayerIndex];
                coordinator.CurrentGameState = gameState;
                coordinator.Run();
                gameState = coordinator.BestMoveSoFar;
                nextPlayerIndex = 1 - nextPlayerIndex;
                gameState.FlipGameState();
                GameStateViewModel.GameState = gameState;

                // TODO: If in step-through mode, pause here
                // TODO: If aborted, exit here
            }
            // TODO: Show who the winner was, update stats, etc.
        }
    }
}
