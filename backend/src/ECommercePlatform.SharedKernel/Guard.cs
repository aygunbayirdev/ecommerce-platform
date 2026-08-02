namespace ECommercePlatform.SharedKernel;

public static class Guard
{
    public static string AgainstNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", parameterName);
        }

        return value;
    }

    public static T AgainstNull<T>(T? value, string parameterName)
        where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName);
        }

        return value;
    }

    public static Guid AgainstEmpty(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Value cannot be an empty GUID.", parameterName);
        }

        return value;
    }
}
