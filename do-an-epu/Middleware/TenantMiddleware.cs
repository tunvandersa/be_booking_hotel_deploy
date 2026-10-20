using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace do_an_epu.Middleware
{
    public sealed class TenantMiddleware(RequestDelegate next,
    IOptions<MultiTenancyOptions> options,
    IConfiguration configuration)
    {
        private readonly MultiTenancyOptions _options = options.Value;
        private readonly IConfiguration _configuration = configuration;

        public async Task InvokeAsync(HttpContext context)
        {
            var tenant = GetTenantFromRequest(context);
            if (!_options.AllowedTenants.Contains(tenant))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync($"Invalid tenant: {tenant}");
                return;
            }

            var connectionString = BuildConnectionString(tenant);
            context.Items["TenantConnectionString"] = connectionString;
            await next(context);
        }
        public string GetTenantFromRequest(HttpContext context)
        {
            var host = context.Request.Host.Host;
            var subdomain = host.Split('.')[0];
              
            if (_options.DomainAuto.Equals("AUTO", StringComparison.OrdinalIgnoreCase))
            {
                return subdomain;
            }
            else
            {
                return _options.DomainAuto; // Hardcoded tenant for local dev mode
            }
        }
        public string BuildConnectionString (string tenant)
        {
            var defaultConnectionString = _configuration.GetConnectionString("Default");
            var builder = new SqlConnectionStringBuilder(defaultConnectionString)
            {
                InitialCatalog = $"{_options.DbPrefix}{tenant}"
            };
            return builder.ConnectionString;
        }

    }

    public sealed class MultiTenancyOptions
    {
        public const string Section = "MultiTenancy";

        /// <summary>
        /// Parity Web.config legacy "DbServer" — informational only (server lấy từ ConnectionStrings:Default).
        /// Giữ field cho backward-compat config; runtime KHÔNG dùng để build connection string.
        /// </summary>
        public string DbServer { get; init; } = default!;

        public string DbPrefix { get; init; } = "QLBH_";

        public string DomainAuto { get; init; } = "AUTO";

        public string[] AllowedTenants { get; init; } = [];
    }
}
