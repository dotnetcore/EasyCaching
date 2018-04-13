namespace EasyCaching.HybridCache
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Hybrid caching provider.
    /// </summary>
    public class HybridCachingProvider : IHybridCachingProvider
    {              
        /// <summary>
        /// The caching providers.
        /// </summary>
        private readonly IEnumerable<IEasyCachingProvider> _providers;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.HybridCache.HybridCachingProvider"/> class.
        /// </summary>
        /// <param name="providers">Providers.</param>
        public HybridCachingProvider(IEnumerable<IEasyCachingProvider> providers)
        {
            if(providers == null || !providers.Any())
            {
                throw new ArgumentNullException(nameof(providers));
            }

            //2-level and 3-level are enough for hybrid
            if(providers.Count() > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(providers));   
            }

            //
            this._providers = providers.OrderBy(x=>x.Order);

            //TODO: local cache should subscribe the remote cache
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:EasyCaching.HybridCache.HybridCachingProvider"/> is
        /// distributed cache.
        /// </summary>
        /// <value><c>true</c> if is distributed cache; otherwise, <c>false</c>.</value>
        public bool IsDistributedCache => true;

        public int Order => throw new NotImplementedException();

        public int MaxRdSecond => throw new NotImplementedException();

        public CachingProviderType CachingProviderType => throw new NotImplementedException();

        /// <summary>
        /// Exists the specified cacheKey.
        /// </summary>
        /// <returns>The exists.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public bool Exists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = false;

            var local = _providers.FirstOrDefault();

            flag = local.Exists(cacheKey);

            if(!flag)
            {       
                //remote
                foreach (var provider in _providers.Skip(1))
                {
                    flag = provider.Exists(cacheKey);

                    if (flag) break;
                }             
            }                       

            return flag;
        }

        /// <summary>
        /// Existses the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task<bool> ExistsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var flag = false;

            var local = _providers.FirstOrDefault();

            flag = await local.ExistsAsync(cacheKey);

            if (!flag) 
            {
                //remote
                foreach (var provider in _providers.Skip(1))
                {
                    flag = provider.Exists(cacheKey);
                    
                    if (flag) break;
                }                
            }
        
            return flag;
        }

        /// <summary>
        /// Get the specified cacheKey, dataRetriever and expiration.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public CacheValue<T> Get<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var local = _providers.FirstOrDefault();

            CacheValue<T> cachedValue = local.Get(cacheKey,dataRetriever,expiration);

            if(cachedValue.HasValue)
            {
                return cachedValue;
            }

            //remote
            foreach (var provider in _providers.Skip(1))
            {
                cachedValue = provider.Get(cacheKey, dataRetriever, expiration);

                if (cachedValue.HasValue)
                {
                    break;
                }
            }

            if (!cachedValue.HasValue)
            {
                var retriever = dataRetriever?.Invoke();
                if (retriever != null)
                {
                    Set(cacheKey, retriever, expiration);
                    return new CacheValue<T>(retriever, true);
                }
                else
                {
                    //TODO : Set a null value to cache!!
                    return CacheValue<T>.NoValue;
                }
            }

            return cachedValue;
        }

        /// <summary>
        /// Get the specified cacheKey.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public CacheValue<T> Get<T>(string cacheKey) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var local = _providers.FirstOrDefault();

            CacheValue<T> cachedValue = local.Get<T>(cacheKey);

            if (cachedValue.HasValue)
            {
                return cachedValue;
            }

            //remote
            foreach (var provider in _providers.Skip(1))
            {
                cachedValue = provider.Get<T>(cacheKey);

                if (cachedValue.HasValue)
                {
                    break;
                }
            }

            if (!cachedValue.HasValue)
            {
                return CacheValue<T>.NoValue;
            }

            return cachedValue;
        }

        /// <summary>
        /// Gets the specified cacheKey, dataRetriever and expiration async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var local = _providers.FirstOrDefault();

            CacheValue<T> cachedValue = await local.GetAsync(cacheKey, dataRetriever, expiration);

            if (cachedValue.HasValue)
            {
                return cachedValue;
            }

            //remote
            foreach (var provider in _providers.Skip(1))
            {
                cachedValue = provider.Get<T>(cacheKey);

                if (cachedValue.HasValue)
                {
                    break;
                }
            }

            if (!cachedValue.HasValue)
            {
                var retriever = await dataRetriever?.Invoke();
                if (retriever != null)
                {
                    await SetAsync(cacheKey, retriever, expiration);
                    return new CacheValue<T>(retriever, true);
                }
                else
                {
                    //TODO : Set a null value to cache!!

                    return CacheValue<T>.NoValue;
                }
            }

            return cachedValue;
        }

        /// <summary>
        /// Gets the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var local = _providers.FirstOrDefault();

            CacheValue<T> cachedValue = await local.GetAsync<T>(cacheKey);

            //remote
            foreach (var provider in _providers.Skip(1))
            {
                cachedValue = provider.Get<T>(cacheKey);

                if (cachedValue.HasValue)
                {
                    break;
                }
            }

            if (!cachedValue.HasValue)
            {
                return CacheValue<T>.NoValue;
            }

            return cachedValue;                      
        }

        /// <summary>
        /// Remove the specified cacheKey.
        /// </summary>
        /// <returns>The remove.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public void Remove(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var local = _providers.FirstOrDefault();

            local.Remove(cacheKey);

            //remote
            foreach (var provider in _providers.Skip(1))
            {
                provider.Remove(cacheKey);
            }
        }

        /// <summary>
        /// Removes the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task RemoveAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var local = _providers.FirstOrDefault();

            await local.RemoveAsync(cacheKey);

            //remote
            foreach (var provider in _providers.Skip(1))
            {
                provider.Remove(cacheKey);
            }
        }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var local = _providers.FirstOrDefault();

            local.Set(cacheKey, cacheValue, expiration);

            //remote
            foreach (var provider in _providers.Skip(1))
            {
                provider.Set(cacheKey, cacheValue, expiration);
            }                    
        }

        /// <summary>
        /// Sets the specified cacheKey, cacheValue and expiration async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var local = _providers.FirstOrDefault();

            await local.SetAsync(cacheKey, cacheValue, expiration);

            //remote
            foreach (var provider in _providers.Skip(1))
            {
                provider.Set(cacheKey, cacheValue, expiration);
            }   
        }

        /// <summary>
        /// Refresh the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Refresh<T>(string cacheKey, T cacheValue, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            this.Remove(cacheKey);
            this.Set(cacheKey, cacheValue, expiration);
        }

        /// <summary>
        /// Refreshs the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task RefreshAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            await this.RemoveAsync(cacheKey);
            await this.SetAsync(cacheKey, cacheValue, expiration);
        }

        /// <summary>
        /// Removes cached item by cachekey's prefix.
        /// </summary>
        /// <returns>The by prefix async.</returns>
        /// <param name="prefix">Prefix.</param>
        public void RemoveByPrefix(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            var local = _providers.FirstOrDefault();

            local.RemoveByPrefix(prefix);

            //remote
            foreach (var provider in _providers.Skip(1))
            {
                provider.RemoveByPrefix(prefix);
            }   
        }

        /// <summary>
        /// Removes cached item by cachekey's prefix async.
        /// </summary>
        /// <returns>The by prefix async.</returns>
        /// <param name="prefix">Prefix.</param>
        public async Task RemoveByPrefixAsync(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            var local = _providers.FirstOrDefault();

            await local.RemoveByPrefixAsync(prefix);

            //remote
            foreach (var provider in _providers.Skip(1))
            {
                provider.RemoveByPrefix(prefix);
            }                        
        }

        /// <summary>
        /// Sets all.
        /// </summary>
        /// <param name="values">Values.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void SetAll<T>(IDictionary<string, T> values, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

            var local = _providers.FirstOrDefault();

            local.SetAll(values, expiration);

            //remote
            foreach (var provider in _providers.Skip(1))
            {
                provider.SetAll(values, expiration);
            }   
        }

        /// <summary>
        /// Sets all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="values">Values.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task SetAllAsync<T>(IDictionary<string, T> values, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

            var local = _providers.FirstOrDefault();

            await local.SetAllAsync(values, expiration);

            //remote
            foreach (var provider in _providers.Skip(1))
            {
                provider.SetAll(values, expiration);
            }                       
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>The all.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public IDictionary<string, CacheValue<T>> GetAll<T>(IEnumerable<string> cacheKeys) where T : class
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var local = _providers.FirstOrDefault();

            var localDict = local.GetAll<T>(cacheKeys);

            //not find in local caching.
            var localNotFindKeys = localDict.Where(x => !x.Value.HasValue).Select(x => x.Key);

            if (!localNotFindKeys.Any())
            {
                return localDict;
            }

            foreach (var item in localNotFindKeys)
                localDict.Remove(item);

            //remote
            foreach (var provider in _providers.Skip(1))
            {
                var disDict = provider.GetAll<T>(localNotFindKeys);
                localDict.Concat(disDict).ToDictionary(k => k.Key, v => v.Value);
            }

            return localDict;                      
        }

        /// <summary>
        /// Gets all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task<IDictionary<string, CacheValue<T>>> GetAllAsync<T>(IEnumerable<string> cacheKeys) where T : class
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var local = _providers.FirstOrDefault();

            var localDict = await local.GetAllAsync<T>(cacheKeys);

            //not find in local caching.
            var localNotFindKeys = localDict.Where(x => !x.Value.HasValue).Select(x => x.Key);

            if (!localNotFindKeys.Any())
            {
                return localDict;
            }

            foreach (var item in localNotFindKeys)
                localDict.Remove(item);

            //remote
            foreach (var provider in _providers.Skip(1))
            {
                var disDict = provider.GetAll<T>(localNotFindKeys);
                localDict.Concat(disDict).ToDictionary(k => k.Key, v => v.Value);
            }

            return localDict;    
        }

        /// <summary>
        /// Gets the by prefix.
        /// </summary>
        /// <returns>The by prefix.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public IDictionary<string, CacheValue<T>> GetByPrefix<T>(string prefix) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            var local = _providers.FirstOrDefault();

            var localDict = local.GetByPrefix<T>(prefix);

            //not find in local caching.
            var localNotFindKeys = localDict.Where(x => !x.Value.HasValue).Select(x => x.Key);

            if (!localNotFindKeys.Any())
            {
                return localDict;
            }
         
            foreach (var item in localNotFindKeys)
                localDict.Remove(item);

            //remote
            foreach (var provider in _providers.Skip(1))
            {
                var disDict = provider.GetAll<T>(localNotFindKeys);
                localDict.Concat(disDict).ToDictionary(k => k.Key, v => v.Value);
            }

            return localDict;          
        }

        /// <summary>
        /// Gets the by prefix async.
        /// </summary>
        /// <returns>The by prefix async.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task<IDictionary<string, CacheValue<T>>> GetByPrefixAsync<T>(string prefix) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            var local = _providers.FirstOrDefault();

            var localDict = await local.GetByPrefixAsync<T>(prefix);

            //not find in local caching.
            var localNotFindKeys = localDict.Where(x => !x.Value.HasValue).Select(x => x.Key);

            if (!localNotFindKeys.Any())
            {
                return localDict;
            }

            foreach (var item in localNotFindKeys)
                localDict.Remove(item);

            //remote
            foreach (var provider in _providers.Skip(1))
            {
                var disDict = provider.GetAll<T>(localNotFindKeys);
                localDict.Concat(disDict).ToDictionary(k => k.Key, v => v.Value);
            }

            return localDict;
        }

        /// <summary>
        /// Removes all.
        /// </summary>
        /// <param name="cacheKeys">Cache keys.</param>
        public void RemoveAll(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));                      

            var local = _providers.FirstOrDefault();

            local.RemoveAll(cacheKeys);
                      
            //remote
            foreach (var provider in _providers.Skip(1))
            {
                provider.RemoveAll(cacheKeys);
            }
        }

        /// <summary>
        /// Removes all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        public async Task RemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var local = _providers.FirstOrDefault();

            await local.RemoveAllAsync(cacheKeys);

            //remote
            foreach (var provider in _providers.Skip(1))
            {
                provider.RemoveAll(cacheKeys);
            }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <returns>The count.</returns>
        /// <param name="prefix">Prefix.</param>
        public int GetCount(string prefix = "")
        {
            var list = new List<int>();

            foreach (var provider in _providers)
            {
                list.Add(provider.GetCount(prefix));
            }

            return list.OrderByDescending(x => x).FirstOrDefault();
        }

        /// <summary>
        /// Flush All Cached Item.
        /// </summary>
        public void Flush()
        {
            foreach (var provider in _providers)
            {
                provider.Flush();
            }
        }

        /// <summary>
        /// Flush All Cached Item async.
        /// </summary>
        /// <returns>The async.</returns>
        public async Task FlushAsync()
        {
            var tasks = new List<Task>();

            foreach (var provider in _providers)
            {
                tasks.Add(provider.FlushAsync());
            }

            await Task.WhenAll(tasks);                   
        }
    }
}
