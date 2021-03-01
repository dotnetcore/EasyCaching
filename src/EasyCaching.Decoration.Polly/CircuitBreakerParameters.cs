namespace EasyCaching.Decoration.Polly
{
    using global::Polly;
    using System;

    public class CircuitBreakerParameters : ICircuitBreakerParameters
    {
        private readonly int _exceptionsAllowedBeforeBreaking;
        private readonly TimeSpan _durationOfBreak;

        public CircuitBreakerParameters(int exceptionsAllowedBeforeBreaking, TimeSpan durationOfBreak)
        {
            _exceptionsAllowedBeforeBreaking = exceptionsAllowedBeforeBreaking;
            _durationOfBreak = durationOfBreak;
        }
        
        public Policy<TResult> CreatePolicy<TResult>(PolicyBuilder<TResult> policyBuilder) => 
            policyBuilder.CircuitBreaker(_exceptionsAllowedBeforeBreaking, _durationOfBreak);

        public Policy CreatePolicy(PolicyBuilder policyBuilder) => 
            policyBuilder.CircuitBreaker(_exceptionsAllowedBeforeBreaking, _durationOfBreak);

        public AsyncPolicy CreatePolicyAsync(PolicyBuilder policyBuilder) => 
            policyBuilder.CircuitBreakerAsync(_exceptionsAllowedBeforeBreaking, _durationOfBreak);
    }
}