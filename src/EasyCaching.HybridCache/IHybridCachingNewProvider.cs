
// namespace EasyCaching.HybridCache
// {
//     using EasyCaching.Core;
//     using EasyCaching.Core.Internal;
//     using System;
//     using System.Collections.Generic;
//     using System.Linq;
//     using System.Threading.Tasks;

//     public interface IHybridCachingNewProvider
//     {
//         void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration);

//         CacheValue<T> Get<T>(string cacheKey);

//         void Remove(string cacheKey);
//     }

//     public class HybridCacheNewProvider : IHybridCachingNewProvider
//     {
//         private readonly IEasyCachingProvider _local;
//         private readonly IEasyCachingProvider _remote;
//         private readonly IEasyCachingBus _bus;

//         public HybridCacheNewProvider(IEnumerable<IEasyCachingProvider> providers, IEasyCachingBus bus)
//         {
//             if (providers == null || !providers.Any())
//             {
//                 throw new ArgumentNullException(nameof(providers));
//             }
            
//             if (providers.Count() > 2)
//             {
//                 throw new ArgumentOutOfRangeException(nameof(providers));
//             }

//             if(providers.Count(x=>x.IsDistributedCache)>1)
//             {
//                 throw new ArgumentOutOfRangeException(nameof(providers));
//             }

//             if(providers.Count(x=>!x.IsDistributedCache)>1)
//             {
//                 throw new ArgumentOutOfRangeException(nameof(providers));
//             }

//             this._local = providers.First(x=>!x.IsDistributedCache);
//             this._remote = providers.First(x=>x.IsDistributedCache);
//             this._bus = bus;
//             this._bus.Subscribe("channel");
//         }

//         public CacheValue<T> Get<T>(string cacheKey)
//         {
//             var obj = _local.Get<T>(cacheKey);
            
//             if(obj.HasValue) return obj;
                        
//             obj = _remote.Get<T>(cacheKey);

//             return obj.HasValue
//                     ? obj
//                     : CacheValue<T>.NoValue;            
//         }

//         public void Remove(string cacheKey)
//         {
//             _remote.Remove(cacheKey);
//             _bus.Publish("channel",new EasyCachingMessage
//             {
//                 CacheKey = cacheKey,
//                 NotifyType = NotifyType.Delete            
//             });
//         }

//         public void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration)
//         {
//             _remote.Set(cacheKey,cacheValue,expiration);
//             _bus.Publish("channel",new EasyCachingMessage
//             {
//                 CacheKey = cacheKey,
//                 CacheValue = cacheValue,
//                 Expiration = expiration,
//                 NotifyType = NotifyType.Add    
//             });
//         }
//     }
// }
