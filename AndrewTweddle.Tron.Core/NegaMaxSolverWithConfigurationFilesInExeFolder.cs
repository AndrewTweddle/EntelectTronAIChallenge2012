using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace AndrewTweddle.Tron.Core
{
    public class NegaMaxSolverWithConfigurationFilesInExeFolder: NegaMaxSolverWithFileBasedWeightings
    {
        public string Name
        {
            get;
            private set;
        }

        public NegaMaxSolverWithConfigurationFilesInExeFolder(string name)
        {
            Name = name;
            LoadWeightings();
        }

        protected override string GetXmlFileNameForWeightingsInSameCompartment()
        {
            string exeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string xmlWeightingsFilePath = Path.Combine(exeFolder, String.Format("{0}_SameCompartment.xml", Name));
            return xmlWeightingsFilePath;
        }

        protected override string GetXmlFileNameForWeightingsInSeparateCompartments()
        {
            string exeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string xmlWeightingsFilePath = Path.Combine(exeFolder, String.Format("{0}_SeparateCompartments.xml", Name));
            return xmlWeightingsFilePath;
        }
    }
}
