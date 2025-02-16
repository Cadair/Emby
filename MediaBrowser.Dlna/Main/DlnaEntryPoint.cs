﻿using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Dlna;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Localization;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using MediaBrowser.Dlna.Channels;
using MediaBrowser.Dlna.PlayTo;
using MediaBrowser.Dlna.Ssdp;
using MediaBrowser.Model.Logging;
using System;
using System.Collections.Generic;
using System.Net;

namespace MediaBrowser.Dlna.Main
{
    public class DlnaEntryPoint : IServerEntryPoint
    {
        private readonly IServerConfigurationManager _config;
        private readonly ILogger _logger;
        private readonly IServerApplicationHost _appHost;
        private readonly INetworkManager _network;

        private PlayToManager _manager;
        private readonly ISessionManager _sessionManager;
        private readonly IHttpClient _httpClient;
        private readonly ILibraryManager _libraryManager;
        private readonly IUserManager _userManager;
        private readonly IDlnaManager _dlnaManager;
        private readonly IImageProcessor _imageProcessor;
        private readonly IUserDataManager _userDataManager;
        private readonly ILocalizationManager _localization;
        private readonly IMediaSourceManager _mediaSourceManager;

        private readonly SsdpHandler _ssdpHandler;
        private readonly IDeviceDiscovery _deviceDiscovery;

        private readonly List<string> _registeredServerIds = new List<string>();
        private bool _dlnaServerStarted;

        public DlnaEntryPoint(IServerConfigurationManager config,
            ILogManager logManager,
            IServerApplicationHost appHost,
            INetworkManager network,
            ISessionManager sessionManager,
            IHttpClient httpClient,
            ILibraryManager libraryManager,
            IUserManager userManager,
            IDlnaManager dlnaManager,
            IImageProcessor imageProcessor,
            IUserDataManager userDataManager,
            ILocalizationManager localization,
            IMediaSourceManager mediaSourceManager,
            ISsdpHandler ssdpHandler, IDeviceDiscovery deviceDiscovery)
        {
            _config = config;
            _appHost = appHost;
            _network = network;
            _sessionManager = sessionManager;
            _httpClient = httpClient;
            _libraryManager = libraryManager;
            _userManager = userManager;
            _dlnaManager = dlnaManager;
            _imageProcessor = imageProcessor;
            _userDataManager = userDataManager;
            _localization = localization;
            _mediaSourceManager = mediaSourceManager;
            _deviceDiscovery = deviceDiscovery;
            _ssdpHandler = (SsdpHandler)ssdpHandler;
            _logger = logManager.GetLogger("Dlna");
        }

        public void Run()
        {
            StartSsdpHandler();
            ReloadComponents();

            _config.NamedConfigurationUpdated += _config_NamedConfigurationUpdated;
        }

        void _config_NamedConfigurationUpdated(object sender, ConfigurationUpdateEventArgs e)
        {
            if (string.Equals(e.Key, "dlna", StringComparison.OrdinalIgnoreCase))
            {
                ReloadComponents();
            }
        }

        private void ReloadComponents()
        {
            var isServerStarted = _dlnaServerStarted;

            var options = _config.GetDlnaConfiguration();

            if (options.EnableServer && !isServerStarted)
            {
                StartDlnaServer();
            }
            else if (!options.EnableServer && isServerStarted)
            {
                DisposeDlnaServer();
            }

            var isPlayToStarted = _manager != null;

            if (options.EnablePlayTo && !isPlayToStarted)
            {
                StartPlayToManager();
            }
            else if (!options.EnablePlayTo && isPlayToStarted)
            {
                DisposePlayToManager();
            }
        }

        private void StartSsdpHandler()
        {
            try
            {
                _ssdpHandler.Start();

                ((DeviceDiscovery)_deviceDiscovery).Start(_ssdpHandler);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error starting ssdp handlers", ex);
            }
        }

        public void StartDlnaServer()
        {
            try
            {
                RegisterServerEndpoints();

                _dlnaServerStarted = true;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error registering endpoint", ex);
            }
        }

        private void RegisterServerEndpoints()
        {
            foreach (var address in _appHost.LocalIpAddresses)
            {
                //if (IPAddress.IsLoopback(address))
                //{
                //    // Should we allow this?
                //    continue;
                //}

                var addressString = address.ToString();
                var guid = addressString.GetMD5();

                var descriptorURI = "/dlna/" + guid.ToString("N") + "/description.xml";

                var uri = new Uri(_appHost.GetLocalApiUrl(addressString) + descriptorURI);

                var services = new List<string>
                {
                    "upnp:rootdevice", 
                    "urn:schemas-upnp-org:device:MediaServer:1", 
                    "urn:schemas-upnp-org:service:ContentDirectory:1", 
                    "urn:schemas-upnp-org:service:ConnectionManager:1",
                    "urn:microsoft.com:service:X_MS_MediaReceiverRegistrar:1",
                    "uuid:" + guid.ToString("N")
                };

                _ssdpHandler.RegisterNotification(guid, uri, address, services);

                _registeredServerIds.Add(guid.ToString("N"));
            }
        }

        private readonly object _syncLock = new object();
        private void StartPlayToManager()
        {
            lock (_syncLock)
            {
                try
                {
                    _manager = new PlayToManager(_logger,
                        _sessionManager,
                        _libraryManager,
                        _userManager,
                        _dlnaManager,
                        _appHost,
                        _imageProcessor,
                        _deviceDiscovery,
                        _httpClient,
                        _config,
                        _userDataManager,
                        _localization,
                        _mediaSourceManager);

                    _manager.Start();
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error starting PlayTo manager", ex);
                }
            }
        }

        private void DisposePlayToManager()
        {
            lock (_syncLock)
            {
                if (_manager != null)
                {
                    try
                    {
                        _manager.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.ErrorException("Error disposing PlayTo manager", ex);
                    }
                    _manager = null;
                }
            }
        }

        public void Dispose()
        {
            DisposeDlnaServer();
            DisposePlayToManager();
        }

        public void DisposeDlnaServer()
        {
            foreach (var id in _registeredServerIds)
            {
                try
                {
                    _ssdpHandler.UnregisterNotification(new Guid(id));
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Error unregistering server", ex);
                }
            }

            _registeredServerIds.Clear();

            _dlnaServerStarted = false;
        }
    }
}
