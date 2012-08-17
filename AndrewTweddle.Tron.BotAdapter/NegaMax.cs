using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Bots;

namespace AndrewTweddle.Tron.BotAdapter
{
    public class NegaMax: BaseBotAdapter
    {
        protected override Core.ISolver CreateSolver()
        {
            return new NegaMaxSolver();
        }
    }
}
