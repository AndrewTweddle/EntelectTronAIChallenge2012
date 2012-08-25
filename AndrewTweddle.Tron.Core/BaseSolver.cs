using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace AndrewTweddle.Tron.Core
{
    public abstract class BaseSolver: ISolver, INotifyPropertyChanged
    {
        protected abstract void DoSolve();

        public void Solve()
        {
            if (Coordinator == null)
            {
                throw new ApplicationException("The solver has no coordinator");
            }
            DoSolve();
        }
        
        public Coordinator Coordinator { get; set; }

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
