namespace EasyCaching.Serialization.MessagePack
{
    /// <summary>
    /// EasyCachingMsgPackSerializerOptions
    /// </summary>
    public class EasyCachingMsgPackSerializerOptions
    {
        /// <summary>
        /// Whethe to enable custom resolver
        /// </summary>
        public bool EnableCustomResolver { get; set; }

        /// <summary>
        /// The custom resolver you want to use
        /// </summary>
        public global::MessagePack.IFormatterResolver CustomResolvers { get; set; }
    }
}
