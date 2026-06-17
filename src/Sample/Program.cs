using Grapevine;

var cts = new CancellationTokenSource();

var server = new HttpServer(new HttpServerOptions { Prefixes = ["http://localhost:8080/"] });
var pipeline = new HttpPipeline(async ctx =>
{
    ctx.Response.StatusCode = HttpStatusCode.Ok;
    await ctx.Response.CloseAsync();
});

await server.StartAsync(cts.Token);

var pipelineTask = pipeline.ListenAsync(server.Queue, cts.Token);

Console.WriteLine("Server running. Press Enter to stop.");
Console.ReadLine();

cts.Cancel();
await server.StopAsync();
await pipelineTask;