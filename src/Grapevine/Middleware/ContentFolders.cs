using System;
using System.Threading.Tasks;

namespace Grapevine.Middleware
{
    public static class ContentFolders
    {
        public async static Task SendFileIfExistsAsnyc(IHttpContext context)
        {
            // If a matching file is found, the request will be responded to.
            if (context.Request.HttpMethod == HttpMethod.Get)
            {
                foreach (var folder in context.Server.ContentFolders)
                {
                    await folder.SendFileAsync(context);

                    // If a matching file *should have* been found, but wasn't,
                    // then the status code on the response will no longer be 200.
                    if (context.Response.StatusCode == HttpStatusCode.NotFound)
                    {
                        await folder.FileNotFoundHandler(context);
                    }

                    if (context.WasRespondedTo) return;
                }

                if (!context.WasRespondedTo && context.Request.Endpoint.Equals("/favicon.ico", StringComparison.CurrentCultureIgnoreCase))
                {
                    await ContentFolderBase.DefaultFileNotFoundHandler(context);
                }
            }
        }
    }
}