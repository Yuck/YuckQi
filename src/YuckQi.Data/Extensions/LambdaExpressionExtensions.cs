using System.Linq.Expressions;

namespace YuckQi.Data.Extensions;

internal static class LambdaExpressionExtensions
{
    public static String GetPath(this LambdaExpression expression)
    {
        var member = UnwrapToMember(expression.Body);
        var members = GetMembers(member);

        return String.Join(".", members);

        static MemberExpression? UnwrapToMember(Expression expression)
        {
            if (expression is UnaryExpression { NodeType: ExpressionType.Convert } unary)
                return UnwrapToMember(unary.Operand);

            return expression as MemberExpression;
        }

        static IEnumerable<String> GetMembers(MemberExpression? expression)
        {
            var members = new Stack<String>();

            while (expression is not null)
            {
                members.Push(expression.Member.Name);

                expression = expression.Expression as MemberExpression;
            }

            while (members.Count > 0)
                yield return members.Pop();
        }
    }
}
