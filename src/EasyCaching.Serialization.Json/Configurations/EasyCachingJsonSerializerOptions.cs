namespace EasyCaching.Serialization.Json
{
    using Newtonsoft.Json;

    /// <summary>
    /// EasyCaching json serializer options.
    /// </summary>
    public class EasyCachingJsonSerializerOptions
    {
        /// <summary>
        /// Gets or sets the reference loop handling.
        /// </summary>
        /// <value>The reference loop handling.</value>
        public ReferenceLoopHandling ReferenceLoopHandling { get; set; } = ReferenceLoopHandling.Error;

        /// <summary>
        /// Gets or sets the type name handling.
        /// </summary>
        /// <value>The type name handling.</value>
        public TypeNameHandling TypeNameHandling { get; set; } = TypeNameHandling.All;

        /// <summary>
        /// Gets or sets the metadata property handling.
        /// </summary>
        /// <value>The metadata property handling.</value>
        public MetadataPropertyHandling MetadataPropertyHandling { get; set; } = MetadataPropertyHandling.Default;

        /// <summary>
        /// The missing member handling.
        /// </summary>
        public MissingMemberHandling MissingMemberHandling = MissingMemberHandling.Ignore;

        /// <summary>
        /// The null value handling.
        /// </summary>
        public NullValueHandling NullValueHandling = NullValueHandling.Include;

        /// <summary>
        /// The value handling.
        /// </summary>
        public DefaultValueHandling DefaultValueHandling = DefaultValueHandling.Include;

        /// <summary>
        /// The object creation handling.
        /// </summary>
        public ObjectCreationHandling ObjectCreationHandling = ObjectCreationHandling.Auto;

        /// <summary>
        /// The preserve references handling.
        /// </summary>
        public PreserveReferencesHandling PreserveReferencesHandling = PreserveReferencesHandling.None;

        /// <summary>
        /// The constructor handling.
        /// </summary>
        public ConstructorHandling ConstructorHandling = ConstructorHandling.Default;
    }
}
