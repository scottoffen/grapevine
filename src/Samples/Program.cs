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
            // using (var server = RestServerBuilder.UseDefaults().Build())
            using (var server = RestServerBuilder.From<Startup>().Build())
            {
                server.AfterStarting += (s) => {

                    Console.WriteLine("********************************************************************************");
                    Console.WriteLine($"* Server listening on {string.Join(", ", server.Prefixes)}");
                    Console.WriteLine($"* Stop server by going to {server.Prefixes.First()}stop");
                    Console.WriteLine("********************************************************************************");

                    OpenBrowser(s.Prefixes.First());
                };

                server.Run();
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
