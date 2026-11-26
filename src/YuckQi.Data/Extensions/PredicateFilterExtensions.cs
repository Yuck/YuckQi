using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using YuckQi.Data.Filtering;

namespace YuckQi.Data.Extensions;

public static class PredicateFilterExtensions
{
    public static IReadOnlyCollection<FilterExpression<TDomainEntity>> ToFilterExpressions<TDomainEntity>(this Expression<Func<TDomainEntity, Boolean>> predicate)
    {
        return [.. Collect<TDomainEntity>(predicate.Body, predicate.Parameters.First())];
    }

    private static Object? CoerceToEnum(Object? value, Type memberType)
    {
        if (value is null)
            return null;

        var type = Nullable.GetUnderlyingType(memberType) ?? memberType;

        if (! type.IsEnum || type.IsInstanceOfType(value) || ! IsIntegral(value))
            return value;

        try
        {
            var underlying = Convert.ChangeType(value, Enum.GetUnderlyingType(type), CultureInfo.InvariantCulture);

            return Enum.ToObject(type, underlying);
        }
        catch (OverflowException)
        {
            return value;
        }
    }

    private static IEnumerable<FilterExpression<TDomainEntity>> Collect<TDomainEntity>(Expression expression, ParameterExpression parameter)
    {
        if (expression is BinaryExpression { NodeType: ExpressionType.AndAlso } binary)
            return Collect<TDomainEntity>(binary.Left, parameter).Concat(Collect<TDomainEntity>(binary.Right, parameter));

        if (expression is BinaryExpression comparison)
            return [ToComparisonFilter<TDomainEntity>(comparison, parameter)];

        if (expression is MethodCallExpression methodCall)
            return [ToContainsFilter<TDomainEntity>(methodCall, parameter)];

        throw new NotSupportedException($"Expression of type '{expression.NodeType}' is not supported.");
    }

    private static Object? Compile(Expression expression)
    {
        var lambda = Expression.Lambda<Func<Object?>>(Expression.Convert(expression, typeof(Object)));

        return lambda.Compile(preferInterpretation: true)();
    }

    private static Object? Evaluate(Expression expression)
    {
        if (expression is ConstantExpression constant)
            return constant.Value;

        if (expression is UnaryExpression { NodeType: ExpressionType.Convert } unary && Nullable.GetUnderlyingType(unary.Type) == unary.Operand.Type)
            return Evaluate(unary.Operand);

        if (expression is MemberExpression member)
            return EvaluateMember(member);

        return Compile(expression);
    }

    private static Object? EvaluateInstance(Expression? instance)
    {
        if (instance is null)
            return null;

        return Evaluate(instance);
    }

    private static Object? EvaluateMember(MemberExpression member)
    {
        var instance = EvaluateInstance(member.Expression);

        if (member.Expression is not null && instance is null)
            return Compile(Expression.MakeMemberAccess(Expression.Constant(null, member.Expression.Type), member.Member));

        return member.Member switch
        {
            FieldInfo field => field.GetValue(instance),
            PropertyInfo property => property.GetValue(instance),
            _ => Compile(member)
        };
    }

    private static Expression GetMember(Expression expression, ParameterExpression parameter)
    {
        var member = expression.RemoveConversions();

        if (! member.IsMemberPathOf(parameter))
            throw new NotSupportedException($"Expression '{expression}' must be a chain of properties or fields on '{parameter.Name}', such as '{parameter.Name}.Name' or '{parameter.Name}.Address.City'.");

        return member;
    }

    private static FilterOperation GetOperation(ExpressionType type)
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

    private static Boolean IsIntegral(Object value)
    {
        if (value is Enum)
            return false;

        return Type.GetTypeCode(value.GetType()) is TypeCode.SByte or TypeCode.Byte or TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64;
    }

    private static FilterOperation Mirror(FilterOperation operation)
    {
        return operation switch
        {
            FilterOperation.GreaterThan => FilterOperation.LessThan,
            FilterOperation.GreaterThanOrEqual => FilterOperation.LessThanOrEqual,
            FilterOperation.LessThan => FilterOperation.GreaterThan,
            FilterOperation.LessThanOrEqual => FilterOperation.GreaterThanOrEqual,
            _ => operation
        };
    }

    private static FilterExpression<TDomainEntity> ToComparisonFilter<TDomainEntity>(BinaryExpression comparison, ParameterExpression parameter)
    {
        var operation = GetOperation(comparison.NodeType);
        var isLeftMember = comparison.Left.HasReferenceTo(parameter);
        var isRightMember = comparison.Right.HasReferenceTo(parameter);

        if (isLeftMember == isRightMember)
            throw new NotSupportedException($"Comparison '{comparison}' must be between a member of '{parameter.Name}' and a value that does not depend on '{parameter.Name}'.");

        if (isLeftMember)
            return ToFilter<TDomainEntity>(comparison.Left, operation, comparison.Right, parameter);

        return ToFilter<TDomainEntity>(comparison.Right, Mirror(operation), comparison.Left, parameter);
    }

    private static FilterExpression<TDomainEntity> ToContainsFilter<TDomainEntity>(MethodCallExpression methodCall, ParameterExpression parameter)
    {
        if (methodCall.Object is null || methodCall.Method.DeclaringType != typeof(String) || methodCall.Method.Name != nameof(String.Contains) || methodCall.Arguments.Count != 1)
            throw new NotSupportedException($"Expression of type '{methodCall.NodeType}' is not supported.");

        var argument = methodCall.Arguments[0];

        if (argument.HasReferenceTo(parameter))
            throw new NotSupportedException($"The argument of '{methodCall}' must be a value that does not depend on '{parameter.Name}'.");

        var member = GetMember(methodCall.Object, parameter);
        var value = Evaluate(argument);

        if (value is null)
            throw new ArgumentException($"{nameof(FilterExpression<TDomainEntity>.Value)} is required for {nameof(FilterOperation.Contains)}.");

        return new FilterExpression<TDomainEntity>(ToLambda<TDomainEntity>(member, parameter), FilterOperation.Contains, value);
    }

    private static FilterExpression<TDomainEntity> ToFilter<TDomainEntity>(Expression memberSide, FilterOperation operation, Expression valueSide, ParameterExpression parameter)
    {
        var member = GetMember(memberSide, parameter);
        var value = CoerceToEnum(Evaluate(valueSide), member.Type);

        return new FilterExpression<TDomainEntity>(ToLambda<TDomainEntity>(member, parameter), operation, value);
    }

    private static Expression<Func<TDomainEntity, Object>> ToLambda<TDomainEntity>(Expression member, ParameterExpression parameter)
    {
        var converted = Expression.Convert(member, typeof(Object));

        return Expression.Lambda<Func<TDomainEntity, Object>>(converted, parameter);
    }
}
