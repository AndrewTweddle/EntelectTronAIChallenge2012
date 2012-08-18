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

        private Color firstPlayersWallColor = Colors.Red;
        private Color secondPlayersWallColor = Colors.Blue;
        private Color firstPlayersCellColor = Colors.DarkRed;
        private Color secondPlayersCellColor = Colors.DarkBlue;
        private Color firstPlayersClosestToColor = Colors.Pink;
        private Color secondPlayersClosestToColor = Colors.LightBlue;

        public Color FirstPlayersWallColor
        {
            get
            {
                return firstPlayersWallColor;
            }
            set
            {
                if (firstPlayersWallColor != value)
                {
                    firstPlayersWallColor = value;
                    OnPropertyChanged("FirstPlayersWallColor");
                    OnPropertyChanged("YourWallColor");
                    OnPropertyChanged("OpponentsWallColor");
                }
            }
        }

        public Color SecondPlayersWallColor
        {
            get
            {
                return secondPlayersWallColor;
            }
            set
            {
                if (secondPlayersWallColor != value)
                {
                    secondPlayersWallColor = value;
                    OnPropertyChanged("SecondPlayersWallColor");
                    OnPropertyChanged("YourWallColor");
                    OnPropertyChanged("OpponentsWallColor");
                }
            }
        }

        public Color FirstPlayersCellColor
        {
            get
            {
                return firstPlayersCellColor;
            }
            set
            {
                if (firstPlayersCellColor != value)
                {
                    firstPlayersCellColor = value;
                    OnPropertyChanged("FirstPlayersCellColor");
                    OnPropertyChanged("YourCellColor");
                    OnPropertyChanged("OpponentsCellColor");
                }
            }
        }

        public Color SecondPlayersCellColor
        {
            get
            {
                return secondPlayersCellColor;
            }
            set
            {
                if (secondPlayersCellColor != value)
                {
                    secondPlayersCellColor = value;
                    OnPropertyChanged("SecondPlayersCellColor");
                    OnPropertyChanged("YourCellColor");
                    OnPropertyChanged("OpponentsCellColor");
                }
            }
        }

        public Color FirstPlayersClosestToColor
        {
            get
            {
                return firstPlayersClosestToColor;
            }
            set
            {
                if (firstPlayersClosestToColor != value)
                {
                    firstPlayersClosestToColor = value;
                    OnPropertyChanged("FirstPlayersClosestToColor");
                    OnPropertyChanged("YourClosestToColor");
                    OnPropertyChanged("OpponentsClosestToColor");
                }
            }
        }

        public Color SecondPlayersClosestToColor
        {
            get
            {
                return secondPlayersClosestToColor;
            }
            set
            {
                if (secondPlayersClosestToColor != value)
                {
                    secondPlayersClosestToColor = value;
                    OnPropertyChanged("SecondPlayersClosestToColor");
                    OnPropertyChanged("YourClosestToColor");
                    OnPropertyChanged("OpponentsClosestToColor");
                }
            }
        }

        public Color YourWallColor
        {
            get
            {
                if (GameState == null)
                {
                    return Colors.DarkGray;
                }
                if (GameState.PlayerWhoMovedFirst == PlayerType.You)
                {
                    return FirstPlayersWallColor;
                }
                return SecondPlayersWallColor;
            }
        }

        public Color OpponentsWallColor
        {
            get
            {
                if (GameState == null)
                {
                    return Colors.DarkGray;
                }
                if (GameState.PlayerWhoMovedFirst == PlayerType.You)
                {
                    return SecondPlayersWallColor;
                }
                return FirstPlayersWallColor;
            }
        }

        public Color YourCellColor
        {
            get
            {
                if (GameState == null)
                {
                    return Colors.DarkGray;
                }
                if (GameState.PlayerWhoMovedFirst == PlayerType.You)
                {
                    return FirstPlayersCellColor;
                }
                return SecondPlayersCellColor;
            }
        }

        public Color OpponentsCellColor
        {
            get
            {
                if (GameState == null)
                {
                    return Colors.DarkGray;
                }
                if (GameState.PlayerWhoMovedFirst == PlayerType.You)
                {
                    return SecondPlayersCellColor;
                }
                return FirstPlayersCellColor;
            }
        }

        public Color YourClosestToColor
        {
            get
            {
                if (GameState == null)
                {
                    return Colors.DarkGray;
                }
                if (GameState.PlayerWhoMovedFirst == PlayerType.You)
                {
                    return FirstPlayersClosestToColor;
                }
                return SecondPlayersClosestToColor;
            }
        }

        public Color OpponentsClosestToColor
        {
            get
            {
                if (GameState == null)
                {
                    return Colors.DarkGray;
                }
                if (GameState.PlayerWhoMovedFirst == PlayerType.You)
                {
                    return SecondPlayersClosestToColor;
                }
                return FirstPlayersClosestToColor;
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

        public GameStateViewModel()
        {
        }
    }
}
