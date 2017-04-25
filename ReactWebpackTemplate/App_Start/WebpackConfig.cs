using log4net;
using Microsoft.Owin;
using Owin;
using ReactWebpackTemplate.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ReactWebpackTemplate
{
    public class WebpackDevServerMiddleware : OwinMiddleware
    {
        OwinMiddleware _next;
        private readonly ILog _logger;
        WebpackDevServerMiddlewareOptions _options;
        WebpackDevServerConfig _config;

        public WebpackDevServerMiddleware(OwinMiddleware next, WebpackDevServerMiddlewareOptions options)
            : base(next)
        {
            _next = next;
            _logger = LogManager.GetLogger(typeof(WebpackDevServerMiddleware));
            _options = options;
            
            try
            {
                _config = WebpackDevServerConfig.Configure(_options);
            }
            catch(Exception ex)
            {
                _logger.Error("Could not load webpack config", ex);
            }
        }

        public override async Task Invoke(IOwinContext context)
        {
            var buffer = new MemoryStream();
            var stream = context.Response.Body;
            context.Response.Body = buffer;
            
            await _next.Invoke(context);

            buffer.Seek(0, SeekOrigin.Begin);

            var isHtml = context.Response.ContentType?.ToLower().Contains("text/html");
            if (context.Response.StatusCode == 200 && isHtml.GetValueOrDefault())
            {
                using (var reader = new StreamReader(buffer))
                {
                    var response = await reader.ReadToEndAsync();
                    if (response.Contains("</body>"))
                    {
                        _logger.Info("A full html page is returned so the necessary script for webpack will be injected");
                        
                        var scriptTags = string.Join("\n", _options.HotModuleReloading
                            ? _config.Entries.Select(e => string.Format("<script src=\"http://{0}:{1}/{2}/{3}\"></script",
                                _options.Host, _options.Port, _config.OutputDir, e))
                            : _config.Entries.Select(e => string.Format("<script src=\"{0}/{1}\"></script>", _config.OutputDir, e))
                        );
                        
                        response = response.Replace("</body>", scriptTags + "\n" + "</body>");
                    }

                    using (var memStream = new MemoryStream())
                        using (var writer = new StreamWriter(memStream))
                        {
                            writer.Write(response);
                            writer.Flush();
                            memStream.Seek(0, SeekOrigin.Begin);
                            context.Response.Headers["Content-Length"] = memStream.Length.ToString();
                            await memStream.CopyToAsync(stream);
                        }
                }
            }
            else
            {
                await buffer.CopyToAsync(stream);
            }

            context.Response.Body = stream;
        }
    }

    public static class WebpackDevServerMiddlewareExtensions
    {
        public static IAppBuilder UseWebpackDevServer(
            this IAppBuilder app,
            WebpackDevServerMiddlewareOptions options = null)
        {
            app.Use<WebpackDevServerMiddleware>(options ?? new WebpackDevServerMiddlewareOptions());
            return app;
        }
    }

    public class WebpackDevServerConfig
    {
        public const string DefaultHost = "0.0.0.0";

        public static WebpackDevServerConfig Current { get; private set; }

        public bool HotModuleReloading { get; private set; }
        public string[] Entries { get; private set; }
        public string Host { get; private set; }
        public int Port { get; private set; }
        public string OutputDir { get; private set; }
        public string ScriptTags { get; private set; }

        public static WebpackDevServerConfig Configure(WebpackDevServerMiddlewareOptions options = null)
        {
            if (options == null)
                options = new WebpackDevServerMiddlewareOptions();

            try
            {

                Current = new WebpackDevServerConfig()
                {
                    HotModuleReloading = options.HotModuleReloading,
                    Port = options.Port,
                    Host = string.IsNullOrWhiteSpace(options.Host) ? DefaultHost : options.Host
                };

                if (string.IsNullOrWhiteSpace(options.ConfigFile))
                {
                    options.ConfigFile = Directory.GetFiles(HttpRuntime.AppDomainAppPath, "webpack*.config.js").FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(options.ConfigFile))
                        throw new ArgumentException("No webpack config found in directory");
                }

                var configStr = File.ReadAllText(options.ConfigFile)
                        .Replace(Environment.NewLine, string.Empty)
                        .Replace("\t", string.Empty);

                Current.OutputDir = new Regex(@"output: \{(.*?)publicPath:\s*['|""](.*?)['|""]")
                    .Match(configStr)
                    .Groups[2]
                    .Value;

                if (Current.OutputDir.StartsWith("/"))
                    Current.OutputDir = Current.OutputDir.Substring(1);
                if (Current.OutputDir.EndsWith(@"\"))
                    Current.OutputDir = Current.OutputDir.Substring(0, Current.OutputDir.Length - 1);

                var entryFormat = new Regex(@"output: \{(.*?)filename:\s*['|""](.*?)['|""]")
                    .Match(configStr)
                    .Groups[2]
                    .Value
                    .Replace("[name]", "{0}");
                if (string.IsNullOrWhiteSpace(entryFormat))
                    throw new Exception("No filename format supplied in webpack config");

                var entriesNode = new Regex(@"entry:\s*\{(.*?)\}")
                    .Match(configStr)
                    .Groups[1]
                    .Value;

                Current.Entries = new Regex(@":[\s*]\[(.+?)\]")
                    .Replace(entriesNode, string.Empty)
                    .Split(',')
                    .Select(str => string.Format(entryFormat, str.Replace("'", "").Replace("\"", "").Trim()))
                    .ToArray();

                Current.ScriptTags = string.Join("\n", Current.HotModuleReloading
                    ? Current.Entries.Select(e => string.Format("<script src=\"http://{0}:{1}/{2}/{3}\"></script>",
                        DefaultHost.Equals(Current.Host) ? "localhost" : Current.Host, Current.Port, Current.OutputDir, e))
                    : Current.Entries.Select(e => string.Format("<script src=\"{0}/{1}\"></script>", Current.OutputDir, e))
                );
            }
            catch (Exception ex)
            {
                GeneralHelper.Logger.Error("webpack configuration", ex);
            }

            return Current;
        }
    }

    public class WebpackDevServerMiddlewareOptions
    {
        public string ConfigFile { get; set; }
        public bool HotModuleReloading { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }

        public WebpackDevServerMiddlewareOptions()
        {
            HotModuleReloading = true;
            Port = 3009;
        }
    }
}