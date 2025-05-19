// HealthChecks/DatabaseHealthCheck.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KylesBackendAPI.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        // Add any dependencies here, like DbContext
        
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Perform actual check against database
                // For example: await _dbContext.Database.CanConnectAsync(cancellationToken);
                
                return HealthCheckResult.Healthy("Database connection is healthy");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Database connection failed", ex);
            }
        }
    }
}