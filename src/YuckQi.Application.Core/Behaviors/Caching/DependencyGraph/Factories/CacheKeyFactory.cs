using System.Diagnostics.CodeAnalysis;
using YuckQi.Application.Core.Behaviors.Caching;

namespace YuckQi.Application.Core.Behaviors.Caching.DependencyGraph.Factories;

public static class CacheKeyFactory
{
    public const Char IdentifierSeparator = ':';

    public const Char ParameterAssignment = '=';

    public const Char ParameterSeparator = ';';

    public static CacheKey Create(String resource)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);

        return new CacheKey(resource);
    }

    public static CacheKey Create(String resource, Object identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);
        ArgumentNullException.ThrowIfNull(identifier);

        var identifierText = identifier.ToString();
        ArgumentException.ThrowIfNullOrWhiteSpace(identifierText);

        return new CacheKey($"{resource}{IdentifierSeparator}{identifierText}");
    }

    public static CacheKey Create(String resource, Object identifier, params (String Name, Object Value)[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);
        ArgumentNullException.ThrowIfNull(identifier);
        ArgumentNullException.ThrowIfNull(parameters);

        var identifierText = identifier.ToString();
        ArgumentException.ThrowIfNullOrWhiteSpace(identifierText);

        if (parameters.Length == 0)
            return new CacheKey($"{resource}{IdentifierSeparator}{identifierText}");

        var segments = new String[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var (name, value) = parameters[i];
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(value);

            var valueText = value.ToString();
            ArgumentException.ThrowIfNullOrWhiteSpace(valueText);

            segments[i] = $"{name}{ParameterAssignment}{valueText}";
        }

        return new CacheKey($"{resource}{IdentifierSeparator}{identifierText}{ParameterSeparator}{String.Join(ParameterSeparator, segments)}");
    }

    public static Boolean TryParse(CacheKey key, [NotNullWhen(true)] out CacheKeyParts? parts)
    {
        return TryParse((String) key, out parts);
    }

    public static Boolean TryParse(String? key, [NotNullWhen(true)] out CacheKeyParts? parts)
    {
        parts = null;

        if (String.IsNullOrWhiteSpace(key))
            return false;

        var identifierSeparatorIndex = key.IndexOf(IdentifierSeparator);
        if (identifierSeparatorIndex < 0)
        {
            parts = new CacheKeyParts(key, null, EmptyParameters);

            return true;
        }

        if (identifierSeparatorIndex == 0 || identifierSeparatorIndex == key.Length - 1)
            return false;

        var resource = key[..identifierSeparatorIndex];
        if (String.IsNullOrWhiteSpace(resource))
            return false;

        var remainder = key[(identifierSeparatorIndex + 1)..];
        var parameterSeparatorIndex = remainder.IndexOf(ParameterSeparator);
        if (parameterSeparatorIndex < 0)
        {
            if (String.IsNullOrWhiteSpace(remainder))
                return false;

            parts = new CacheKeyParts(resource, remainder, EmptyParameters);

            return true;
        }

        if (parameterSeparatorIndex == 0)
            return false;

        var identifier = remainder[..parameterSeparatorIndex];
        if (String.IsNullOrWhiteSpace(identifier))
            return false;

        var parameterText = remainder[(parameterSeparatorIndex + 1)..];
        if (String.IsNullOrWhiteSpace(parameterText))
        {
            parts = new CacheKeyParts(resource, identifier, EmptyParameters);

            return true;
        }

        var parameterSegments = parameterText.Split(ParameterSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var parameters = new Dictionary<String, String>(StringComparer.Ordinal);
        foreach (var segment in parameterSegments)
        {
            var assignmentIndex = segment.IndexOf(ParameterAssignment);
            if (assignmentIndex <= 0 || assignmentIndex == segment.Length - 1)
                return false;

            var name = segment[..assignmentIndex];
            var value = segment[(assignmentIndex + 1)..];
            if (String.IsNullOrWhiteSpace(name) || String.IsNullOrWhiteSpace(value))
                return false;

            parameters[name] = value;
        }

        parts = new CacheKeyParts(resource, identifier, parameters);

        return true;
    }

    private static readonly IReadOnlyDictionary<String, String> EmptyParameters = new Dictionary<String, String>();
}
