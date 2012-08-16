using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TronSdk;
using AndrewTweddle.Tron.Core;
using System.Reflection;
using System.IO;

namespace AndrewTweddle.Tron.BotAdapter
{
    public abstract class BaseBotAdapter: ITronBot
    {
        public abstract GameState GenerateNextGameState(GameState gameState);

        public GameState PreviousGameState { get; set; }

        public void ExecuteMove(ref BlockTypes[,] grid)
        {
            string exeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string binaryGameStateFilePath = Path.Combine(exeFolder, "GameState.bin");
            string xmlGameStateFilePath = Path.Combine(exeFolder, "GameState.xml");

            /* Determine previous game state (this will be saved when a new game starts): */
            if (File.Exists(binaryGameStateFilePath))
            {
                PreviousGameState = GameState.LoadGameState(binaryGameStateFilePath, FileType.Binary);
            }
            else
                if (File.Exists(xmlGameStateFilePath))
                {
                    PreviousGameState = GameState.LoadGameState(xmlGameStateFilePath, FileType.Xml);
                }
                else
                {
                    PreviousGameState = null;
                }

            /* Backup existing files: */
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

            /* Run game: */
            GameState currentGameState = PreviousGameState == null ? new GameState() : PreviousGameState.Clone();
            currentGameState.NewGameDetected += OnNewGameDetected;
            try
            {
                IEnumerable<RawCellData> cells = ConvertGridToRawCellData(grid);
                currentGameState.LoadRawCellData(cells);

                /* Save new game state: */
                GameState newGameState = GenerateNextGameState(currentGameState);
                newGameState.SaveGameState(binaryGameStateFilePath, FileType.Binary);
                newGameState.SaveGameState(xmlGameStateFilePath, FileType.Xml);

                /* Communicate decision back to the marshall: */
                UpdateGridWithNewGameState(ref grid, newGameState);
            }
            finally
            {
                /* Ensure no memory leaks due to dangling events: */
                currentGameState.NewGameDetected -= OnNewGameDetected;
            }
        }

        public void OnNewGameDetected(GameState newState)
        {
            if (PreviousGameState != null)
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
                PreviousGameState.SaveGameState(binaryGameStateFilePath, FileType.Binary);
                PreviousGameState.SaveGameState(xmlGameStateFilePath, FileType.Xml);
            }
        }

        private IEnumerable<RawCellData> ConvertGridToRawCellData(BlockTypes[,] grid)
        {
            for (int x = 0; x < Constants.Columns; x++)
            {
                for (int y = 0; y < Constants.Rows; y++)
                {
                    BlockTypes blockType = grid[x, y];
                    OccupationStatus occupationStatus = (OccupationStatus)Enum.Parse(typeof(OccupationStatus), blockType.ToString()); 
                    yield return new RawCellData
                    {
                        X = x,
                        Y = y,
                        OccupationStatus = occupationStatus
                    };
                }
            }
        }

        private void UpdateGridWithNewGameState(ref BlockTypes[,] grid, GameState newGameState)
        {
            for (int x = 0; x < Constants.Columns; x++)
            {
                for (int y = 0; y < Constants.Rows; y++)
                {
                    CellState cellState = newGameState[x, y];
                    BlockTypes blockType = (BlockTypes)Enum.Parse(typeof(BlockTypes), cellState.OccupationStatus.ToString());
                    grid[x, y] = blockType;
                }
            }
        }
    }
}
