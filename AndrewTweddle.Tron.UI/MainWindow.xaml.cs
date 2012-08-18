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

namespace AndrewTweddle.Tron.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel MainViewModel
        {
            get
            {
                return DataContext as MainViewModel;
            }
            set
            {
                DataContext = value;
                MainGameStateView.ViewModel = value.GameStateViewModel;
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
        }
    }
}
