using System;
using System.Threading.Tasks;

namespace Grapevine
{
    public interface IContentFolder
    {
         Task SendFileAsync(IHttpContext context);

         Action<IHttpContext> FileNotFoundHandler { get; set; }
    }
}