using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;

namespace AndrewTweddle.Tron.Bots
{
    public class PendulumState
    {
        public int Level { get; set; }
        public Block FilledBlock { get; set; }
        public Block LeftGap { get; set; }
        public Block RightGap { get; set; }
    }
}
