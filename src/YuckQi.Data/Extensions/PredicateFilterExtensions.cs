using System.Linq.Expressions;
using YuckQi.Data.Filtering;

namespace YuckQi.Data.Extensions;

public static class PredicateFilterExtensions
{
    public static IReadOnlyCollection<FilterExpression<TDomainEntity>> ToFilterExpressions<TDomainEntity>(this Expression<Func<TDomainEntity, Boolean>> predicate)
    {
        return Collect(predicate.Body, predicate.Parameters.First()).ToList();

        static IEnumerable<FilterExpression<TDomainEntity>> Collect(Expression expression, ParameterExpression parameter)
        {
            if (expression is BinaryExpression { NodeType: ExpressionType.AndAlso } binary)
                return Collect(binary.Left, parameter).Concat(Collect(binary.Right, parameter));

            if (expression is BinaryExpression comparison)
            {
                var (member, constant) = GetMemberAndConstant(comparison.Left, comparison.Right);

                var operation = GetOperation(comparison.NodeType);
                var converted = Expression.Convert(member, typeof(Object));
                var lambda = Expression.Lambda<Func<TDomainEntity, Object>>(converted, parameter);
                var value = constant.Value;

                return [new FilterExpression<TDomainEntity>(lambda, operation, value)];
            }

            throw new NotSupportedException($"Expression of type '{expression.NodeType}' is not supported.");
        }

        static (MemberExpression member, ConstantExpression constant) GetMemberAndConstant(Expression left, Expression right)
        {
            var leftMember = UnwrapMember(left);
            var rightMember = UnwrapMember(right);

            if (leftMember is not null && right is ConstantExpression rightConstant)
                return (leftMember, rightConstant);

            if (rightMember is not null && left is ConstantExpression leftConstant)
                return (rightMember, leftConstant);

            throw new NotSupportedException("Comparison must be between a member and a constant.");
        }

        static FilterOperation GetOperation(ExpressionType type)
        {
            return type switch
            {
                ExpressionType.Equal => FilterOperation.Equal,
                ExpressionType.NotEqual => FilterOperation.NotEqual,
                ExpressionType.GreaterThan => FilterOperation.GreaterThan,
                ExpressionType.GreaterThanOrEqual => FilterOperation.GreaterThanOrEqual,
                ExpressionType.LessThan => FilterOperation.LessThan,
                ExpressionType.LessThanOrEqual => FilterOperation.LessThanOrEqual,
                _ => throw new NotSupportedException($"Expression type '{type}' is not supported.")
            };
        }

        static MemberExpression? UnwrapMember(Expression expression)
        {
            if (expression is MemberExpression member)
                return member;

            if (expression is UnaryExpression { NodeType: ExpressionType.Convert } unary)
                return UnwrapMember(unary.Operand);

            return null;
        }
    }
}
