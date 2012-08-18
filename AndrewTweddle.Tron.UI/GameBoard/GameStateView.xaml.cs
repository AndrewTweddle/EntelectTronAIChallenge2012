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
        public GameStateViewModel ViewModel
        {
            get
            {
                return DataContext as GameStateViewModel;
            }
            set
            {
                DataContext = value;
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
    }
}
