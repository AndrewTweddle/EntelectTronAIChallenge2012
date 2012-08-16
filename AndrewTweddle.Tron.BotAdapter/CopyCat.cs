using AndrewTweddle.Tron.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Bots;

namespace AndrewTweddle.Tron.BotAdapter
{
    public class CopyCat: BaseBotAdapter
    {
        protected override ISolver CreateSolver()
        {
            return new CopyCatSolver();
        }

        public CopyCat(): base()
        {
        }
    }
}
