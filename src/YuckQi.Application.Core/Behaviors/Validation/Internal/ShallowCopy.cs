using System.Reflection;

namespace YuckQi.Application.Core.Behaviors.Validation.Internal;

internal static class ShallowCopy
{
    private static readonly Func<Object, Object> Clone = CreateClone();

    public static T Of<T>(T value) where T : notnull
    {
        return (T) Clone(value);
    }

    private static Func<Object, Object> CreateClone()
    {
        var method = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
        if (method is null)
            throw new MissingMethodException(nameof(Object), "MemberwiseClone");

        return method.CreateDelegate<Func<Object, Object>>();
    }
}
