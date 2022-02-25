using System.Threading.Tasks;
using Grapevine;
using HttpMultipartParser;
using Microsoft.Extensions.Logging;

namespace Samples.Resources
{
    [RestResource(BasePath = "form")]
    public class FormDataResource
    {
        private readonly ILogger<FormDataResource> _logger;

        public FormDataResource(ILogger<FormDataResource> logger)
        {
            _logger = logger;
        }

        [RestRoute("Post", "/submit/data", Name = "Upload form data", Description = "This demonstrates how to parse application/www-form-urlencoded data.")]
        [Header("Content-Type", "application/x-www-form-urlencoded")]
        public async Task ParseUrlEncodedFormData(IHttpContext context)
        {
            // set a breakpoint here to see the auto-parsed data
            await context.Response.SendResponseAsync(HttpStatusCode.Ok);
        }

        [RestRoute("Post", "/submit/data", Name = "Upload form data", Description = "This demonstrates how to parse simple multipart/form-data.")]
        [Header("Content-Type", "multipart/form-data")]
        public async Task ParseMultipartFormData(IHttpContext context)
        {
            // https://github.com/Http-Multipart-Data-Parser/Http-Multipart-Data-Parser
            var content = await MultipartFormDataParser.ParseAsync(context.Request.InputStream, context.Request.MultipartBoundary, context.Request.ContentEncoding);
            var name = $"{content.GetParameterValue("FirstName")} {content.GetParameterValue("LastName")} : {content.Files.Count}";
            await context.Response.SendResponseAsync(name);
        }
    }
}