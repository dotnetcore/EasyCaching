namespace EasyCaching.ResponseCaching
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// EasyCaching response option.
    /// </summary>
    public class EasyCachingResponseOption
    {
        /// <summary>
        /// Gets or sets the services.
        /// </summary>
        /// <value>The services.</value>
        public IServiceCollection Services { get; set; }
    }
}
