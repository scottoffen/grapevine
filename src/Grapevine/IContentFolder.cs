using System;
using System.Threading.Tasks;

namespace Grapevine
{
    public interface IContentFolder
    {
         Task SendFileAsync(IHttpContext context);

        Task SendFileAsync(IHttpContext context, string filename);

         Action<IHttpContext> FileNotFoundHandler { get; set; }
    }
}