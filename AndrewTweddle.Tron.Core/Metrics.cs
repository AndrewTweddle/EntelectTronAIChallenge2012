using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace AndrewTweddle.Tron.Core
{
    public class Metrics: INotifyPropertyChanged
    {
        private static Metrics zero;

        private int numberOfCellsReachableByPlayer;
        private int totalDegreesOfCellsReachableByPlayer;
        private int numberOfCellsClosestToPlayer;
        private int totalDegreesOfCellsClosestToPlayer;
        private int numberOfComponentBranchesInTree;
        private int sumOfDistancesFromThisPlayerOnClosestCells;
        private int sumOfDistancesFromOtherPlayerOnClosestCells;

        public static Metrics Zero
        {
            get
            {
                if (zero == null)
                {
                    zero = new Metrics();
                }
                return zero;
            }
        }

        public int NumberOfCellsReachableByPlayer
        {
            get
            {
                return numberOfCellsReachableByPlayer;
            }
            set
            {
                numberOfCellsReachableByPlayer = value;
#if DEBUG
                OnPropertyChanged("NumberOfCellsReachableByPlayer");
#endif
            }
        }

        public int TotalDegreesOfCellsReachableByPlayer
        {
            get
            {
                return totalDegreesOfCellsReachableByPlayer;
            }
            set
            {
                totalDegreesOfCellsReachableByPlayer = value;
#if DEBUG
                OnPropertyChanged("TotalDegreesOfCellsReachableByPlayer");
#endif
            }
        }

        public int NumberOfCellsClosestToPlayer
        {
            get
            {
                return numberOfCellsClosestToPlayer;
            }
            set
            {
                numberOfCellsClosestToPlayer = value;
#if DEBUG
                OnPropertyChanged("NumberOfCellsClosestToPlayer");
#endif
            }
        }

        public int TotalDegreesOfCellsClosestToPlayer
        {
            get
            {
                return totalDegreesOfCellsClosestToPlayer;
            }
            set
            {
                totalDegreesOfCellsClosestToPlayer = value;
#if DEBUG
                OnPropertyChanged("TotalDegreesOfCellsClosestToPlayer");
#endif
            }
        }

        public int SumOfDistancesFromThisPlayerOnClosestCells
        {
            get
            {
                return sumOfDistancesFromThisPlayerOnClosestCells;
            }
            set
            {
                sumOfDistancesFromThisPlayerOnClosestCells = value;
#if DEBUG
                OnPropertyChanged("SumOfDistancesFromThisPlayerOnClosestCells");
#endif
            }
        }

        public int SumOfDistancesFromOtherPlayerOnClosestCells
        {
            get
            {
                return sumOfDistancesFromOtherPlayerOnClosestCells;
            }
            set
            {
                sumOfDistancesFromOtherPlayerOnClosestCells = value;
#if DEBUG
                OnPropertyChanged("SumOfDistancesFromOtherPlayerOnClosestCells");
#endif
            }
        }

        public int NumberOfComponentBranchesInTree
        {
            get
            {
                return numberOfComponentBranchesInTree;
            }
            set
            {
                numberOfComponentBranchesInTree = value;
#if DEBUG
                OnPropertyChanged("NumberOfComponentBranchesInTree");
#endif
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }

        public static Metrics operator+(Metrics metrics1, Metrics metrics2)
        {
            Metrics metrics = new Metrics();

            metrics.NumberOfCellsClosestToPlayer = metrics1.NumberOfCellsClosestToPlayer + metrics2.NumberOfCellsClosestToPlayer;
            metrics.NumberOfCellsReachableByPlayer = metrics1.NumberOfCellsReachableByPlayer + metrics2.NumberOfCellsReachableByPlayer;
            metrics.TotalDegreesOfCellsClosestToPlayer = metrics1.TotalDegreesOfCellsClosestToPlayer + metrics2.TotalDegreesOfCellsClosestToPlayer;
            metrics.TotalDegreesOfCellsReachableByPlayer = metrics1.TotalDegreesOfCellsReachableByPlayer + metrics2.TotalDegreesOfCellsReachableByPlayer;
            metrics.SumOfDistancesFromThisPlayerOnClosestCells = metrics1.SumOfDistancesFromThisPlayerOnClosestCells + metrics2.SumOfDistancesFromThisPlayerOnClosestCells;
            metrics.SumOfDistancesFromOtherPlayerOnClosestCells = metrics1.SumOfDistancesFromOtherPlayerOnClosestCells + metrics2.SumOfDistancesFromOtherPlayerOnClosestCells;

            metrics.NumberOfComponentBranchesInTree = metrics1.NumberOfComponentBranchesInTree + metrics2.NumberOfComponentBranchesInTree;  
                // Not strictly correct, but algorithm overrides it anyway

            return metrics;
        }

        public Metrics Clone()
        {
            Metrics metrics = new Metrics();
            metrics.numberOfCellsClosestToPlayer = numberOfCellsClosestToPlayer;
            metrics.numberOfCellsReachableByPlayer = numberOfCellsReachableByPlayer;
            metrics.totalDegreesOfCellsClosestToPlayer = totalDegreesOfCellsClosestToPlayer;
            metrics.totalDegreesOfCellsReachableByPlayer = totalDegreesOfCellsReachableByPlayer;
            metrics.numberOfComponentBranchesInTree = numberOfComponentBranchesInTree;
            metrics.sumOfDistancesFromThisPlayerOnClosestCells = sumOfDistancesFromThisPlayerOnClosestCells;
            metrics.sumOfDistancesFromOtherPlayerOnClosestCells = sumOfDistancesFromOtherPlayerOnClosestCells;
            return metrics;
        }
    }
}
