using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.Tron.Core;
using AndrewTweddle.Tron.Bots;
using System.IO;

namespace AndrewTweddle.Tron.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                throw new InvalidOperationException("The Tron console must provide the path to the file as its first parameter");
            }
            string tronFilePath = args[0];
            if (!File.Exists(tronFilePath))
            {
                string errorMessage = String.Format("The tron file provided does not exist: {0}", tronFilePath);
                throw new InvalidOperationException(errorMessage);
            }
            try
            {
                ISolver solver = new NegaMaxSolver();
                Coordinator coordinator = new Coordinator(solver);
                coordinator.IsInDebugMode = false;
                coordinator.Run(tronFilePath);
            }
            catch (Exception exc)
            {
                System.Console.Error.WriteLine("Suppressing exception: {0}", exc);
                ChooseRandomMove(tronFilePath);
            }
        }

        private static void ChooseRandomMove(string tronFilePath)
        {
            ISolver solver = new RandomSolver();
            Coordinator coordinator = new Coordinator(solver);
            coordinator.IsInDebugMode = false;
            coordinator.Run(tronFilePath);
        }
    }
}
