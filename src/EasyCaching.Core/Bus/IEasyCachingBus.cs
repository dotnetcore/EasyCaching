namespace EasyCaching.Core.Bus
{
    /// <summary>
    /// Easycaching bus.
    /// </summary>
    public interface IEasyCachingBus :  IEasyCachingPublisher , IEasyCachingSubscriber
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }
    }
}
