using System.Collections;
using Raven.Client.Documents.Session;
using YuckQi.Data.Filtering;

namespace YuckQi.Data.DocumentDb.RavenDb.Extensions;

public static class DocumentQueryExtensions
{
    public static IAsyncDocumentQuery<TDocument> ApplyFilter<TDocument>(this IAsyncDocumentQuery<TDocument> query, IEnumerable<FilterCriteria>? parameters)
    {
        if (parameters is null)
            return query;

        foreach (var parameter in parameters)
        {
            if (parameter is { Operation: FilterOperation.In, Value: not IEnumerable })
                throw new ArgumentException($"{nameof(parameter.Value)} must be convertible to {nameof(IEnumerable)}.");

            var set = (parameter.Value as IEnumerable)?.Cast<Object>();

            query = parameter.Operation switch
            {
                FilterOperation.Equal => query.WhereEquals(parameter.FieldName, parameter.Value),
                FilterOperation.GreaterThan => query.WhereGreaterThan(parameter.FieldName, parameter.Value),
                FilterOperation.GreaterThanOrEqual => query.WhereGreaterThanOrEqual(parameter.FieldName, parameter.Value),
                FilterOperation.In => query.WhereIn(parameter.FieldName, set),
                FilterOperation.LessThan => query.WhereLessThan(parameter.FieldName, parameter.Value),
                FilterOperation.LessThanOrEqual => query.WhereLessThanOrEqual(parameter.FieldName, parameter.Value),
                FilterOperation.NotEqual => query.WhereNotEquals(parameter.FieldName, parameter.Value),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        return query;
    }

    public static IDocumentQuery<TDocument> ApplyFilter<TDocument>(this IDocumentQuery<TDocument> query, IEnumerable<FilterCriteria>? parameters)
    {
        if (parameters is null)
            return query;

        foreach (var parameter in parameters)
        {
            if (parameter is { Operation: FilterOperation.In, Value: not IEnumerable })
                throw new ArgumentException($"{nameof(parameter.Value)} must be convertible to {nameof(IEnumerable)}.");

            var set = (parameter.Value as IEnumerable)?.Cast<Object>();

            query = parameter.Operation switch
            {
                FilterOperation.Equal => query.WhereEquals(parameter.FieldName, parameter.Value),
                FilterOperation.GreaterThan => query.WhereGreaterThan(parameter.FieldName, parameter.Value),
                FilterOperation.GreaterThanOrEqual => query.WhereGreaterThanOrEqual(parameter.FieldName, parameter.Value),
                FilterOperation.In => query.WhereIn(parameter.FieldName, set),
                FilterOperation.LessThan => query.WhereLessThan(parameter.FieldName, parameter.Value),
                FilterOperation.LessThanOrEqual => query.WhereLessThanOrEqual(parameter.FieldName, parameter.Value),
                FilterOperation.NotEqual => query.WhereNotEquals(parameter.FieldName, parameter.Value),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        return query;
    }
}
