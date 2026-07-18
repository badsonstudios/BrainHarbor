using System.Data.Common;

namespace BrainHarbor.Web.Services;

public interface IDbConnectionFactory
{
    Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}
