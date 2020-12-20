using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Grapevine;

namespace Samples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(" Starting Sample Server ");
            using (var server = RestServer.DeveloperConfiguration((services) => { }, true))
            {
                Console.WriteLine($"Server will listen on {string.Join(", ", server.Prefixes)}");
                server.Start();

                OpenBrowser(server.Prefixes.First());

                Console.WriteLine("Press any key to stop the sample server.");
                Console.ReadLine();
            }
        }

        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
