using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    public enum CompartmentStatus: byte
    {
        InOtherCompartment = 0,
        InYourCompartment = 1,
        InOpponentsCompartment = 2,
        InSharedCompartment = 3
    }
}
