namespace EasyCaching.CSRedis
{
    using System;
    using global::CSRedis;

    public class EasyCachingCSRedisClient : CSRedisClient
    {
        private readonly string _name;

        public string Name { get { return this._name; } }

        public EasyCachingCSRedisClient(string name, string connectionString) 
            : base(connectionString)
        {
            this._name = name;
        }

        public EasyCachingCSRedisClient(string name, Func<string, string> nodeRule, params string[] connectionStrings) 
            : base(nodeRule, connectionStrings)
        {
            this._name = name;
        }

        public EasyCachingCSRedisClient(string name, string connectionString, string[] sentinels, bool readOnly)
           : base(connectionString, sentinels, readOnly)
        {
            this._name = name;
        }
    }        
}
