using System;
using System.Threading;
using Grapevine;

namespace Samples
{
    public class InteractiveShell
    {
        private IRestServer _server;
        private bool _detached;

        public InteractiveShell(bool detached = true)
        {
            _detached = detached;
        }

        public void Run(IRestServer server)
        {
            _server = server;

            if (_detached)
                new Thread(ShellRunner).Start();
            else
                ShellRunner();
        }

        public void ShellRunner()
        {
            while (!_server.IsListening) Thread.Sleep(1000);

            var exit = false;
            var exitCmd = nameof(exit).ToLower();

            Console.WriteLine("This is the interactive shell. Use it to communicate with the running application.");
            Console.WriteLine($"Type your command below. Type {exitCmd} to exit.");

            while (_server.IsListening && !exit)
            {
                Console.WriteLine();
                var input = GetCommandInput("Shell is not currently configured.");
                exit = input.ToLower().Equals(exitCmd);
                if (exit) continue;
            }

            Console.WriteLine("You have exited the interactive shell. Good day!");
        }

        public string GetCommandInput(string prompt, bool leadingLineBreak = false)
        {
            while (true)
            {
                if (leadingLineBreak) Console.WriteLine();
                Console.Write($"{prompt} ");
                var input = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(input))
                    return input;
            }
        }
    }
}