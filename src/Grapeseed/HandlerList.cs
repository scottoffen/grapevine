using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grapevine
{
    public class RequestReceivedEvent : List<RequestReceivedAsyncEventHandler>
    {
        public async Task<int> Invoke(IHttpContext context, IRestServer server)
        {
            var counter = 0;
            foreach(var handler in this)
            {
                await handler(context, server);
                counter++;
                if (context.WasRespondedTo) break;
            }
            return counter;
        }

        public static RequestReceivedEvent operator + (RequestReceivedEvent source, RequestReceivedAsyncEventHandler obj)
        {
            source.Add(obj);
            return source;
        }

        public static RequestReceivedEvent operator - (RequestReceivedEvent source, RequestReceivedAsyncEventHandler obj)
        {
            source.Remove(obj);
            return source;
        }
    }

    public class RequestRoutingEvent : List<RoutingAsyncEventHandler>
    {
        public async Task<int> Invoke(IHttpContext context)
        {
            var counter = 0;
            foreach(var handler in this)
            {
                await handler(context);
                counter++;
                if (context.WasRespondedTo) break;
            }
            return counter;
        }

        public static RequestRoutingEvent operator + (RequestRoutingEvent source, RoutingAsyncEventHandler obj)
        {
            source.Add(obj);
            return source;
        }

        public static RequestRoutingEvent operator - (RequestRoutingEvent source, RoutingAsyncEventHandler obj)
        {
            source.Remove(obj);
            return source;
        }
    }
}