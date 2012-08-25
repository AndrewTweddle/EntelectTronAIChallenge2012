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
            MainViewModel.TakeNextTurn();
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
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
    }
}
