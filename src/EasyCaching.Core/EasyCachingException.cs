namespace EasyCaching.Core
{
    using System;

    public class EasyCachingException : Exception
    {
        public EasyCachingException(string message)
            : base(message)
        {
        }
    }

    public class EasyCachingNotFoundException : Exception
    {
        public EasyCachingNotFoundException(string message)
            : base(message)
        {
        }
    }
}
