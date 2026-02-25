namespace HeimdallWeb.Extensions
{
    public class LowercaseParameterTransformer : IOutboundParameterTransformer
    {
        public string? TransformOutbound(object? value)
        {
            if (value is null) return null;
            return value.ToString()?.ToLowerInvariant();
        }
    }
}
