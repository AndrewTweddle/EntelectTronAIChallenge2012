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
using AndrewTweddle.Tron.Core;

namespace AndrewTweddle.Tron.UI.SearchTree
{
    /// <summary>
    /// Interaction logic for SearchTreeControl.xaml
    /// </summary>
    public partial class SearchTreeView : UserControl
    {
        public SearchTreeViewModel ViewModel
        {
            get
            {
                return DataContext as SearchTreeViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        public SearchTreeView(SearchTreeViewModel viewModel)
            : this()
        {
            ViewModel = viewModel;
        }

        public SearchTreeView()
        {
            InitializeComponent();
        }

        private void DisplayButton_Click(object sender, RoutedEventArgs e)
        {
            SearchNode node = (sender as Button).Tag as SearchNode;
            ViewModel.DisplayGameState();
        }
    }
}
