# WebApi.HealthChecks

WebApi.HealthChecks offers a **WebApi** implementation of the health check endpoints for reporting the health of app infrastructure components.

[![Build status](https://ci.appveyor.com/api/projects/status/1g00xtolkwtlt6kh?svg=true)](https://ci.appveyor.com/project/kpol/webapi-healthchecks)

The package is available on [**NuGet**](https://nuget.org/packages/WebApi.HealthChecks)

Health checks are exposed by an app as HTTP endpoints.
Supports two endpoints: 
- `/health`
- `/health/ui?check={check-name}`


By default the health check endpoint is created at `/health`
```
public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
    {
        config.AddHealthChecks()
            .AddCheck("Sql Db", new SqlHealthCheck())
            .AddCheck("Cosmos Db", new CosmosDbHealthCheck());
    }
}
```

Every health check must implement `IHealthCheck` interface
```
public interface IHealthCheck
{
    Task<HealthCheckResult> CheckHealthAsync();
}
```
The framework supports three statuses: `Unhealthy` , `Degraded` and `Healthy`.

`GET /health` returns json in the following format:
```
{
  "status": "Unhealthy",
  "totalResponseTime": 0,
  "entries": {
    "sql Db": {
      "responseTime": 8,
      "status": "Healthy"
    },
    "cosmos Db": {
      "responseTime": 5,
      "status": "Degraded"
    }
  }
}
```
The `/health/ui?check={check-name}` endpoint returns a SVG badge which shows individual status of the service component.
For example `/health/ui?check=sql db` will output this image: ![degraded](/src/WebApi.HealthChecks/Content/status-degraded-lightgrey.svg)
