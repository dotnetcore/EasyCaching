namespace EasyCaching.Decoration.Polly
{
    using global::Polly;

    public interface ICircuitBreakerParameters
    {
        Policy<TResult> CreatePolicy<TResult>(PolicyBuilder<TResult> policyBuilder);
        Policy CreatePolicy(PolicyBuilder policyBuilder);
        AsyncPolicy CreatePolicyAsync(PolicyBuilder policyBuilder);
    }
}