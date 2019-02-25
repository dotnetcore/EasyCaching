namespace EasyCaching.Core.Diagnostics
{
    public class GetCountEventData : EventData
    {
        public GetCountEventData(string cacheType, string name, string operation, string prefix, long count)
            : base(cacheType, name, operation)
        {
            this.Prefix = prefix;
            this.Count = count;
        }

        public string Prefix { get; set; }

        public long Count { get; set; }
    }
}
