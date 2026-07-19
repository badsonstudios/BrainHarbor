using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BrainHarbor.Web.Identity;

/// <summary>
/// WI-207: Identity storage for the single admin account. Deliberately the
/// only EF Core context in the app — everything else uses Dapper. Identity's
/// schema is not worth hand-rolling, and getting password hashing or token
/// storage subtly wrong is exactly the sort of thing that ends badly.
///
/// Tables live under the `identity` schema so DbUp's plain-SQL migrations and
/// EF's Identity tables never collide.
/// </summary>
public sealed class AdminDbContext(DbContextOptions<AdminDbContext> options)
    : IdentityDbContext<IdentityUser>(options)
{
    public const string SchemaName = "identity";

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema(SchemaName);
    }
}
