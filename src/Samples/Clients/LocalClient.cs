using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Grapevine;
using Grapevine.Client;

namespace Samples.Clients
{
    public class LocalClient
    {
        private readonly HttpClient _client;

        private IRestServer _server;

        public readonly Thread InteractiveShell;

        private bool exit = false;

        public LocalClient(HttpClient client)
        {
            _client = client;
            InteractiveShell = new Thread(InteractiveShellAsync);
        }

        public void RunInteractiveShell(IRestServer server)
        {
            _server = server;
            InteractiveShell.Start();
        }

        protected async void InteractiveShellAsync()
        {
            while (!_server.IsListening) { }
            Console.WriteLine($"Welcome to the interactive cookie shell.");
            Console.WriteLine("This shell will test the sending and receiving of cookies via the Grapevine.Client extension methods.");
            Console.WriteLine($"Type {nameof(exit)} to exit.");

            while (_server.IsListening && !exit)
            {
                Console.WriteLine();
                var name = GetUserInput("Enter the cookie name");
                if (exit) continue;

                var value = GetUserInput("Enter the cookie value");
                if (exit) continue;

                Console.WriteLine("Setting cookie...");
                var response = await _client.UsingRoute($"/cookie/set/{name}/{value}")
                    .GetAsync();

                var cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie");
                Console.WriteLine("Cookies:");
                foreach (var cookie in cookies.Value) Console.WriteLine(cookie);

                Console.WriteLine();
                Console.WriteLine("Retrieving sent cookie...");
                var output = await _client.UsingRoute($"/cookie/get/{name}")
                    .WithCookie(name, value)
                    .GetAsync()
                    .GetResponseStringAsync();

                Console.WriteLine(output);
            }

            Console.WriteLine("You have exited the interactive cookie shell.");
        }

        protected string GetUserInput(string prompt)
        {
            var value = string.Empty;

            while (string.IsNullOrWhiteSpace(value))
            {
                Console.Write($"{prompt}: ");
                value = Console.ReadLine().Trim();
            }

            if (value.Equals(nameof(exit), StringComparison.CurrentCultureIgnoreCase)) exit = true;
            return value;
        }
    }
}