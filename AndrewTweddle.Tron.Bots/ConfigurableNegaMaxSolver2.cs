using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;

namespace AndrewTweddle.Tron.Bots
{
    public class ConfigurableNegaMaxSolver2 : NegaMaxSolverWithConfigurationFilesInExeFolder
    {
        public ConfigurableNegaMaxSolver2(): base("ConfigurableSolver2") 
        {
        }
    }
}
