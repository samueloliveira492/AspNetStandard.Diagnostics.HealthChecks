﻿using AspNetStandard.Diagnostics.HealthChecks.HttpMessageHandlers;
using AspNetStandard.Diagnostics.HealthChecks.Services;
using System.Web.Http;

namespace AspNetStandard.Diagnostics.HealthChecks
{
    public static class HttpConfigurationExtensions
    {
        public static HealthChecksBuilder AddHealthChecks(this HttpConfiguration httpConfiguration, string healthEndpoint = "health")
        {
            var hcBuilder = new HealthChecksBuilder();
            var dependencyResolver = httpConfiguration.DependencyResolver;
            var hcConfig = hcBuilder.HealthCheckConfig;

            // Service Instances
            var healthChecksService = new HealthCheckService(dependencyResolver, hcConfig.HealthChecksDependencies);
            var authenticationService = new AuthenticationService(hcConfig);

            // Handler Instances
            var authenticationHandler = new AuthenticationHandler(hcConfig, authenticationService);
            var healthCheckHandler = new HealthCheckHandler(hcConfig, healthChecksService);

            // ChainOfResponsibility
            authenticationHandler.SetNextHandler(healthCheckHandler);

            httpConfiguration.Routes.MapHttpRoute(
                name: "health_check",
                routeTemplate: healthEndpoint,
                defaults: new { check = RouteParameter.Optional },
                constraints: null,
                handler: authenticationHandler
            );

            return hcBuilder;
        }
    }
}