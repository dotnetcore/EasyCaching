namespace EasyCaching.CSRedis
{
    using EasyCaching.Core;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GeoUnit = global::CSRedis.GeoUnit;

    public partial class DefaultCSRedisCachingProvider : IRedisCachingProvider
    {
        public long GeoAdd(string cacheKey, List<(double longitude, double latitude, string member)> values)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

            var list = new List<(double longitude, double latitude, object member)>();

            foreach (var item in values)
            {
                list.Add((item.longitude, item.latitude, item.member));
            }

            var res = _cache.GeoAdd(cacheKey, list.ToArray());
            return res;
        }

        public async Task<long> GeoAddAsync(string cacheKey, List<(double longitude, double latitude, string member)> values)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

            var list = new List<(double longitude, double latitude, object member)>();

            foreach (var item in values)
            {
                list.Add((item.longitude, item.latitude, item.member));
            }

            var res = await _cache.GeoAddAsync(cacheKey, list.ToArray());
            return res;
        }

        public double? GeoDist(string cacheKey, string member1, string member2, string unit = "m")
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(member1, nameof(member1));
            ArgumentCheck.NotNullOrWhiteSpace(member2, nameof(member2));
            ArgumentCheck.NotNullOrWhiteSpace(unit, nameof(unit));

            var res = _cache.GeoDist(cacheKey, member1, member2, GetGeoUnit(unit));
            return res;
        }

        public async Task<double?> GeoDistAsync(string cacheKey, string member1, string member2, string unit = "m")
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullOrWhiteSpace(member1, nameof(member1));
            ArgumentCheck.NotNullOrWhiteSpace(member2, nameof(member2));
            ArgumentCheck.NotNullOrWhiteSpace(unit, nameof(unit));

            var res = await _cache.GeoDistAsync(cacheKey, member1, member2, GetGeoUnit(unit));
            return res;
        }

        public List<string> GeoHash(string cacheKey, List<string> members)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(members, nameof(members));
            
            var list = new List<object>();
            foreach (var item in members)
            {
                list.Add(item);
            }

            var res = _cache.GeoHash(cacheKey, list.ToArray());
            return res.ToList() ;
        }

        public async Task<List<string>> GeoHashAsync(string cacheKey, List<string> members)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(members, nameof(members));

            var list = new List<object>();
            foreach (var item in members)
            {
                list.Add(item);
            }

            var res = await _cache.GeoHashAsync(cacheKey, list.ToArray());
            return res.ToList();
        }

        public List<(double longitude, double latitude)?> GeoPos(string cacheKey, List<string> members)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(members, nameof(members));

            var list = new List<object>();
            foreach (var item in members)
            {
                list.Add(item);
            }

            var res = _cache.GeoPos(cacheKey, list.ToArray());
            return res.ToList();
        }

        public async Task<List<(double longitude, double latitude)?>> GeoPosAsync(string cacheKey, List<string> members)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNullAndCountGTZero(members, nameof(members));

            var list = new List<object>();
            foreach (var item in members)
            {
                list.Add(item);
            }

            var res = await _cache.GeoPosAsync(cacheKey, list.ToArray());
            return res.ToList();
        }

        private GeoUnit GetGeoUnit(string unit)
        {
            GeoUnit geoUnit;
            switch (unit)
            {
                case "km":
                    geoUnit = GeoUnit.km;
                    break;
                case "ft":
                    geoUnit = GeoUnit.ft;
                    break;
                case "mi":
                    geoUnit = GeoUnit.mi;
                    break;
                default:
                    geoUnit = GeoUnit.m;
                    break;
            }
            return geoUnit;
        }
    }
}
