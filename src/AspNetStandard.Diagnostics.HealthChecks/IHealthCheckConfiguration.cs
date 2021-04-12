﻿using AspNetStandard.Diagnostics.HealthChecks.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AspNetStandard.Diagnostics.HealthChecks
{
    public interface IHealthCheckConfiguration
    {
        IDictionary<HealthStatus, HttpStatusCode> ResultStatusCodes { get; }
        IDictionary<string, Registration> HealthChecksDependencies { get; }
        string ApiKey { get; set; }
        JsonSerializerSettings SerializerSettings { get; set; }
        HttpStatusCode GetStatusCode(HealthStatus healthstatus);
    }
}