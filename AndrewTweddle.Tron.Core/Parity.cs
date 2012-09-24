using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    public enum Parity
    {
        Even,    // Sum of x and y coordinates is odd
        Odd,     // Sum of x and y coordinates is odd
        Neutral  // i.e. at a pole
    }
}
