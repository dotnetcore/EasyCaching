namespace EasyCaching.Core.Decoration
{
    using Polly;

    public interface ICircuitBreakerParameters
    {
        Policy<TResult> CreatePolicy<TResult>(PolicyBuilder<TResult> policyBuilder);
        Policy CreatePolicy(PolicyBuilder policyBuilder);
        AsyncPolicy CreatePolicyAsync(PolicyBuilder policyBuilder);
    }
}