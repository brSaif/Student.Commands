namespace gRPCOnHttp3.Extensions;

public static class Ensure
{
    public static bool NullOrWhiteSpace(string value)
        => string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{value} cannot be null or empty")
            : true;
}