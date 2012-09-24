using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using AndrewTweddle.Tron.Core;

namespace AndrewTweddle.Tron.Console.TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                System.Console.WriteLine("Usage: Player1FolderPath Player2FolderPath emptyTronStateFilePath repetitions[=optional,int]");
            }
            string player1FolderPath = args[0];
            string player2FolderPath = args[1];
            string emptyTronStateFilePath = args[2];
            int repetitions = 1;
            if (args.Length < 4 || !int.TryParse(args[3], out repetitions))
            {
                repetitions = 1;
            }

            for (int i = 0; i < repetitions; i++)
            {
                // Alternate start player:
                string startPlayerFolderPath;
                string secondPlayerFolderPath;

                if (i % 2 == 0)
                {
                    startPlayerFolderPath = player1FolderPath;
                    secondPlayerFolderPath = player2FolderPath;
                }
                else
                {
                    startPlayerFolderPath = player2FolderPath;
                    secondPlayerFolderPath = player1FolderPath;
                }

                string startPlayerBatFilePath = Path.Combine(startPlayerFolderPath, "start.bat");
                string startPlayerTronGameFilePath = Path.Combine(startPlayerFolderPath, "game.state");

                string secondPlayerBatFilePath = Path.Combine(secondPlayerFolderPath, "start.bat");
                string secondPlayerTronGameFilePath = Path.Combine(secondPlayerFolderPath, "game.state");

                File.Copy(emptyTronStateFilePath, startPlayerTronGameFilePath, overwrite:true);

                IEnumerable<RawCellData> cells = GameState.LoadRawCellDataFromTronGameFile(startPlayerTronGameFilePath);
                GameState currentGameState = new GameState();
                currentGameState.LoadRawCellData(cells);
                bool isFirstPlayer = false;

                while (!currentGameState.IsGameOver)
                {
                    isFirstPlayer = !isFirstPlayer;
                    if (isFirstPlayer)
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo(startPlayerBatFilePath, startPlayerTronGameFilePath);
                        startInfo.WorkingDirectory = startPlayerFolderPath;
                        startInfo.CreateNoWindow = true;
                        startInfo.RedirectStandardInput = true;
                        startInfo.RedirectStandardError = true;
                        startInfo.RedirectStandardOutput = true;
                        Process process = Process.Start(startInfo);
                        process.WaitForExit();
                        TimeSpan duration = process.ExitTime - process.StartTime;
                        System.Console.WriteLine("First player took {0}", duration);
                        if (duration > TimeSpan.FromSeconds(5))
                        {
                            System.Console.Error.WriteLine("Exceeded 5 seconds!");
                        }
                        cells = GameState.LoadRawCellDataFromTronGameFile(startPlayerTronGameFilePath);
                        currentGameState = new GameState();
                        currentGameState.LoadRawCellData(cells, PlayerType.Opponent);
                        currentGameState.FlipGameState();
                        currentGameState.SaveTronGameFile(secondPlayerTronGameFilePath);
                    }
                    else
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo(secondPlayerBatFilePath, secondPlayerTronGameFilePath);
                        startInfo.WorkingDirectory = secondPlayerFolderPath;
                        startInfo.CreateNoWindow = true;
                        startInfo.RedirectStandardInput = true;
                        startInfo.RedirectStandardError = true;
                        startInfo.RedirectStandardOutput = true;
                        Process process = Process.Start(startInfo);
                        process.WaitForExit();
                        TimeSpan duration = process.ExitTime - process.StartTime;
                        System.Console.WriteLine("Second player took {0}", duration);
                        if (duration > TimeSpan.FromSeconds(5))
                        {
                            System.Console.Error.WriteLine("Exceeded 5 seconds!");
                        }
                        cells = GameState.LoadRawCellDataFromTronGameFile(secondPlayerTronGameFilePath);
                        currentGameState = new GameState();
                        currentGameState.LoadRawCellData(cells, PlayerType.Opponent);
                        currentGameState.FlipGameState();
                        currentGameState.SaveTronGameFile(startPlayerTronGameFilePath);
                    }
                }
            }

        }
    }
}
