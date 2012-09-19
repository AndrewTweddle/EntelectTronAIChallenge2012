using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    public abstract class NegaMaxSolverWithFileBasedWeightings: BaseNegaMaxSolverWithMetricsWeightings
    {
        protected abstract string GetXmlFileNameForWeightingsInSameCompartment();
        protected abstract string GetXmlFileNameForWeightingsInSeparateCompartments();

        public NegaMaxSolverWithFileBasedWeightings(): base()
        {
        }

        protected void LoadWeightings()
        {
            WeightingsInSameCompartment.ReadWeightingsFromXmlFile(GetXmlFileNameForWeightingsInSameCompartment());
            WeightingsInSeparateCompartment.ReadWeightingsFromXmlFile(GetXmlFileNameForWeightingsInSeparateCompartments());
        }
    }
}
