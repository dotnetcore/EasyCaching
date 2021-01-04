namespace EasyCaching.Bus.CSRedis
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

        public EasyCachingCSRedisClient(string name, Func<string, string> NodeRule, params string[] connectionStrings) 
            : base(NodeRule, connectionStrings)
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
