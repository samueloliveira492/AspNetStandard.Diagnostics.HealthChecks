﻿using AspNetStandard.Diagnostics.HealthChecks.Entities;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetStandard.Diagnostics.HealthChecks.Redis
{
    public class RedisHealthCheck : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> _connections = new ConcurrentDictionary<string, ConnectionMultiplexer>();
        private readonly string _redisConnectionString;

        public RedisHealthCheck(string redisConnectionString)
        {
            _redisConnectionString = redisConnectionString ?? throw new ArgumentNullException(nameof(redisConnectionString));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_connections.TryGetValue(_redisConnectionString, out ConnectionMultiplexer connection))
                {
                    connection = await ConnectionMultiplexer.ConnectAsync(_redisConnectionString);

                    if (!_connections.TryAdd(_redisConnectionString, connection))
                    {
                        connection.Dispose();
                        connection = _connections[_redisConnectionString]; 
                    }

                }

                foreach (var endPoint in connection.GetEndPoints(configuredOnly: true))
                {
                    var server = connection.GetServer(endPoint);

                    if (server.ServerType != ServerType.Cluster)
                    {
                        await connection.GetDatabase().PingAsync();
                        await server.PingAsync();
                    }
                    else
                    {
                        var clusterInfo = await server.ExecuteAsync("CLUSTER", "INFO");
                        if (clusterInfo is object && !clusterInfo.IsNull)
                        {
                            if (!clusterInfo.ToString().Contains("cluster_state:ok"))
                            {
                                return new HealthCheckResult(HealthStatus.Unhealthy, description: $"INFO CLUSTER is not on OK state for endpoint {endPoint}");
                            }
                        }
                        else
                        {
                            return new HealthCheckResult(HealthStatus.Unhealthy, description: $"INFO CLUSTER is null or can't be read for endpoint {endPoint}");                            
                        }
                    }
                }

                return new HealthCheckResult(HealthStatus.Healthy, "Redis is healthy");
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(HealthStatus.Unhealthy, "Redis is unhealthy", exception: ex);
            }
        }
    }
}
