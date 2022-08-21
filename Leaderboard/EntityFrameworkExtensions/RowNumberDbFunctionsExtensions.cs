using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

/**
 * Copyright (c) 2021, Pawel Gerr. All rights reserved.
 *
 * See LICENSE.md for full license.
 */

namespace Leaderboard.EntityFrameworkExtensions;

public static class RelationalDbFunctionsExtensions
{
    /// <summary>
    /// Definition of the ORDER BY clause of a the ROW_NUMBER expression.
    /// </summary>
    /// <remarks>
    /// This method is for use with Entity Framework Core only and has no in-memory implementation.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
    public static RowNumberOrderByClause OrderBy<T>(this DbFunctions _, T _column)
    {
        throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
    }

    /// <summary>
    /// Definition of the ROW_NUMBER.
    /// <remarks>
    /// This method is for use with Entity Framework Core only and has no in-memory implementation.
    /// </remarks>
    /// </summary>
    /// <param name="_">An instance of <see cref="DbFunctions"/>.</param>
    /// <param name="orderBy">A column or an object containing columns to order by.</param>
    /// <exception cref="InvalidOperationException">Is thrown if executed in-memory.</exception>
    public static long RowNumber(this DbFunctions _, RowNumberOrderByClause _orderBy)
    {
        throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
    }
}

public static class RelationalDbContextOptionsBuilderExtensions
{

}

public sealed class RelationalMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
{
    /// <inheritdoc />
    public IEnumerable<IMethodCallTranslator> Translators { get; }

    /// <summary>
    /// Initializes new instance of <see cref="RelationalMethodCallTranslatorPlugin"/>.
    /// </summary>
    public RelationalMethodCallTranslatorPlugin(ISqlExpressionFactory sqlExpressionFactory)
    {
        Translators = new List<IMethodCallTranslator>
        {
            new RowNumberTranslator(sqlExpressionFactory)
        };
    }
}

public static class RelationalDbFunctionsServicesCollectionExtensions
{
    public static void AddRelationalDbFunctions(this IServiceCollection services)
    {
        var pluginLifetime = GetLifetime<IMethodCallTranslatorPlugin>();
        services.Add<IMethodCallTranslatorPlugin, RelationalMethodCallTranslatorPlugin>(pluginLifetime);
    }

    /// <summary>
    /// Gets the lifetime of a Entity Framework Core service.
    /// </summary>
    /// <typeparam name="TService">Service to fetch lifetime for.</typeparam>
    /// <returns>Lifetime of the provided service.</returns>
    /// <exception cref="InvalidOperationException">If service is not found.</exception>
    [SuppressMessage("Usage", "EF1001", MessageId = "Internal EF Core API usage.")]
    private static ServiceLifetime GetLifetime<TService>()
    {
        var serviceType = typeof(TService);

        if (EntityFrameworkRelationalServicesBuilder.RelationalServices.TryGetValue(serviceType, out var serviceCharacteristics) ||
            EntityFrameworkServicesBuilder.CoreServices.TryGetValue(serviceType, out serviceCharacteristics))
            return serviceCharacteristics.Lifetime;

        throw new InvalidOperationException($"No service characteristics for service '{serviceType.Name}' found.");
    }
}

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class RelationalServiceCollectionExtensions
{
    /// <summary>
    /// Adds a service of the type specified in <typeparamref name="TService" /> with an
    /// implementation type specified in <typeparamref name="TImplementation" /> to the
    /// <paramref name="services"/> if the service type hasn't already been registered.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <param name="lifetime">Lifetime of the service.</param>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <c>null</c>.</exception>
    public static void TryAdd<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime)
       where TImplementation : TService
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(ServiceDescriptor.Describe(typeof(TService), typeof(TImplementation), lifetime));
    }

    /// <summary>
    /// Adds a service of the type specified in <typeparamref name="TService" /> with an
    /// implementation type specified in <typeparamref name="TImplementation" /> to the
    /// <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <param name="lifetime">Lifetime of the service.</param>
    /// <typeparam name="TService">The type of the service to add.</typeparam>
    /// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <c>null</c>.</exception>
    public static void Add<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime)
       where TImplementation : TService
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Add(ServiceDescriptor.Describe(typeof(TService), typeof(TImplementation), lifetime));
    }
}

/// <summary>
/// Translated extension method "RowNumber"
/// </summary>
public sealed class RowNumberTranslator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    internal RowNumberTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <inheritdoc />
    public SqlExpression? Translate(
       SqlExpression? instance,
       MethodInfo method,
       IReadOnlyList<SqlExpression> arguments,
       IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(arguments);

        if (method.DeclaringType != typeof(RelationalDbFunctionsExtensions))
            return null;

        switch (method.Name)
        {
            //case nameof(RelationalDbFunctionsExtensions.OrderBy):
            //    {
            //        var orderBy = arguments.Skip(1).Select(e => new OrderingExpression(_sqlExpressionFactory.ApplyDefaultTypeMapping(e), true)).ToList();
            //        return new RowNumberClauseOrderingsExpression(orderBy);
            //    }
            //case nameof(RelationalDbFunctionsExtensions.OrderByDescending):
            //    {
            //        var orderBy = arguments.Skip(1).Select(e => new OrderingExpression(_sqlExpressionFactory.ApplyDefaultTypeMapping(e), false)).ToList();
            //        return new RowNumberClauseOrderingsExpression(orderBy);
            //    }
            //case nameof(RelationalDbFunctionsExtensions.ThenBy):
            //    {
            //        var orderBy = arguments.Skip(1).Select(e => new OrderingExpression(_sqlExpressionFactory.ApplyDefaultTypeMapping(e), true));
            //        return ((RowNumberClauseOrderingsExpression)arguments[0]).AddColumns(orderBy);
            //    }
            //case nameof(RelationalDbFunctionsExtensions.ThenByDescending):
            //    {
            //        var orderBy = arguments.Skip(1).Select(e => new OrderingExpression(_sqlExpressionFactory.ApplyDefaultTypeMapping(e), false));
            //        return ((RowNumberClauseOrderingsExpression)arguments[0]).AddColumns(orderBy);
            //    }
            case nameof(RelationalDbFunctionsExtensions.RowNumber):
                {
                    var partitionBy = arguments
                        .Skip(1)
                        .Take(arguments.Count - 2)
                        .Select(
                            e => _sqlExpressionFactory.ApplyDefaultTypeMapping(e)
                        )
                        .ToList();
                    var orderings = (RowNumberClauseOrderingsExpression)arguments[^1];
                    return new RowNumberExpression(partitionBy, orderings.Orderings, RelationalTypeMapping.NullMapping);
                }
            default:
                throw new InvalidOperationException($"Unexpected method '{method.Name}' in '{nameof(RelationalDbFunctionsExtensions)}'.");
        }
    }
}

/// <summary>
/// Helper class to attach extension methods to.
/// </summary>
public sealed class RowNumberOrderByClause
{
    private RowNumberOrderByClause()
    {
    }
}


/// <summary>
/// Accumulator for orderings.
/// </summary>
public sealed class RowNumberClauseOrderingsExpression : SqlExpression
{
    /// <summary>
    /// Orderings.
    /// </summary>
    public IReadOnlyList<OrderingExpression> Orderings { get; }

    /// <inheritdoc />
    public RowNumberClauseOrderingsExpression(IReadOnlyList<OrderingExpression> orderings)
       : base(typeof(RowNumberOrderByClause), RelationalTypeMapping.NullMapping)
    {
        Orderings = orderings ?? throw new ArgumentNullException(nameof(orderings));
    }

    /// <inheritdoc />
    protected override Expression Accept(ExpressionVisitor visitor)
    {
        if (visitor is QuerySqlGenerator)
            throw new NotSupportedException($"The EF function '{nameof(RelationalDbFunctionsExtensions.RowNumber)}' contains some expressions not supported by the Entity Framework. One of the reason is the creation of new objects like: 'new {{ e.MyProperty, e.MyOtherProperty }}'.");

        return base.Accept(visitor);
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var visited = visitor.VisitExpressions(Orderings);

        return ReferenceEquals(visited, Orderings) ? this : new RowNumberClauseOrderingsExpression(visited);
    }

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        ArgumentNullException.ThrowIfNull(expressionPrinter);

        expressionPrinter.VisitCollection(Orderings);
    }

    /// <summary>
    /// Adds provided <paramref name="orderings"/> to existing <see cref="Orderings"/> and returns a new <see cref="RowNumberClauseOrderingsExpression"/>.
    /// </summary>
    /// <param name="orderings">Orderings to add.</param>
    /// <returns>New instance of <see cref="RowNumberClauseOrderingsExpression"/>.</returns>
    public RowNumberClauseOrderingsExpression AddColumns(IEnumerable<OrderingExpression> orderings)
    {
        ArgumentNullException.ThrowIfNull(orderings);

        return new RowNumberClauseOrderingsExpression(Orderings.Concat(orderings).ToList());
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj != null && (ReferenceEquals(this, obj) || Equals(obj as RowNumberClauseOrderingsExpression));
    }

    private bool Equals(RowNumberClauseOrderingsExpression? expression)
    {
        return base.Equals(expression) && Orderings.SequenceEqual(expression.Orderings);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());

        for (var i = 0; i < Orderings.Count; i++)
        {
            hash.Add(Orderings[i]);
        }

        return hash.ToHashCode();
    }
}

/// <summary>
/// Extension methods for <see cref="ExpressionVisitor"/>.
/// </summary>
public static class RelationalExpressionVisitorExtensions
{
    /// <summary>
    /// Visits a collection of <paramref name="expressions"/> and returns new collection if it least one expression has been changed.
    /// Otherwise the provided <paramref name="expressions"/> are returned if there are no changes.
    /// </summary>
    /// <param name="visitor">Visitor to use.</param>
    /// <param name="expressions">Expressions to visit.</param>
    /// <returns>
    /// New collection with visited expressions if at least one visited expression has been changed; otherwise the provided <paramref name="expressions"/>.
    /// </returns>
    public static IReadOnlyList<T> VisitExpressions<T>(this ExpressionVisitor visitor, IReadOnlyList<T> expressions)
       where T : Expression
    {
        ArgumentNullException.ThrowIfNull(visitor);
        ArgumentNullException.ThrowIfNull(expressions);

        var visitedExpressions = new List<T>();
        var hasChanges = false;

        foreach (var expression in expressions)
        {
            var visitedExpression = (T)visitor.Visit(expression);
            visitedExpressions.Add(visitedExpression);
            hasChanges |= !ReferenceEquals(visitedExpression, expression);
        }

        return hasChanges ? visitedExpressions.AsReadOnly() : expressions;
    }
}
