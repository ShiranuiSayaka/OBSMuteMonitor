using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;

namespace Mute_OBS_WS
{
    public class OBSMonitorWorker : BackgroundService
    {
        private readonly ILogger<OBSMonitorWorker> _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly OBSWebsocket _obs;
        public OBSMonitorWorker(ILogger<OBSMonitorWorker> logger, IHostApplicationLifetime appLifetime)
        {
            _logger = logger;
            _appLifetime = appLifetime;
            _obs = new OBSWebsocket();
        }

        private void onConnected(object sender, EventArgs e)
            => _logger.LogInformation($"Connected OBS Version {_obs.GetVersion().OBSStudioVersion}");

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //_obs.SourceMuteStateChanged += 
            _obs.Connected += this.onConnected;
            try
            {
                _obs.Connect("ws://localhost:60019", "aaa");
            }
            catch (AuthFailureException)
            {
                _logger.LogCritical("Authentication failed.");
                _appLifetime.StopApplication();
            }
            catch (ErrorResponseException ex)
            {
                _logger.LogCritical("Connect failed : " + ex.Message);
                _appLifetime.StopApplication();
            }
            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
