using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.ComponentModel;

namespace AndrewTweddle.Tron.Core
{
    public class MetricsWeightings: INotifyPropertyChanged
    {
        private double numberOfCellsReachableByPlayerFactor;
        private double totalDegreesOfCellsReachableByPlayerFactor;
        private double numberOfCellsClosestToPlayerFactor;
        private double totalDegreesOfCellsClosestToPlayerFactor;
        private double numberOfComponentBranchesInTreeFactor;
        private double sumOfDistancesFromThisPlayerOnClosestCellsFactor;
        private double sumOfDistancesFromOtherPlayerOnClosestCellsFactor;
        private double chamberValueFactor;

        public double NumberOfCellsReachableByPlayerFactor
        {
            get
            {
                return numberOfCellsReachableByPlayerFactor;
            }
            set
            {
                numberOfCellsReachableByPlayerFactor = value;
#if DEBUG
                OnPropertyChanged("NumberOfCellsReachableByPlayerFactor");
#endif
            }
        }

        public double TotalDegreesOfCellsReachableByPlayerFactor
        {
            get
            {
                return totalDegreesOfCellsReachableByPlayerFactor;
            }
            set
            {
                totalDegreesOfCellsReachableByPlayerFactor = value;
#if DEBUG
                OnPropertyChanged("TotalDegreesOfCellsReachableByPlayerFactor");
#endif
            }
        }

        public double NumberOfCellsClosestToPlayerFactor
        {
            get
            {
                return numberOfCellsClosestToPlayerFactor;
            }
            set
            {
                numberOfCellsClosestToPlayerFactor = value;
#if DEBUG
                OnPropertyChanged("NumberOfCellsClosestToPlayerFactor");
#endif
            }
        }

        public double TotalDegreesOfCellsClosestToPlayerFactor
        {
            get
            {
                return totalDegreesOfCellsClosestToPlayerFactor;
            }
            set
            {
                totalDegreesOfCellsClosestToPlayerFactor = value;
#if DEBUG
                OnPropertyChanged("TotalDegreesOfCellsClosestToPlayerFactor");
#endif
            }
        }

        public double SumOfDistancesFromThisPlayerOnClosestCellsFactor
        {
            get
            {
                return sumOfDistancesFromThisPlayerOnClosestCellsFactor;
            }
            set
            {
                sumOfDistancesFromThisPlayerOnClosestCellsFactor = value;
#if DEBUG
                OnPropertyChanged("SumOfDistancesFromThisPlayerOnClosestCellsFactor");
#endif
            }
        }

        public double SumOfDistancesFromOtherPlayerOnClosestCellsFactor
        {
            get
            {
                return sumOfDistancesFromOtherPlayerOnClosestCellsFactor;
            }
            set
            {
                sumOfDistancesFromOtherPlayerOnClosestCellsFactor = value;
#if DEBUG
                OnPropertyChanged("SumOfDistancesFromOtherPlayerOnClosestCellsFactor");
#endif
            }
        }

        public double NumberOfComponentBranchesInTreeFactor
        {
            get
            {
                return numberOfComponentBranchesInTreeFactor;
            }
            set
            {
                numberOfComponentBranchesInTreeFactor = value;
#if DEBUG
                OnPropertyChanged("NumberOfComponentBranchesInTreeFactor");
#endif
            }
        }

        public double ChamberValueFactor
        {
            get
            {
                return chamberValueFactor;
            }
            set
            {
                chamberValueFactor = value;
#if DEBUG
                OnPropertyChanged("ChamberValueFactor");
#endif
            }
        }

        public void ReadWeightingsFromXmlFile(string xmlFilePath)
        {
            XDocument xdoc = XDocument.Load(xmlFilePath);
            XElement root = xdoc.Element("Weightings");
            NumberOfCellsReachableByPlayerFactor = GetElementValueAsDouble(root, "NumberOfCellsReachableByPlayerFactor", xmlFilePath);
            TotalDegreesOfCellsReachableByPlayerFactor = GetElementValueAsDouble(root, "TotalDegreesOfCellsReachableByPlayerFactor", xmlFilePath);
            NumberOfCellsClosestToPlayerFactor = GetElementValueAsDouble(root, "NumberOfCellsClosestToPlayerFactor", xmlFilePath);
            TotalDegreesOfCellsClosestToPlayerFactor = GetElementValueAsDouble(root, "TotalDegreesOfCellsClosestToPlayerFactor", xmlFilePath);
            NumberOfComponentBranchesInTreeFactor = GetElementValueAsDouble(root, "NumberOfComponentBranchesInTreeFactor", xmlFilePath);
            SumOfDistancesFromThisPlayerOnClosestCellsFactor = GetElementValueAsDouble(root, "SumOfDistancesFromThisPlayerOnClosestCellsFactor", xmlFilePath);
            SumOfDistancesFromOtherPlayerOnClosestCellsFactor = GetElementValueAsDouble(root, "SumOfDistancesFromOtherPlayerOnClosestCellsFactor", xmlFilePath);
            ChamberValueFactor = GetElementValueAsDouble(root, "ChamberValueFactor", xmlFilePath);
        }

        private static double GetElementValueAsDouble(XElement root, string elementName, string xmlFilePath)
        {
            XElement element = root.Element(elementName);
            if (element == null)
            {
                return 0.0;
            }
            double value = 0.0;
            bool canParse = double.TryParse(element.Value, out value);
            if (!canParse)
            {
                string errorMessage = String.Format(
                    "Could not convert '{0}' to a string when reading {1} from {2}", element.Value, elementName, xmlFilePath);
                throw new ApplicationException(errorMessage);
            }
            return value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, args);
            }
        }
    }
}
