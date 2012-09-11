using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace AndrewTweddle.Tron.Core
{
    public class Metrics: INotifyPropertyChanged
    {
        private int numberOfCellsReachableByPlayer;
        private int totalDegreesOfCellsReachableByPlayer;
        private int numberOfCellsClosestToPlayer;
        private int totalDegreesOfCellsClosestToPlayer;

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

            return metrics;
        }

        public Metrics Clone()
        {
            Metrics metrics = new Metrics();
            metrics.numberOfCellsClosestToPlayer = numberOfCellsClosestToPlayer;
            metrics.numberOfCellsReachableByPlayer = numberOfCellsReachableByPlayer;
            metrics.totalDegreesOfCellsClosestToPlayer = totalDegreesOfCellsClosestToPlayer;
            metrics.totalDegreesOfCellsReachableByPlayer = totalDegreesOfCellsReachableByPlayer;
            return metrics;
        }
    }
}
