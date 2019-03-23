﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetworkVisualizer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkVisualizer.Services
{
    internal class DataGenerationService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;
        private Timer _timer;

        public DataGenerationService(ILogger<PruneDatabaseService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        // Every hour generate a set of fake data
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.FromMinutes(1),
                TimeSpan.FromHours(1));

            return Task.CompletedTask;
        }

        private static List<string> Domains = new List<string>
        {
            "microsoft.com",
            "azure.com",
            "github.com",
            "github.io",
            "google.com",
            "youtube.com"
        };
        private static List<string> Types = new List<string>
        {
            "HTTPS",
            "HTTP",
            "UDP",
            "TCP",
            "DNS"
        };

        // Generate random amount of packets between 60 and 120, with random data
        private void DoWork(object state)
        {
            // Check config to see if enabled, if not, return and do nothing
            if (!Config.config.DataGenerationEnabled)
                return;

            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<NetworkVisualizerContext>();
                
                Random rnd = new Random();

                // Generate random packets and add to context
                int amount = rnd.Next(60, 120);
                for (int i = 0; i < amount; i++)
                {
                    Packet packet = new Packet
                    {
                        DateTime = DateTime.UtcNow.Subtract(TimeSpan.FromHours(7)),
                        PacketType = Types[rnd.Next(0, Types.Count)],
                        DestinationHostname = Domains[rnd.Next(0, Domains.Count)],
                        OriginHostname = "DataGenerationService"
                    };
                    _context.Add(packet);
                }

                // Save changes
                _context.SaveChanges();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}