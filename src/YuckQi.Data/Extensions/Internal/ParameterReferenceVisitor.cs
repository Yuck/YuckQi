using System.Linq.Expressions;

namespace YuckQi.Data.Extensions.Internal;

internal sealed class ParameterReferenceVisitor : ExpressionVisitor
{
    private readonly ParameterExpression _parameter;
    private Boolean _isReferenced;

    private ParameterReferenceVisitor(ParameterExpression parameter)
    {
        _parameter = parameter;
    }

    public static Boolean IsReferenced(Expression expression, ParameterExpression parameter)
    {
        var visitor = new ParameterReferenceVisitor(parameter);

        visitor.Visit(expression);

        return visitor._isReferenced;
    }

    public override Expression? Visit(Expression? node)
    {
        if (_isReferenced)
            return node;

        return base.Visit(node);
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (node == _parameter)
            _isReferenced = true;

        return node;
    }
}
