using System.Collections;
using MongoDB.Driver;
using YuckQi.Data.Filtering;

namespace YuckQi.Data.DocumentDb.MongoDb.Filtering.Extensions;

public static class FilterCriteriaExtensions
{
    public static FilterDefinition<TDocument>? ToFilterDefinition<TDocument>(this IEnumerable<FilterCriteria>? parameters)
    {
        if (parameters is null)
            return Builders<TDocument>.Filter.Empty;

        var builder = Builders<TDocument>.Filter;
        var result = new List<FilterDefinition<TDocument>>();

        foreach (var parameter in parameters)
        {
            if (parameter is { Operation: FilterOperation.In, Value: not IEnumerable })
                throw new ArgumentException($"{nameof(parameter.Value)} must be convertible to {nameof(IEnumerable)}.");

            var field = new StringFieldDefinition<TDocument, Object?>(parameter.FieldName);
            var set = (parameter.Value as IEnumerable)?.Cast<Object>();
            var definition = parameter.Operation switch
            {
                FilterOperation.Equal => builder.Eq(field, parameter.Value),
                FilterOperation.GreaterThan => builder.Gt(field, parameter.Value),
                FilterOperation.GreaterThanOrEqual => builder.Gte(field, parameter.Value),
                FilterOperation.In => builder.In(field, set),
                FilterOperation.LessThan => builder.Lt(field, parameter.Value),
                FilterOperation.LessThanOrEqual => builder.Lte(field, parameter.Value),
                FilterOperation.NotEqual => builder.Not(builder.Eq(field, parameter.Value)),
                _ => throw new ArgumentOutOfRangeException()
            };

            result.Add(definition);
        }

        return builder.And(result);
    }
}
