using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Grapevine
{
    public abstract class ContentFolderBase : IContentFolder
    {
        public static string DefaultIndexFileName { get; } = "index.html";

        public static Func<IHttpContext, Task> DefaultFileNotFoundHandler { get; set; } = async (context) =>
        {
            context.Response.StatusCode = HttpStatusCode.NotFound;
            var content = $"File Not Found: {context.Request.Endpoint}";
            await context.Response.SendResponseAsync(content);
        };

        public ConcurrentDictionary<string, string> DirectoryListing { get; protected set; }

        protected FileSystemWatcher Watcher;

        public abstract string IndexFileName { get; set; }

        public abstract string Prefix { get; set; }

        public abstract string FolderPath { get; set; }

        public Func<IHttpContext, Task> FileNotFoundHandler { get; set; } = DefaultFileNotFoundHandler;

        public virtual void AddToDirectoryListing(string fullPath)
        {
            if (DirectoryListing == null)
                DirectoryListing = new ConcurrentDictionary<string, string>();

            DirectoryListing[CreateDirectoryListingKey(fullPath)] = fullPath;

            if (fullPath.EndsWith($"\\{IndexFileName}", StringComparison.CurrentCultureIgnoreCase))
                DirectoryListing[CreateDirectoryListingKey(fullPath.Replace($"\\{IndexFileName}", ""))] = fullPath;
        }

        public virtual string CreateDirectoryListingKey(string item)
        {
            return $"{Prefix}{item.Replace(FolderPath, string.Empty).Replace(@"\", "/")}";
        }

        public virtual void PopulateDirectoryListing()
        {
            if (DirectoryListing?.Count > 0) return;

            Directory.GetFiles(FolderPath, "*", SearchOption.AllDirectories)
                .ToList()
                .ForEach(AddToDirectoryListing);
        }

        public virtual void RemoveFromDirectoryListing(string fullPath)
        {
            if (DirectoryListing == null) return;

            DirectoryListing.Where(x => x.Value == fullPath)
                .ToList()
                .ForEach(pair => DirectoryListing.TryRemove(pair.Key, out string key));
        }

        public virtual void RenameInDirectoryListing(string oldFullPath, string newFullPath)
        {
            RemoveFromDirectoryListing(oldFullPath);
            AddToDirectoryListing(newFullPath);
        }

        public abstract Task SendFileAsync(IHttpContext context);

        public abstract Task SendFileAsync(IHttpContext context, string filename);
    }

    public class ContentFolder : ContentFolderBase, IContentFolder, IDisposable
    {
        private string _indexFileName = DefaultIndexFileName;
        private string _prefix = string.Empty;
        private string _path = string.Empty;

        public ILogger<IContentFolder> Logger { get; protected set; }

        public ContentFolder(string path) : this(path, null, null) { }

        public ContentFolder(string path, string prefix) : this(path, prefix, null) { }

        public ContentFolder(string path, Func<IHttpContext, Task> handler) : this(path, null, handler) { }

        public ContentFolder(string path, string prefix, Func<IHttpContext, Task> handler)
        {
            Logger = DefaultLogger.GetInstance<IContentFolder>();
            FolderPath = path;
            Prefix = prefix;
            FileNotFoundHandler = handler ?? DefaultFileNotFoundHandler;
        }

        public override string FolderPath
        {
            get { return _path; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;

                var path = Path.GetFullPath(value);
                if (_path == path) return;

                if (!Directory.Exists(path)) path = Directory.CreateDirectory(path).FullName;
                _path = path;
                DirectoryListing?.Clear();

                Watcher?.Dispose();
                Watcher = new FileSystemWatcher
                {
                    Path = _path,
                    Filter = "*",
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName
                };

                Watcher.Created += (sender, args) => { AddToDirectoryListing(args.FullPath); };
                Watcher.Deleted += (sender, args) => { RemoveFromDirectoryListing(args.FullPath); };
                Watcher.Renamed += (sender, args) => { RenameInDirectoryListing(args.OldFullPath, args.FullPath); };
            }
        }

        public override string IndexFileName
        {
            get { return _indexFileName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value) || _indexFileName.Equals(value, StringComparison.CurrentCultureIgnoreCase)) return;
                _indexFileName = value;
                DirectoryListing?.Clear();
            }
        }

        public override string Prefix
        {
            get { return _prefix; }
            set
            {
                var prefix = (string.IsNullOrWhiteSpace(value))
                    ? string.Empty
                    : $"/{value.Trim().TrimStart('/').TrimEnd('/').Trim()}";

                if (_prefix.Equals(prefix, StringComparison.CurrentCultureIgnoreCase)) return;

                _prefix = prefix;
                DirectoryListing?.Clear();
            }
        }

        public void Dispose()
        {
            Watcher?.Dispose();
        }

        public async override Task SendFileAsync(IHttpContext context)
        {
            await SendFileAsync(context, null);
        }

        public async override Task SendFileAsync(IHttpContext context, string filename)
        {
            PopulateDirectoryListing();
            if (DirectoryListing.ContainsKey(context.Request.Endpoint))
            {
                var filepath = DirectoryListing[context.Request.Endpoint];
                context.Response.StatusCode = HttpStatusCode.Ok;

                var lastModified = File.GetLastWriteTimeUtc(filepath).ToString("R");
                context.Response.AddHeader("Last-Modified", lastModified);

                if (context.Request.Headers.AllKeys.Contains("If-Modified-Since"))
                {
                    if (context.Request.Headers["If-Modified-Since"].Equals(lastModified))
                    {
                        context.Response.StatusCode = HttpStatusCode.NotModified;
                    }
                }

                if (!string.IsNullOrWhiteSpace(filename))
                    context.Response.AddHeader("Content-Disposition", $"attachment; filename=\"{filename}\"");

                context.Response.ContentType = ContentType.FindKey(Path.GetExtension(filepath).TrimStart('.').ToLower());

                using (var stream = new FileStream(filepath, FileMode.Open))
                {
                    await context.Response.SendResponseAsync(stream);
                }
            }

            // File not found, but should have been based on the path info
            else if (!string.IsNullOrEmpty(Prefix) && context.Request.Endpoint.StartsWith(Prefix, StringComparison.CurrentCultureIgnoreCase))
            {
                context.Response.StatusCode = HttpStatusCode.NotFound;
            }
        }
    }
}