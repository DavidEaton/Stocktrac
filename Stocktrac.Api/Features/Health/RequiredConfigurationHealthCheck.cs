using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Stocktrac.Api.Features.Health;

public sealed class RequiredConfigurationHealthCheck(IConfiguration configuration) : IHealthCheck
{
    private readonly IConfiguration _configuration = configuration;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(_configuration.GetConnectionString("DefaultConnection")))
        {
            missing.Add("ConnectionStrings:DefaultConnection");
        }

        if (missing.Count > 0)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    $"Missing required configuration: {string.Join(", ", missing)}"));
        }

        return Task.FromResult(
            HealthCheckResult.Healthy("Required configuration is present."));
    }
}