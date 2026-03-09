using System.Collections;
using Raven.Client.Documents.Session;
using YuckQi.Data.Filtering;

namespace YuckQi.Data.DocumentDb.RavenDb.Extensions;

public static class DocumentQueryExtensions
{
    public static IDocumentQuery<TDocument> ApplyFilter<TDocument>(this IDocumentQuery<TDocument> query, IEnumerable<FilterCriteria>? parameters)
    {
        if (parameters is null)
            return query;

        foreach (var parameter in parameters)
        {
            if (parameter is { Operation: FilterOperation.In, Value: not IEnumerable })
                throw new ArgumentException($"{nameof(parameter.Value)} must be convertible to {nameof(IEnumerable)}.");

            var set = (parameter.Value as IEnumerable)?.Cast<Object>();

            switch (parameter.Operation)
            {
                case FilterOperation.Equal:
                    query = query.WhereEquals(parameter.FieldName, parameter.Value);
                    break;
                case FilterOperation.GreaterThan:
                    query = query.WhereGreaterThan(parameter.FieldName, parameter.Value);
                    break;
                case FilterOperation.GreaterThanOrEqual:
                    query = query.WhereGreaterThanOrEqual(parameter.FieldName, parameter.Value);
                    break;
                case FilterOperation.In:
                    query = query.WhereIn(parameter.FieldName, set);
                    break;
                case FilterOperation.LessThan:
                    query = query.WhereLessThan(parameter.FieldName, parameter.Value);
                    break;
                case FilterOperation.LessThanOrEqual:
                    query = query.WhereLessThanOrEqual(parameter.FieldName, parameter.Value);
                    break;
                case FilterOperation.NotEqual:
                    query = query.WhereNotEquals(parameter.FieldName, parameter.Value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return query;
    }

    public static IAsyncDocumentQuery<TDocument> ApplyFilter<TDocument>(this IAsyncDocumentQuery<TDocument> query, IEnumerable<FilterCriteria>? parameters)
    {
        if (parameters is null)
            return query;

        foreach (var parameter in parameters)
        {
            if (parameter is { Operation: FilterOperation.In, Value: not IEnumerable })
                throw new ArgumentException($"{nameof(parameter.Value)} must be convertible to {nameof(IEnumerable)}.");

            var set = (parameter.Value as IEnumerable)?.Cast<Object>();

            switch (parameter.Operation)
            {
                case FilterOperation.Equal:
                    query = query.WhereEquals(parameter.FieldName, parameter.Value);
                    break;
                case FilterOperation.GreaterThan:
                    query = query.WhereGreaterThan(parameter.FieldName, parameter.Value);
                    break;
                case FilterOperation.GreaterThanOrEqual:
                    query = query.WhereGreaterThanOrEqual(parameter.FieldName, parameter.Value);
                    break;
                case FilterOperation.In:
                    query = query.WhereIn(parameter.FieldName, set);
                    break;
                case FilterOperation.LessThan:
                    query = query.WhereLessThan(parameter.FieldName, parameter.Value);
                    break;
                case FilterOperation.LessThanOrEqual:
                    query = query.WhereLessThanOrEqual(parameter.FieldName, parameter.Value);
                    break;
                case FilterOperation.NotEqual:
                    query = query.WhereNotEquals(parameter.FieldName, parameter.Value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return query;
    }
}
