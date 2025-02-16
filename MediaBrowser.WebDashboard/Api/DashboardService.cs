﻿using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.IO;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Localization;
using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Extensions;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Net;
using MediaBrowser.Model.Serialization;
using ServiceStack;
using ServiceStack.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonIO;
using WebMarkupMin.Core.Minifiers;

namespace MediaBrowser.WebDashboard.Api
{
    /// <summary>
    /// Class GetDashboardConfigurationPages
    /// </summary>
    [Route("/dashboard/ConfigurationPages", "GET")]
    [Route("/web/ConfigurationPages", "GET")]
    public class GetDashboardConfigurationPages : IReturn<List<ConfigurationPageInfo>>
    {
        /// <summary>
        /// Gets or sets the type of the page.
        /// </summary>
        /// <value>The type of the page.</value>
        public ConfigurationPageType? PageType { get; set; }
    }

    /// <summary>
    /// Class GetDashboardConfigurationPage
    /// </summary>
    [Route("/dashboard/ConfigurationPage", "GET")]
    [Route("/web/ConfigurationPage", "GET")]
    public class GetDashboardConfigurationPage
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }
    }

    [Route("/web/Package", "GET")]
    [Route("/dashboard/Package", "GET")]
    public class GetDashboardPackage
    {
        public string Mode { get; set; }
    }

    [Route("/robots.txt", "GET")]
    public class GetRobotsTxt
    {
    }

    /// <summary>
    /// Class GetDashboardResource
    /// </summary>
    [Route("/web/{ResourceName*}", "GET")]
    [Route("/dashboard/{ResourceName*}", "GET")]
    public class GetDashboardResource
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string ResourceName { get; set; }
        /// <summary>
        /// Gets or sets the V.
        /// </summary>
        /// <value>The V.</value>
        public string V { get; set; }
    }

    /// <summary>
    /// Class DashboardService
    /// </summary>
    public class DashboardService : IRestfulService, IHasResultFactory
    {
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the HTTP result factory.
        /// </summary>
        /// <value>The HTTP result factory.</value>
        public IHttpResultFactory ResultFactory { get; set; }

        /// <summary>
        /// Gets or sets the request context.
        /// </summary>
        /// <value>The request context.</value>
        public IRequest Request { get; set; }

        /// <summary>
        /// The _app host
        /// </summary>
        private readonly IServerApplicationHost _appHost;

        /// <summary>
        /// The _server configuration manager
        /// </summary>
        private readonly IServerConfigurationManager _serverConfigurationManager;

        private readonly IFileSystem _fileSystem;
        private readonly ILocalizationManager _localization;
        private readonly IJsonSerializer _jsonSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardService" /> class.
        /// </summary>
        /// <param name="appHost">The app host.</param>
        /// <param name="serverConfigurationManager">The server configuration manager.</param>
        /// <param name="fileSystem">The file system.</param>
        public DashboardService(IServerApplicationHost appHost, IServerConfigurationManager serverConfigurationManager, IFileSystem fileSystem, ILocalizationManager localization, IJsonSerializer jsonSerializer)
        {
            _appHost = appHost;
            _serverConfigurationManager = serverConfigurationManager;
            _fileSystem = fileSystem;
            _localization = localization;
            _jsonSerializer = jsonSerializer;
        }

        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public object Get(GetDashboardConfigurationPage request)
        {
            var page = ServerEntryPoint.Instance.PluginConfigurationPages.First(p => p.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase));

            return ResultFactory.GetStaticResult(Request, page.Plugin.Version.ToString().GetMD5(), null, null, MimeTypes.GetMimeType("page.html"), () => GetPackageCreator().ModifyHtml(page.GetHtmlStream(), null, _appHost.ApplicationVersion.ToString(), null, false));
        }

        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public object Get(GetDashboardConfigurationPages request)
        {
            const string unavilableMessage = "The server is still loading. Please try again momentarily.";

            var instance = ServerEntryPoint.Instance;

            if (instance == null)
            {
                throw new InvalidOperationException(unavilableMessage);
            }

            var pages = instance.PluginConfigurationPages;

            if (pages == null)
            {
                throw new InvalidOperationException(unavilableMessage);
            }

            if (request.PageType.HasValue)
            {
                pages = pages.Where(p => p.ConfigurationPageType == request.PageType.Value);
            }

            // Don't allow a failing plugin to fail them all
            var configPages = pages.Select(p =>
            {

                try
                {
                    return new ConfigurationPageInfo(p);
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("Error getting plugin information from {0}", ex, p.GetType().Name);
                    return null;
                }
            })
                .Where(i => i != null)
                .ToList();

            return ResultFactory.GetOptimizedResult(Request, configPages);
        }

        public object Get(GetRobotsTxt request)
        {
            return Get(new GetDashboardResource
            {
                ResourceName = "robots.txt"
            });
        }

        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>System.Object.</returns>
        public object Get(GetDashboardResource request)
        {
            var path = request.ResourceName;

            path = path.Replace("bower_components" + _appHost.ApplicationVersion, "bower_components", StringComparison.OrdinalIgnoreCase);

            var contentType = MimeTypes.GetMimeType(path);

            // Bounce them to the startup wizard if it hasn't been completed yet
            if (!_serverConfigurationManager.Configuration.IsStartupWizardCompleted && path.IndexOf("wizard", StringComparison.OrdinalIgnoreCase) == -1 && GetPackageCreator().IsCoreHtml(path))
            {
                // But don't redirect if an html import is being requested.
                if (path.IndexOf("bower_components", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    Request.Response.Redirect("wizardstart.html");
                    return null;
                }
            }

            path = path.Replace("scripts/jquery.mobile-1.4.5.min.map", "thirdparty/jquerymobile-1.4.5/jquery.mobile-1.4.5.min.map", StringComparison.OrdinalIgnoreCase);

            var localizationCulture = GetLocalizationCulture();

            // Don't cache if not configured to do so
            // But always cache images to simulate production
            if (!_serverConfigurationManager.Configuration.EnableDashboardResponseCaching &&
                !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) &&
                !contentType.StartsWith("font/", StringComparison.OrdinalIgnoreCase))
            {
                return ResultFactory.GetResult(GetResourceStream(path, localizationCulture).Result, contentType);
            }

            TimeSpan? cacheDuration = null;

            // Cache images unconditionally - updates to image files will require new filename
            // If there's a version number in the query string we can cache this unconditionally
            if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) || contentType.StartsWith("font/", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrEmpty(request.V))
            {
                cacheDuration = TimeSpan.FromDays(365);
            }

            var assembly = GetType().Assembly.GetName();

            var cacheKey = (assembly.Version + (localizationCulture ?? string.Empty) + path).GetMD5();

            return ResultFactory.GetStaticResult(Request, cacheKey, null, cacheDuration, contentType, () => GetResourceStream(path, localizationCulture));
        }

        private string GetLocalizationCulture()
        {
            return _serverConfigurationManager.Configuration.UICulture;
        }

        /// <summary>
        /// Gets the resource stream.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="localizationCulture">The localization culture.</param>
        /// <returns>Task{Stream}.</returns>
        private Task<Stream> GetResourceStream(string path, string localizationCulture)
        {
            var minify = _serverConfigurationManager.Configuration.EnableDashboardResourceMinification;

            return GetPackageCreator()
                .GetResource(path, null, localizationCulture, _appHost.ApplicationVersion.ToString(), minify);
        }

        private PackageCreator GetPackageCreator()
        {
            return new PackageCreator(_fileSystem, _localization, Logger, _serverConfigurationManager, _jsonSerializer);
        }

        public async Task<object> Get(GetDashboardPackage request)
        {
            var path = Path.Combine(_serverConfigurationManager.ApplicationPaths.ProgramDataPath,
                "webclient-dump");

            try
            {
                _fileSystem.DeleteDirectory(path, true);
            }
            catch (IOException)
            {

            }

            var creator = GetPackageCreator();

            CopyDirectory(creator.DashboardUIPath, path);

            string culture = null;

            var appVersion = _appHost.ApplicationVersion.ToString();

            var mode = request.Mode;

            if (string.Equals(mode, "cordova", StringComparison.OrdinalIgnoreCase))
            {
                _fileSystem.DeleteFile(Path.Combine(path, "scripts", "registrationservices.js"));
            }

            // Try to trim the output size a bit
            var bowerPath = Path.Combine(path, "bower_components");

            if (!string.Equals(mode, "cordova", StringComparison.OrdinalIgnoreCase))
            {
                var versionedBowerPath = Path.Combine(Path.GetDirectoryName(bowerPath), "bower_components" + _appHost.ApplicationVersion);
                Directory.Move(bowerPath, versionedBowerPath);
                bowerPath = versionedBowerPath;
            }

            DeleteFilesByExtension(bowerPath, ".log");
            DeleteFilesByExtension(bowerPath, ".txt");
            DeleteFilesByExtension(bowerPath, ".map");
            DeleteFilesByExtension(bowerPath, ".md");
            DeleteFilesByExtension(bowerPath, ".json");
            DeleteFilesByExtension(bowerPath, ".gz");
            DeleteFilesByExtension(bowerPath, ".bat");
            DeleteFilesByExtension(bowerPath, ".sh");
            DeleteFilesByName(bowerPath, "copying", true);
            DeleteFilesByName(bowerPath, "license", true);
            DeleteFilesByName(bowerPath, "license-mit", true);
            DeleteFilesByName(bowerPath, "gitignore");
            DeleteFilesByName(bowerPath, "npmignore");
            DeleteFilesByName(bowerPath, "jshintrc");
            DeleteFilesByName(bowerPath, "gruntfile");
            DeleteFilesByName(bowerPath, "bowerrc");
            DeleteFilesByName(bowerPath, "jscsrc");
            DeleteFilesByName(bowerPath, "hero.svg");
            DeleteFilesByName(bowerPath, "travis.yml");
            DeleteFilesByName(bowerPath, "build.js");
            DeleteFilesByName(bowerPath, "editorconfig");
            DeleteFilesByName(bowerPath, "gitattributes");
            DeleteFoldersByName(bowerPath, "demo");
            DeleteFoldersByName(bowerPath, "test");
            DeleteFoldersByName(bowerPath, "guides");
            DeleteFoldersByName(bowerPath, "grunt");
            DeleteFoldersByName(bowerPath, "rollups");

            _fileSystem.DeleteDirectory(Path.Combine(bowerPath, "jquery", "external"), true);
            _fileSystem.DeleteDirectory(Path.Combine(bowerPath, "jquery", "src"), true);
          
            DeleteCryptoFiles(Path.Combine(bowerPath, "cryptojslib", "components"));

            DeleteFoldersByName(Path.Combine(bowerPath, "jquery"), "src");
            DeleteFoldersByName(Path.Combine(bowerPath, "jstree"), "src");
            DeleteFoldersByName(Path.Combine(bowerPath, "Sortable"), "meteor");
            DeleteFoldersByName(Path.Combine(bowerPath, "Sortable"), "st");
            DeleteFoldersByName(Path.Combine(bowerPath, "swipebox"), "lib");
            DeleteFoldersByName(Path.Combine(bowerPath, "swipebox"), "scss");

            if (string.Equals(mode, "cordova", StringComparison.OrdinalIgnoreCase))
            {
                // Delete things that are unneeded in an attempt to keep the output as trim as possible
                _fileSystem.DeleteDirectory(Path.Combine(path, "css", "images", "tour"), true);

                _fileSystem.DeleteFile(Path.Combine(path, "thirdparty", "jquerymobile-1.4.5", "jquery.mobile-1.4.5.min.map"));
            }
            else
            {
                MinifyCssDirectory(path);
                MinifyJsDirectory(path);
            }

            await DumpHtml(creator.DashboardUIPath, path, mode, culture, appVersion);

            await DumpFile("css/all.css", Path.Combine(path, "css", "all.css"), mode, culture, appVersion).ConfigureAwait(false);

            return "";
        }

        private void DeleteCryptoFiles(string path)
        {
            var files = _fileSystem.GetFiles(path)
                .ToList();

            var keepFiles = new[] { "core-min.js", "md5-min.js", "sha1-min.js" };

            foreach (var file in files)
            {
                if (!keepFiles.Contains(file.Name, StringComparer.OrdinalIgnoreCase))
                {
                    _fileSystem.DeleteFile(file.FullName);
                }
            }
        }

        private void DeleteFilesByExtension(string path, string extension)
        {
            var files = _fileSystem.GetFiles(path, true)
                .Where(i => string.Equals(i.Extension, extension, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var file in files)
            {
                _fileSystem.DeleteFile(file.FullName);
            }
        }

        private void DeleteFilesByName(string path, string name, bool exact = false)
        {
            var files = _fileSystem.GetFiles(path, true)
                .Where(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase) || (!exact && i.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) != -1))
                .ToList();

            foreach (var file in files)
            {
                _fileSystem.DeleteFile(file.FullName);
            }
        }

        private void DeleteFoldersByName(string path, string name)
        {
            var directories = _fileSystem.GetDirectories(path, true)
                .Where(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var directory in directories)
            {
                _fileSystem.DeleteDirectory(directory.FullName, true);
            }
        }

        private void MinifyCssDirectory(string path)
        {
            foreach (var file in Directory.GetFiles(path, "*.css", SearchOption.AllDirectories))
            {
                if (file.IndexOf(".min.", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    continue;
                }
                if (file.IndexOf("bower_", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    continue;
                }

                try
                {
                    var text = _fileSystem.ReadAllText(file, Encoding.UTF8);

                    var result = new KristensenCssMinifier().Minify(text, false, Encoding.UTF8);

                    if (result.Errors.Count > 0)
                    {
                        Logger.Error("Error minifying css: " + result.Errors[0].Message);
                    }
                    else
                    {
                        text = result.MinifiedContent;
                        _fileSystem.WriteAllText(file, text, Encoding.UTF8);
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("Error minifying css", ex);
                }
            }
        }

        private void MinifyJsDirectory(string path)
        {
            foreach (var file in Directory.GetFiles(path, "*.js", SearchOption.AllDirectories))
            {
                if (file.IndexOf(".min.", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    continue;
                }
                if (file.IndexOf("bower_", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    continue;
                }

                try
                {
                    var text = _fileSystem.ReadAllText(file, Encoding.UTF8);

                    var result = new CrockfordJsMinifier().Minify(text, false, Encoding.UTF8);

                    if (result.Errors.Count > 0)
                    {
                        Logger.Error("Error minifying javascript: " + result.Errors[0].Message);
                    }
                    else
                    {
                        text = result.MinifiedContent;
                        _fileSystem.WriteAllText(file, text, Encoding.UTF8);
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorException("Error minifying css", ex);
                }
            }
        }

        private async Task DumpHtml(string source, string destination, string mode, string culture, string appVersion)
        {
            foreach (var file in Directory.GetFiles(source, "*", SearchOption.TopDirectoryOnly))
            {
                var filename = Path.GetFileName(file);

                await DumpFile(filename, Path.Combine(destination, filename), mode, culture, appVersion).ConfigureAwait(false);
            }

            var excludeFiles = new List<string>();

            if (string.Equals(mode, "cordova", StringComparison.OrdinalIgnoreCase))
            {
                excludeFiles.Add("supporterkey.html");
            }

            foreach (var file in excludeFiles)
            {
                _fileSystem.DeleteFile(Path.Combine(destination, file));
            }
        }

        private async Task DumpFile(string resourceVirtualPath, string destinationFilePath, string mode, string culture, string appVersion)
        {
            using (var stream = await GetPackageCreator().GetResource(resourceVirtualPath, mode, culture, appVersion, true).ConfigureAwait(false))
            {
                using (var fs = _fileSystem.GetFileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    stream.CopyTo(fs);
                }
            }
        }

        private void CopyDirectory(string source, string destination)
        {
            _fileSystem.CreateDirectory(destination);

            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(source, "*",
                SearchOption.AllDirectories))
                _fileSystem.CreateDirectory(dirPath.Replace(source, destination));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(source, "*.*",
                SearchOption.AllDirectories))
                _fileSystem.CopyFile(newPath, newPath.Replace(source, destination), true);
        }
    }

}
