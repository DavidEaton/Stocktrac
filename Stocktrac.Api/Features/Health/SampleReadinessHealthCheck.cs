using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Stocktrac.Api.Features.Health
{
    public sealed class SampleReadinessHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            // Replace this with real dependency checks:
            // database reachable, storage reachable, required config present, etc.

            var ready = true;

            return Task.FromResult(
                ready
                    ? HealthCheckResult.Healthy("Application dependencies are available.")
                    : HealthCheckResult.Unhealthy("Application dependencies are unavailable."));
        }
    }
}