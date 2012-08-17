using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    public class SearchNode
    {
        private List<SearchNode> childNodes = new List<SearchNode>();

        public int Depth { get; private set; }
        public double Value { get; set; }
        public bool IsExpanded { get; private set; }

        public GameState GameState { get; private set; }
        public SearchNode ParentNode { get; private set; }
        public IEnumerable<SearchNode> ChildNodes
        {
            get
            {
                return childNodes;
            }
        }

        private SearchNode() {}

        public SearchNode(SearchNode parentNode, GameState gameState)
        {
            GameState = gameState;
            if (parentNode == null)
            {
                Depth = 0;
            }
            else
            {
                Depth = parentNode.Depth + 1;
            }
        }

        /*
        public void AddChildSearchNode(GameState gameState)
        {
            SearchNode childSearchNode = new SearchNode(this, gameState);
            childNodes.Add(childSearchNode);
        }
         */

        public void Expand()
        {
            childNodes.Clear();
            IEnumerable<GameState> possibleMoves = GameState.GetPossibleNextStates();
            IEnumerable<SearchNode> childSearchNodes = possibleMoves.Select(gs => new SearchNode(this, gs));
            childNodes.AddRange(childSearchNodes);
            IsExpanded = true;
        }
    }
}
