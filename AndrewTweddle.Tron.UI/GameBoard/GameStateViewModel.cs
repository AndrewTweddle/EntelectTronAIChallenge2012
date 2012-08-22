using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace AndrewTweddle.Tron.UI.GameBoard
{
    public class GameStateViewModel:BaseViewModel
    {
        private GameState gameState;
        private CellStateViewModel selectedCellStateViewModel;
        private ObservableCollection<CellStateViewModel> cellStateViewModels;

        public GameState GameState
        {
            get
            {
                return gameState;
            }
            set
            {
                gameState = value;
                OnPropertyChanged("GameState");
                if (gameState == null)
                {
                    if (CellStateViewModels != null)
                    {
                        CellStateViewModels = null;
                    }
                }
                else
                {
                    List<CellState> newCellStates = gameState.GetAllCellStates().OrderBy(cs => cs.Position.Y).ThenBy(cs => cs.Position.X).ToList();
                    if (CellStateViewModels == null)
                    {
                        ObservableCollection<CellStateViewModel> newCellStateViewModels = new ObservableCollection<CellStateViewModel>();
                        foreach (CellState cellState in newCellStates)
                        {
                            CellStateViewModel cellStateViewModel = new CellStateViewModel(this)
                            {
                                CellState = cellState
                            };
                            newCellStateViewModels.Add(cellStateViewModel);
                        }
                        CellStateViewModels = newCellStateViewModels;
                    }
                    else
                    {
                        int index = 0;
                        foreach (CellStateViewModel csvm in CellStateViewModels)
                        {
                            csvm.CellState = newCellStates[index];
                            index++;
                        }
                    }
                }
            }
        }

        public ObservableCollection<CellStateViewModel> CellStateViewModels
        {
            get
            {
                return cellStateViewModels;
            }
            private set
            {
                if (cellStateViewModels != value)
                {
                    cellStateViewModels = value;
                    OnPropertyChanged("CellStateViewModels");
                }
            }
        }

        public CellStateViewModel SelectedCellStateViewModel
        {
            get
            {
                return selectedCellStateViewModel;
            }
            set
            {
                selectedCellStateViewModel = value;
                OnPropertyChanged("SelectedCellStateViewModel");
            }
        }

        public GameStateViewModel()
        {
        }
    }
}
