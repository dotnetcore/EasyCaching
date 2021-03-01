namespace EasyCaching.Decoration.Polly
{
    using global::Polly;
    using System;

    public static class PolicyFactories
    {
        public static PolicyBuilder GetHandleExceptionPolicyBuilder(this Func<Exception, bool> exceptionFilter)
        {
            return exceptionFilter == null
                ? Policy.Handle<Exception>()
                : Policy.Handle(exceptionFilter).OrInner(exceptionFilter);
        }
        
        public static PolicyBuilder<TResult> GetHandleExceptionPolicyBuilder<TResult>(this Func<Exception, bool> exceptionFilter)
        {
            return exceptionFilter == null
                ? Policy<TResult>.Handle<Exception>()
                : Policy<TResult>.Handle(exceptionFilter).OrInner(exceptionFilter);
        }
    }
}