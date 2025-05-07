# Grapevine extensions for the MCP C# SDK

[![NuGet preview version](https://img.shields.io/nuget/vpre/ModelContextProtocol.svg)](https://www.nuget.org/packages/ModelContextProtocol/absoluteLatest)

The official C# SDK for the [Model Context Protocol](https://modelcontextprotocol.io/), enabling .NET applications, services, and libraries to implement and interact with MCP clients and servers. Please visit our [API documentation](https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.html) for more details on available functionality.

> [!NOTE]
> This project is in preview; breaking changes can be introduced without prior notice.

## About MCP

The Model Context Protocol (MCP) is an open protocol that standardizes how applications provide context to Large Language Models (LLMs). It enables secure integration between LLMs and various data sources and tools.

For more information about MCP:

- [Official Documentation](https://modelcontextprotocol.io/)
- [Protocol Specification](https://spec.modelcontextprotocol.io/)
- [GitHub Organization](https://github.com/modelcontextprotocol)

## Installation

To get started, install the package from NuGet

```
dotnet new console
dotnet add package Grapevine.Extensions.Mcp --prerelease
```

## Getting Started

```csharp
// Program.cs
using System;
using Grapevine;
using Grapevine.Extensions.Mcp;
using ModelContextProtocol.Server;

[McpServerToolType]
public static class EchoTool
{
    [McpServerTool, System.ComponentModel.Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"hello {message}";
}

class Program
{
    static void Main(string[] args)
    {
        var builder = new RestServerBuilder(
            services => services.AddMcpServer()
                                .WithHttpTransport()
                                .WithPromptsFromAssembly()
                                .WithToolsFromAssembly(),
            server => server.Prefixes.Add("http://localhost:3001/")
        );
        var server = builder.Build();

        server.MapMcp("/mcp");
        server.Start();

        Console.WriteLine("MCP server listening on http://localhost:3001/mcp");
        Console.ReadLine();
        server.Stop();
    }
}
```
