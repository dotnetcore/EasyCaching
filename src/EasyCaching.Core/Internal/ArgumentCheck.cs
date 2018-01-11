namespace EasyCaching.Core.Internal
{
    using System;

    /// <summary>
    /// Argument check.
    /// </summary>
    public static class ArgumentCheck
    {
        /// <summary>
        /// Validates that <paramref name="argument"/> is not <c>null</c> , otherwise throws an exception.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="argumentName">Argument name.</param>
        /// <exception cref="ArgumentNullException">
        public static void NotNull(object argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Validates that <paramref name="argument"/> is not <c>null</c> , otherwise throws an exception.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="argumentName">Argument name.</param>
        /// <exception cref="ArgumentNullException">
        public static void NotNullOrWhiteSpace(string argument, string argumentName)
        {            
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentNullException(argumentName);
            }
        }
    }
}
