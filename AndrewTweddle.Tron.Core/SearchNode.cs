﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace AndrewTweddle.Tron.Core
{
    public class SearchNode: INotifyPropertyChanged
    {
        #region Private Member variables

        private WeakReference weakReferenceToGameState;
        private GameState gameState;
        private double evaluation;
        private SearchNode parentNode;
        private ObservableCollection<SearchNode> childNodes = new ObservableCollection<SearchNode>();
        private int depth;
        private ExpansionStatus expansionStatus;
        private EvaluationStatus evaluationStatus;
        private GameStateStoragePolicy gameStateStoragePolicy;
        private Move move;

        #endregion

        #region Public properties

        public int Depth 
        {
            get
            {
                return depth;
            }
            private set
            {
                depth = value;
                OnPropertyChanged("Depth");
            }
        }

        public double Evaluation 
        {
            get
            {
                return evaluation;
            }
            set
            {
                evaluation = value;
                OnPropertyChanged("Evaluation");
                EvaluationStatus = Core.EvaluationStatus.Evaluated;
            }
        }

        public SearchNode ParentNode 
        {
            get
            {
                return parentNode;
            }
            private set
            {
                parentNode = value;
                OnPropertyChanged("ParentNode");
            }
        }

        public ObservableCollection<SearchNode> ChildNodes
        {
            get
            {
                return childNodes;
            }
        }

        public ExpansionStatus ExpansionStatus
        {
            get
            {
                return expansionStatus;
            }
            private set
            {
                expansionStatus = value;
                OnPropertyChanged("ExpansionStatus");
            }
        }
        
        public EvaluationStatus EvaluationStatus
        {
            get
            {
                return evaluationStatus;
            }
            set
            {
                evaluationStatus = value;
                OnPropertyChanged("EvaluationStatus");
            }
        }
        
        public GameStateStoragePolicy GameStateStoragePolicy
        {
            get
            {
                return gameStateStoragePolicy;
            }
            set
            {
                gameStateStoragePolicy = value;
                OnPropertyChanged("GameStateStoragePolicy");
            }
        }
        
        public Move Move
        {
            get
            {
                return move;
            }
            private set
            {
                move = value;
                OnPropertyChanged("Move");
            }
        }

        public GameState GameState {
            get
            {
                return GetOrGenerateGameState();
            }
            private set
            {
                SetGameState(value);
                OnPropertyChanged("GameState");
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructors

        private SearchNode() { }

        /* This constuctor is for root nodes: */
        public SearchNode(GameState rootGameState)
        {
            // Always maintain a strong reference to the root game state:
            GameStateStoragePolicy = Core.GameStateStoragePolicy.StrongReference;
            GameState = rootGameState;
        }

        public SearchNode(SearchNode parentNode, Move move)
        {
            if (parentNode == null)
            {
                throw new InvalidOperationException(
                    "Incorrect constructor used to create the root search node (a game state must be provided)");
            }
            ParentNode = parentNode;
            Move = move;
            if (parentNode == null)
            {
                Depth = 0;
            }
            else
            {
                Depth = parentNode.Depth + 1;
            }
        }

        #endregion

        #region Public methods

        /* TODO: Add a method to partially expand a search tree:
        public void AddChildSearchNode(GameState gameState)
        {
            SearchNode childSearchNode = new SearchNode(this, gameState);
            childNodes.Add(childSearchNode);
        }
         */

        public void Expand()
        {
            IEnumerable<Move> possibleMoves = GameState.GetPossibleNextMoves();
            IList<SearchNode> childSearchNodes = possibleMoves.Select(move => new SearchNode(this, move)).ToList();
            if (childNodes.Any())
            {
                childSearchNodes = childSearchNodes.Where(child => !childNodes.Contains(child)).ToList();
            }
            if (childSearchNodes.Any())
            {
                foreach (SearchNode childSearchNode in childSearchNodes)
                {
                    childNodes.Add(childSearchNode);
                }
            }
            ExpansionStatus = ExpansionStatus.FullyExpanded;
            if (GameStateStoragePolicy == GameStateStoragePolicy.StrongReferenceOnRootAndLeafNodeOnly
                && ParentNode != null && gameState != null)
            {
                /* This is no longer a leaf node, so change reference from a strong to a weak reference: */
                weakReferenceToGameState = new WeakReference(gameState);
                gameState = null;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is SearchNode)
            {
                SearchNode otherSearchNode = (SearchNode)obj;
                return otherSearchNode.Depth == Depth && otherSearchNode.Move == Move 
                    && ( otherSearchNode.ParentNode.Equals(ParentNode)
                         || (otherSearchNode.ParentNode == null && ParentNode == null)
                       );
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Depth ^ Move.GetHashCode() ^ (ParentNode == null ? 0 : ParentNode.GetHashCode());
        }

        #endregion

        #region Protected methods

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }

        #endregion

        #region Private methods

        private GameState GetOrGenerateGameState()
        {
            if (gameState != null)
            {
                return gameState;
            }

            if (weakReferenceToGameState != null && weakReferenceToGameState.IsAlive)
            {
                GameState gs = null;

                /* What happens if a GC occurs here? Will the following throw an error, or just be null? 
                 * Trap the error in case...
                 */
                try
                {
                    gs = (GameState)weakReferenceToGameState.Target;
                }
                catch
                {
                    // Trap any exception, since we are going to check IsAlive again anyway
                }

                // There could be race condition if we don't check IsAlive again:
                if (weakReferenceToGameState.IsAlive)
                {
                    return gs;
                }
            }

            if (ParentNode == null)
            {
                // This is the root node of the game search tree. It's supposed to always have the starting game state!
                throw new ApplicationException("Root search node had no game state");
            }

            // Recursively get from parent:
            GameState parentGameState = ParentNode.GetOrGenerateGameState();
            GameState newGameState = parentGameState.Clone();
            newGameState.MakeMove(Move);
            GameState = newGameState;
            return newGameState;
        }

        private void SetGameState(GameState newGameState)
        {
            switch (GameStateStoragePolicy)
            {
                case GameStateStoragePolicy.StrongReferenceOnRootAndLeafNodeOnly:
                    if (ExpansionStatus == ExpansionStatus.FullyExpanded)
                    {
                        if (weakReferenceToGameState == null)
                        {
                            weakReferenceToGameState = new WeakReference(newGameState);
                        }
                        else
                        {
                            weakReferenceToGameState.Target = newGameState;
                        }
                    }
                    else
                    {
                        gameState = newGameState;
                    }
                    break;
                case GameStateStoragePolicy.StrongReference:
                    if (weakReferenceToGameState != null && weakReferenceToGameState.Target != null)
                    {
                        weakReferenceToGameState = null;
                    }
                    gameState = newGameState;
                    break;
                case Core.GameStateStoragePolicy.WeakReference:
                    if (weakReferenceToGameState == null)
                    {
                        weakReferenceToGameState = new WeakReference(newGameState);
                    }
                    else
                    {
                        weakReferenceToGameState.Target = newGameState;
                    }
                    gameState = null;
                    break;
                default:
                    /* Ignore the attempt to store the game state: */
                    /* was...
                    string errorMessage = String.Format(
                        "Invalid attempt to set the GameState property when the storage policy is {0}",
                        GameStateStoragePolicy);
                    throw new InvalidOperationException(errorMessage);
                     */
                    break;
            }
        }

        #endregion
    }
}
