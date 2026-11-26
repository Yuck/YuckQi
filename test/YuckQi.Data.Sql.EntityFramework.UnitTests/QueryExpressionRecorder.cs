using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace YuckQi.Data.Sql.EntityFramework.UnitTests;

internal class QueryExpressionRecorder : IQueryExpressionInterceptor
{
    public List<Expression> Expressions { get; } = [];

    public Expression QueryCompilationStarting(Expression queryExpression, QueryExpressionEventData eventData)
    {
        Expressions.Add(queryExpression);

        return queryExpression;
    }
}
