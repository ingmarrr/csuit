namespace Utils;

public static class EnumExtensions
{
    public static T ToEnum<T>(this string value, bool ignoreCase = false, T? defaultValue = null)
        where T : struct, IConvertible
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (!Enum.TryParse<T>(value, ignoreCase, out var result))
        {
            if (defaultValue != null) return (T)defaultValue;
            throw new InvalidCastException($"Cannot convert {value} to {typeof(T).Name}");
        }

        return result;
    }
}