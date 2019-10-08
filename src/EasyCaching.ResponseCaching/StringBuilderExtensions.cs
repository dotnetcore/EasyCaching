namespace EasyCaching.ResponseCaching
{
    using System.Text;

    internal static class StringBuilderExtensions
    {
        internal static StringBuilder AppendUpperInvariant(this StringBuilder builder, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                builder.EnsureCapacity(builder.Length + value.Length);
                for (var i = 0; i < value.Length; i++)
                {
                    builder.Append(char.ToUpperInvariant(value[i]));
                }
            }

            return builder;
        }
    }
}
