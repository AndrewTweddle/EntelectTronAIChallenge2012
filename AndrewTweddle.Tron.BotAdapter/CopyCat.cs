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
        public override Core.GameState GenerateNextGameState(GameState gameState)
        {
            CopyCatBot bot = new CopyCatBot();
            return bot.GenerateNextGameState(gameState);
        }
    }
}
