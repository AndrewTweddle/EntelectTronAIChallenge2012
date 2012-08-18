using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AndrewTweddle.Tron.UI.SearchTree
{
    public class SearchTreeViewModel: BaseViewModel
    {
        private SearchNode rootNode;
        private ObservableCollection<SearchNode> rootNodes = new ObservableCollection<SearchNode>();

        public SearchNode RootNode
        {
            get
            {
                return rootNode;
            }
            set
            {
                if (rootNode != value)
                {
                    rootNode = value;
                    rootNodes.Clear();
                    rootNodes.Add(rootNode);
                    OnPropertyChanged("RootNode");
                }
            }
        }

        public ObservableCollection<SearchNode> RootNodes 
        {
            get
            {
                return rootNodes;
            }
        }

        public void DisplayGameState() // TODO: SearchNode node) ???
        {

        }
    }
}
