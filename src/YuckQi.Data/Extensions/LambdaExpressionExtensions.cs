using System.Linq.Expressions;
using YuckQi.Data.Extensions.Internal;

namespace YuckQi.Data.Extensions;

internal static class LambdaExpressionExtensions
{
    public static String GetPath(this LambdaExpression expression)
    {
        var members = GetMemberNames(expression.Body, expression.Parameters);

        if (members is null)
            throw new NotSupportedException($"Expression '{expression}' must be a chain of properties or fields on its parameter, such as 't => t.Name' or 't => t.Address.City'.");

        return String.Join(".", members);
    }

    public static Boolean HasReferenceTo(this Expression expression, ParameterExpression parameter)
    {
        return ParameterReferenceVisitor.IsReferenced(expression, parameter);
    }

    public static Boolean IsMemberPathOf(this Expression expression, ParameterExpression parameter)
    {
        return GetMemberNames(expression, [parameter]) is not null;
    }

    public static Expression RemoveConversions(this Expression expression)
    {
        var current = expression;

        while (current is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked or ExpressionType.TypeAs } unary)
            current = unary.Operand;

        return current;
    }

    private static IReadOnlyList<String>? GetMemberNames(Expression expression, IReadOnlyCollection<ParameterExpression> parameters)
    {
        var members = new List<String>();
        var current = expression.RemoveConversions();

        while (current is MemberExpression member)
        {
            if (member.Expression is null)
                return null;

            if (IsNullableMember(member, nameof(Nullable<Int32>.HasValue)))
                return null;

            if (! IsNullableMember(member, nameof(Nullable<Int32>.Value)))
                members.Add(member.Member.Name);

            current = member.Expression.RemoveConversions();
        }

        if (members.Count == 0)
            return null;

        if (current is not ParameterExpression parameter || ! parameters.Contains(parameter))
            return null;

        members.Reverse();

        return members;
    }

    private static Boolean IsNullableMember(MemberExpression member, String name)
    {
        if (member.Member.DeclaringType is null || Nullable.GetUnderlyingType(member.Member.DeclaringType) is null)
            return false;

        return member.Member.Name == name;
    }
}
