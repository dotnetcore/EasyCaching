namespace EasyCaching.ResponseCaching
{
    public class ResponseCachingOptions
    {
        /// <summary>
        /// The largest cacheable size for the response body in bytes. The default is set to 64 MB.
        /// </summary>
        public long MaximumBodySize { get; set; } = 64 * 1024 * 1024;

        /// <summary>
        /// <c>true</c> if request paths are case-sensitive; otherwise <c>false</c>. The default is to treat paths as case-insensitive.
        /// </summary>
        public bool UseCaseSensitivePaths { get; set; } = false;
    }
}
