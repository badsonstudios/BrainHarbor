using System.Data.Common;
using Npgsql;

namespace BrainHarbor.Web.Services;

public sealed class NpgsqlConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
    public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        => await dataSource.OpenConnectionAsync(cancellationToken);
}
