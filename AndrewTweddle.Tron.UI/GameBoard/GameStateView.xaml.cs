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

namespace AndrewTweddle.Tron.UI.GameBoard
{
    /// <summary>
    /// Interaction logic for GameStateView.xaml
    /// </summary>
    public partial class GameStateView : UserControl
    {
        private GameStateViewModel viewModel;

        public GameStateViewModel ViewModel
        {
            get
            {
                return viewModel;
            }
            set
            {
                viewModel = value;
                DataContext = viewModel;
            }
        }

        public GameStateView()
        {
            InitializeComponent();
        }

        public GameStateView(GameStateViewModel viewModel): this()
        {
            ViewModel = viewModel;
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel.OnSelectedCellActivated();
        }

        private void ListBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.OnSelectedCellActivated();
            }
        }
    }
}
