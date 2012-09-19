using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;

namespace AndrewTweddle.Tron.Bots
{
    public class ConfigurableNegaMaxSolver1 : NegaMaxSolverWithConfigurationFilesInExeFolder
    {
        public ConfigurableNegaMaxSolver1(): base("ConfigurableSolver1") 
        {
        }
    }
}
