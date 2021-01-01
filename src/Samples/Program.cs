using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
                server.AfterStarting += (s) =>
                {
                    // This will produce a weird name in the output like `<Main>b__0_2` or something unless you add a name argument.
                    s.Router.Register(new Route(async (ctx) => { await ctx.Response.SendResponseAsync("What's my name?"); }, "Get", "/addhoc"));
                };

                server.AfterStarting += (s) => {
                    var sb = new StringBuilder();
                    sb.Append($"********************************************************************************{Environment.NewLine}");
                    sb.Append($"* Server listening on {string.Join(", ", server.Prefixes)}{Environment.NewLine}");
                    sb.Append($"* Stop server by going to {server.Prefixes.First()}stop{Environment.NewLine}");
                    sb.Append($"********************************************************************************{Environment.NewLine}");
                    Console.Write(sb.ToString());

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
