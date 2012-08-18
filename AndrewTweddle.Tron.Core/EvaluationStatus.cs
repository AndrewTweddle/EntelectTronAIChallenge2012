using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    public enum EvaluationStatus: byte
    {
        Unevaluated = 0,
        Evaluated = 1,
        Pruned = 2
    }
}
