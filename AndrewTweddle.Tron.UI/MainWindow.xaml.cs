using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Microsoft.Win32;
using AndrewTweddle.Tron.Core;
using System.Reflection;

namespace AndrewTweddle.Tron.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel mainViewModel;

        MainViewModel MainViewModel
        {
            get
            {
                return mainViewModel;
            }
            set
            {
                mainViewModel = value;
                DataContext = value;
                MainGameStateView.ViewModel = value.GameStateViewModel;
                SearchTreeView.ViewModel = value.SearchTreeViewModel;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            MainViewModel = new MainViewModel();
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.StartGame();
            TakeNextTurnInASeparateThread();
        }

        private void TakeNextTurnInASeparateThread()
        {
            BackgroundWorker backgroundWorker = (BackgroundWorker)this.FindResource("takeTurnBackgroundWorker");
            backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                MainViewModel.TakeNextTurn();
            }
            catch (Exception exc)
            {
                // Just to create somewhere to put a breakpoint:
                throw exc;
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            MainViewModel.DisplaySearchTree();

            if (MainViewModel.GameStateViewModel.GameState.IsGameOver)
            {
                // TODO: Indicate who won
            }
            else
            {
                if (MainViewModel.IsInProgress && !MainViewModel.IsPaused)
                {
                    TakeNextTurnInASeparateThread();
                }
            }
        }

        private void StopGameButton_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.StopGame();
        }

        private void PauseGameButton_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.Pause();
        }

        private void ResumeGameButton_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.Resume();

            /* Only take the next turn if the current turn is not still in progress 
             * (since Pause & Resume could be pressed within the duration of a single turn:
             */
            if (!MainViewModel.IsTurnInProgress)
            {
                TakeNextTurnInASeparateThread();
            }
        }

        private void StepGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(MainViewModel.IsTurnInProgress || MainViewModel.GameStateViewModel.GameState.IsGameOver))
            {
                TakeNextTurnInASeparateThread();
            }
        }

        private void LoadGameButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "xml game state files (*.xml)|*.xml|binary game state files (*.bin)|*.bin",
                InitialDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase),
                CheckFileExists = true
            };
            if (dialog.ShowDialog() == true)
            {
                MainViewModel.LoadedFilePath = dialog.FileName;
                MainViewModel.LoadedFileType = (System.IO.Path.GetExtension(dialog.FileName) == ".xml") ? FileType.Xml : FileType.Binary;
                MainViewModel.LoadGameState();
            }
        }

        private void ReloadGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(MainViewModel.LoadedFilePath))
            {
                MainViewModel.LoadGameState();
            }
        }

        private void SaveGameButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                DefaultExt = ".xml",
                Filter = "xml game state files (*.xml)|*.xml|binary game state files (*.bin)|*.bin",
                FileName = String.Format("{0}.xml", DateTime.Now.ToString("yyyy-MM-dd_HHmmss")),
                InitialDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase),
                OverwritePrompt = true,
                AddExtension = true
            };
            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                FileType fileType = (System.IO.Path.GetExtension(filePath) == ".xml") ? FileType.Xml : FileType.Binary;
                MainViewModel.SaveGameState(filePath, fileType);
            }
        }

        private void GoBackToMoveNumberButton_Click(object sender, RoutedEventArgs e)
        {
            int moveNumber;
            bool validMoveNumber = int.TryParse(moveNumberTextBox.Text, out moveNumber);
            if (validMoveNumber)
            {
                validMoveNumber = moveNumber > 0;
            }
            if (!validMoveNumber)
            {
                MessageBox.Show("Please specify a valid move number", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // TODO: Check that move number is not in the future

            PlayerType playerWhoMovedLast;
            bool validPlayer = Enum.TryParse(PlayerWhoMovedLastComboBox.Text, out playerWhoMovedLast);
            if (!validPlayer)
            {
                MessageBox.Show("Please specify which player should be the one who moved last", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MainViewModel.GoBackToMoveNumber(playerWhoMovedLast, moveNumber);
        }

        private void UndoMoveButton_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.UndoLastMove();
        }
    }
}
