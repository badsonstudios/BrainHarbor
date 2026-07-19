using System.Data;
using Dapper;

namespace BrainHarbor.Web.Services;

/// <summary>
/// Dapper has no built-in mapping for DateOnly, which the schema uses for
/// published_at (a publication date has no time or zone — keeping it DateOnly
/// stops "was this yesterday?" timezone bugs in the feed). Registered once at
/// startup.
/// </summary>
public sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }

    public override DateOnly Parse(object value) => value switch
    {
        DateOnly dateOnly => dateOnly,
        DateTime dateTime => DateOnly.FromDateTime(dateTime),
        string text => DateOnly.Parse(text),
        _ => throw new InvalidCastException($"Cannot convert {value.GetType()} to DateOnly."),
    };
}

public static class DapperTypeHandlers
{
    public static void Register()
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
    }
}
