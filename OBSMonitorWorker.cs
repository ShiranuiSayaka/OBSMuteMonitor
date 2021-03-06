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

        private async void OnSourceMuteStateChanged(OBSWebsocket sender, string sourceName, bool muted)
        {
            try
            {
                if (sourceName == "マイク" && muted)
                {
                    await Task.Run(() =>
                    {
                        _obs.SetSourceRender("Muted_Icon", visible: true);
                    });
                }
                if (sourceName == "マイク" && !muted)
                {
                    await Task.Run(() =>
                    {
                        _obs.SetSourceRender("Muted_Icon", visible: false);
                    });
                }
                _logger.LogDebug($"Change Mute State at {sourceName} to {muted}. {DateTimeOffset.Now}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private void OnConnected(object sender, EventArgs e)
            => _logger.LogInformation($"Connected OBS Version {_obs.GetVersion().OBSStudioVersion}");

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _obs.Connected += this.OnConnected;
            _obs.SourceMuteStateChanged += this.OnSourceMuteStateChanged;
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
            await Task.CompletedTask;
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _obs.Disconnect();
            await Task.CompletedTask;
        }
    }
}