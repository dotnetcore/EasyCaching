namespace EasyCaching.Decoration.Polly
{
    using global::Polly;
    using System;

    public class AdvancedCircuitBreakerParameters : ICircuitBreakerParameters
    {
        private readonly double _failureThreshold;
        private readonly TimeSpan _samplingDuration;
        private readonly int _minimumThroughput;
        private readonly TimeSpan _durationOfBreak;

        public AdvancedCircuitBreakerParameters(
            double failureThreshold,
            TimeSpan samplingDuration,
            int minimumThroughput,
            TimeSpan durationOfBreak)
        {
            _failureThreshold = failureThreshold;
            _samplingDuration = samplingDuration;
            _minimumThroughput = minimumThroughput;
            _durationOfBreak = durationOfBreak;
        }
        
        public Policy<TResult> CreatePolicy<TResult>(PolicyBuilder<TResult> policyBuilder) => 
            policyBuilder.AdvancedCircuitBreaker(_failureThreshold, _samplingDuration, _minimumThroughput, _durationOfBreak);

        public Policy CreatePolicy(PolicyBuilder policyBuilder) => 
            policyBuilder.AdvancedCircuitBreaker(_failureThreshold, _samplingDuration, _minimumThroughput, _durationOfBreak);

        public AsyncPolicy CreatePolicyAsync(PolicyBuilder policyBuilder) => 
            policyBuilder.AdvancedCircuitBreakerAsync(_failureThreshold, _samplingDuration, _minimumThroughput, _durationOfBreak);
    }
}