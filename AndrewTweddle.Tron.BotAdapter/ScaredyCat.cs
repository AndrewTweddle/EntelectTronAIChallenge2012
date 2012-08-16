using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Bots;
using AndrewTweddle.Tron.Core;

namespace AndrewTweddle.Tron.BotAdapter
{
    public class ScaredyCat: BaseBotAdapter
    {
        protected override ISolver CreateSolver()
        {
            return new ScaredyCatSolver();
        }
    }
}
