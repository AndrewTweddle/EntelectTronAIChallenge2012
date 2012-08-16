using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TronSdk
{
    /// <summary>
    /// Interface to implement a bot
    /// </summary>
    public interface ITronBot
    {
        /// <summary>
        /// This method is called to request a single move from the bot
        /// </summary>
        /// <param name="grid"></param>
        void ExecuteMove(ref BlockTypes[,] grid);
    }
}
