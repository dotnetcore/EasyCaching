﻿using EasyCaching.Core.DistributedLock;

namespace EasyCaching.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.Diagnostics;

    public abstract class EasyCachingAbstractProvider : IEasyCachingProvider
    {
        protected static readonly DiagnosticListener s_diagnosticListener =
                    new DiagnosticListener(EasyCachingDiagnosticListenerExtensions.DiagnosticListenerName);

        private readonly IDistributedLockFactory _lockFactory;
        private readonly BaseProviderOptions _options;

        protected string ProviderName { get; set; }
        protected bool IsDistributedProvider { get; set; }
        protected int ProviderMaxRdSecond { get; set; }
        protected CachingProviderType ProviderType { get; set; }
        protected CacheStats ProviderStats { get; set; }

        public string Name => this.ProviderName;
        public bool IsDistributedCache => this.IsDistributedProvider;
        public bool UseLock => _lockFactory != null;
        public int MaxRdSecond => this.ProviderMaxRdSecond;
        public CachingProviderType CachingProviderType => this.ProviderType;
        public CacheStats CacheStats => this.ProviderStats;

        public object Database => BaseGetDatabase();

        protected EasyCachingAbstractProvider() { }

        protected EasyCachingAbstractProvider(IDistributedLockFactory lockFactory, BaseProviderOptions options)
        {
            _lockFactory = lockFactory;
            _options = options;
        }

        public abstract object BaseGetDatabase();
        public abstract bool BaseExists(string cacheKey);
        public abstract Task<bool> BaseExistsAsync(string cacheKey, CancellationToken cancellationToken = default);
        public abstract void BaseFlush();
        public abstract Task BaseFlushAsync(CancellationToken cancellationToken = default);
        public abstract CacheValue<T> BaseGet<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration);
        public abstract CacheValue<T> BaseGet<T>(string cacheKey);
        public abstract IEnumerable<string> BaseGetAllKeys(string prefix = "");
        public abstract Task<IEnumerable<string>> BaseGetAllKeysAsync(string prefix = "", CancellationToken cancellationToken = default);
        public abstract IDictionary<string, CacheValue<T>> BaseGetAll<T>(IEnumerable<string> cacheKeys);
        public abstract IDictionary<string, CacheValue<T>> BaseGetAll<T>(string prefix ="");
        public abstract Task<IDictionary<string, CacheValue<T>>> BaseGetAllAsync<T>(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default);
        public abstract Task<IDictionary<string, CacheValue<T>>> BaseGetAllAsync<T>(string prefix ="", CancellationToken cancellationToken = default);
        public abstract Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration, CancellationToken cancellationToken = default);
        public abstract Task<object> BaseGetAsync(string cacheKey, Type type, CancellationToken cancellationToken = default);
        public abstract Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey, CancellationToken cancellationToken = default);
        public abstract IDictionary<string, CacheValue<T>> BaseGetByPrefix<T>(string prefix);
        public abstract Task<IDictionary<string, CacheValue<T>>> BaseGetByPrefixAsync<T>(string prefix, CancellationToken cancellationToken = default);
        public abstract int BaseGetCount(string prefix = "");
        public abstract Task<int> BaseGetCountAsync(string prefix = "", CancellationToken cancellationToken = default);
        public abstract void BaseRemove(string cacheKey);
        public abstract void BaseRemoveAll(IEnumerable<string> cacheKeys);
        public abstract Task BaseRemoveAllAsync(IEnumerable<string> cacheKeys, CancellationToken cancellation = default);
        public abstract Task BaseRemoveAsync(string cacheKey, CancellationToken cancellationToken = default);
        public abstract void BaseRemoveByPrefix(string prefix);
        public abstract Task BaseRemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
        public abstract void BaseRemoveByPattern(string pattern);
        public abstract Task BaseRemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
        public abstract void BaseSet<T>(string cacheKey, T cacheValue, TimeSpan expiration);
        public abstract void BaseSetAll<T>(IDictionary<string, T> values, TimeSpan expiration);
        public abstract Task BaseSetAllAsync<T>(IDictionary<string, T> values, TimeSpan expiration, CancellationToken cancellationToken = default);
        public abstract Task BaseSetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration, CancellationToken cancellationToken = default);
        public abstract bool BaseTrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration);
        public abstract Task<bool> BaseTrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration, CancellationToken cancellationToken = default);

        public abstract TimeSpan BaseGetExpiration(string cacheKey);
        public abstract Task<TimeSpan> BaseGetExpirationAsync(string cacheKey, CancellationToken cancellationToken = default);
        public abstract ProviderInfo BaseGetProviderInfo();

        public bool Exists(string cacheKey)
        {
            var operationId = s_diagnosticListener.WriteExistsCacheBefore(new BeforeExistsRequestEventData(CachingProviderType.ToString(), Name, nameof(Exists), cacheKey));
            Exception e = null;
            try
            {
                return BaseExists(cacheKey);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteExistsCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteExistsCacheAfter(operationId);
                }
            }
        }

        public async Task<bool> ExistsAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteExistsCacheBefore(new BeforeExistsRequestEventData(CachingProviderType.ToString(), Name, nameof(ExistsAsync), cacheKey));
            Exception e = null;
            try
            {
                var flag = await BaseExistsAsync(cacheKey, cancellationToken);
                return flag;
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteExistsCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteExistsCacheAfter(operationId);
                }
            }
        }

        public void Flush()
        {
            var operationId = s_diagnosticListener.WriteFlushCacheBefore(new EventData(CachingProviderType.ToString(), Name, nameof(Flush)));
            Exception e = null;
            try
            {
                BaseFlush();
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteFlushCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteFlushCacheAfter(operationId);
                }
            }
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteFlushCacheBefore(new EventData(CachingProviderType.ToString(), Name, nameof(FlushAsync)));
            Exception e = null;
            try
            {
                await BaseFlushAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteFlushCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteFlushCacheAfter(operationId);
                }
            }
        }

        public CacheValue<T> Get<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(Get), new[] { cacheKey }, expiration));
            Exception e = null;
            try
            {
                if (_lockFactory == null) return BaseGet<T>(cacheKey, dataRetriever, expiration);

                var value = BaseGet<T>(cacheKey);
                if (value.HasValue) return value;

                using (var @lock = _lockFactory.CreateLock(Name, $"{cacheKey}_Lock"))
                {
                    if (!@lock.Lock(_options.SleepMs)) throw new TimeoutException();

                    value = BaseGet<T>(cacheKey);
                    if (value.HasValue) return value;

                    var item = dataRetriever();
                    if (item != null || _options.CacheNulls)
                    {
                        BaseSet(cacheKey, item, expiration);

                        return new CacheValue<T>(item, true);
                    }
                    else
                    {
                        return CacheValue<T>.NoValue;
                    }
                }
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteGetCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteGetCacheAfter(operationId);
                }
            }
        }

        public CacheValue<T> Get<T>(string cacheKey)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(Get), new[] { cacheKey }));
            Exception e = null;
            try
            {
                return BaseGet<T>(cacheKey);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteGetCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteGetCacheAfter(operationId);
                }
            }
        }
        
        public IEnumerable<string> GetAllKeys(string prefix = "")
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(GetAllKeys), null));
            Exception e = null;
            try
            {
                return BaseGetAllKeys(prefix);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteGetCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteGetCacheAfter(operationId);
                } 
            }        
        }
        
        public async Task<IEnumerable<string>> GetAllKeysAsync(string prefix = "", CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(GetAllKeysAsync), null));
            Exception e = null;
            try
            {
                return await BaseGetAllKeysAsync(prefix, cancellationToken);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteGetCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteGetCacheAfter(operationId);
                } 
            }        
        }

        public IDictionary<string, CacheValue<T>> GetAll<T>(IEnumerable<string> cacheKeys)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(GetAll), cacheKeys.ToArray()));
            Exception e = null;
            try
            {
                return BaseGetAll<T>(cacheKeys);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteGetCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteGetCacheAfter(operationId);
                }
            }
        }

        public IDictionary<string, CacheValue<object>> GetAll(string prefix = "")
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(GetAll), null));
            Exception e = null;
            try
            {
                return BaseGetAll<object>(prefix);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteGetCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteGetCacheAfter(operationId);
                } 
            }        
        }
        
        public async Task<IDictionary<string, CacheValue<object>>> GetAllAsync(string prefix = "", CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(GetAllAsync), null));
            Exception e = null;
            try
            {
                return await BaseGetAllAsync<object>(prefix, cancellationToken);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteGetCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteGetCacheAfter(operationId);
                }
            }
        }

        public async Task<IDictionary<string, CacheValue<T>>> GetAllAsync<T>(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(GetAllAsync), cacheKeys.ToArray()));
            Exception e = null;
            try
            {
                return await BaseGetAllAsync<T>(cacheKeys, cancellationToken);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteGetCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteGetCacheAfter(operationId);
                }
            }
        }

        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(GetAsync), new[] { cacheKey }, expiration));
            Exception e = null;
            try
            {
                if (_lockFactory == null) return await BaseGetAsync<T>(cacheKey, dataRetriever, expiration, cancellationToken);

                var value = await BaseGetAsync<T>(cacheKey);
                if (value.HasValue) return value;

                var @lock = _lockFactory.CreateLock(Name, $"{cacheKey}_Lock");
                try
                {
                    if (!await @lock.LockAsync(_options.SleepMs)) throw new TimeoutException();

                    value = await BaseGetAsync<T>(cacheKey, cancellationToken);
                    if (value.HasValue) return value;

                    var task = dataRetriever();
                    if (!task.IsCompleted &&
                        await Task.WhenAny(task, Task.Delay(_options.LockMs)) != task)
                        throw new TimeoutException();

                    var item = await task;
                    if (item != null || _options.CacheNulls)
                    {
                        await BaseSetAsync(cacheKey, item, expiration, cancellationToken);

                        return new CacheValue<T>(item, true);
                    }
                    else
                    {
                        return CacheValue<T>.NoValue;
                    }
                }
                finally
                {
                    await @lock.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteGetCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteGetCacheAfter(operationId);
                }
            }
        }

        public async Task<object> GetAsync(string cacheKey, Type type, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, "GetAsync_Type", new[] { cacheKey }));
            Exception e = null;
            try
            {
                return await BaseGetAsync(cacheKey, type, cancellationToken);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteGetCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteGetCacheAfter(operationId);
                }
            }
        }

        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(GetAsync), new[] { cacheKey }));
            Exception e = null;
            try
            {
                return await BaseGetAsync<T>(cacheKey, cancellationToken);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteGetCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteGetCacheAfter(operationId);
                }
            }
        }

        public IDictionary<string, CacheValue<T>> GetByPrefix<T>(string prefix)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(GetByPrefix), new[] { prefix }));
            Exception e = null;
            try
            {
                return BaseGetByPrefix<T>(prefix);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteGetCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteGetCacheAfter(operationId);
                }
            }
        }

        public async Task<IDictionary<string, CacheValue<T>>> GetByPrefixAsync<T>(string prefix, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteGetCacheBefore(new BeforeGetRequestEventData(CachingProviderType.ToString(), Name, nameof(GetByPrefixAsync), new[] { prefix }));
            Exception e = null;
            try
            {
                return await BaseGetByPrefixAsync<T>(prefix, cancellationToken);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteGetCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteGetCacheAfter(operationId);
                }
            }
        }

        public int GetCount(string prefix = "")
        {
            return BaseGetCount(prefix);
        }

        public async Task<int> GetCountAsync(string prefix = "", CancellationToken cancellationToken = default)
        {
            return await BaseGetCountAsync(prefix, cancellationToken);
        }

        public void Remove(string cacheKey)
        {
            var operationId = s_diagnosticListener.WriteRemoveCacheBefore(new BeforeRemoveRequestEventData(CachingProviderType.ToString(), Name, nameof(Remove), new[] { cacheKey }));
            Exception e = null;
            try
            {
                BaseRemove(cacheKey);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteRemoveCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteRemoveCacheAfter(operationId);
                }
            }
        }

        public void RemoveAll(IEnumerable<string> cacheKeys)
        {
            var operationId = s_diagnosticListener.WriteRemoveCacheBefore(new BeforeRemoveRequestEventData(CachingProviderType.ToString(), Name, nameof(RemoveAll), cacheKeys.ToArray()));
            Exception e = null;
            try
            {
                BaseRemoveAll(cacheKeys);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteRemoveCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteRemoveCacheAfter(operationId);
                }
            }
        }

        public async Task RemoveAllAsync(IEnumerable<string> cacheKeys, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteRemoveCacheBefore(new BeforeRemoveRequestEventData(CachingProviderType.ToString(), Name, nameof(RemoveAllAsync), cacheKeys.ToArray()));
            Exception e = null;
            try
            {
                await BaseRemoveAllAsync(cacheKeys, cancellationToken);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteRemoveCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteRemoveCacheAfter(operationId);
                }
            }
        }

        public async Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteRemoveCacheBefore(new BeforeRemoveRequestEventData(CachingProviderType.ToString(), Name, nameof(RemoveAsync), new[] { cacheKey }));
            Exception e = null;
            try
            {
                await BaseRemoveAsync(cacheKey, cancellationToken);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteRemoveCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteRemoveCacheAfter(operationId);
                }
            }
        }

        public void RemoveByPrefix(string prefix)
        {
            var operationId = s_diagnosticListener.WriteRemoveCacheBefore(new BeforeRemoveRequestEventData(CachingProviderType.ToString(), Name, nameof(RemoveByPrefix), new[] { prefix }));
            Exception e = null;
            try
            {
                BaseRemoveByPrefix(prefix);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteRemoveCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteRemoveCacheAfter(operationId);
                }
            }
        }

        public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteRemoveCacheBefore(new BeforeRemoveRequestEventData(CachingProviderType.ToString(), Name, nameof(RemoveByPrefixAsync), new[] { prefix }));
            Exception e = null;
            try
            {
                await BaseRemoveByPrefixAsync(prefix, cancellationToken);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteRemoveCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteRemoveCacheAfter(operationId);
                }
            }
        }

        public void RemoveByPattern(string pattern)
        {
            var operationId = s_diagnosticListener.WriteRemoveCacheBefore(
                new BeforeRemoveRequestEventData(CachingProviderType.ToString(), Name, nameof(RemoveByPattern),
                    new[] { pattern }));
            Exception e = null;
            try
            {
                BaseRemoveByPattern(pattern);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteRemoveCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteRemoveCacheAfter(operationId);
                }
            }
        }

        public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteRemoveCacheBefore(
                new BeforeRemoveRequestEventData(CachingProviderType.ToString(), Name, nameof(RemoveByPatternAsync),
                    new[] { pattern }));
            Exception e = null;
            try
            {
                await BaseRemoveByPatternAsync(pattern, cancellationToken);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteRemoveCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteRemoveCacheAfter(operationId);
                }
            }
        }

        public void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            var operationId = s_diagnosticListener.WriteSetCacheBefore(new BeforeSetRequestEventData(CachingProviderType.ToString(), Name, nameof(Set), new Dictionary<string, object> { { cacheKey, cacheValue } }, expiration));
            Exception e = null;
            try
            {
                BaseSet(cacheKey, cacheValue, expiration);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteSetCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteSetCacheAfter(operationId);
                }
            }
        }

        public void SetAll<T>(IDictionary<string, T> value, TimeSpan expiration)
        {
            var operationId = s_diagnosticListener.WriteSetCacheBefore(new BeforeSetRequestEventData(CachingProviderType.ToString(), Name, nameof(SetAll), value.ToDictionary(k => k.Key, v => (object)v.Value), expiration));
            Exception e = null;
            try
            {
                BaseSetAll(value, expiration);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteSetCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteSetCacheAfter(operationId);
                }
            }
        }

        public async Task SetAllAsync<T>(IDictionary<string, T> value, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteSetCacheBefore(new BeforeSetRequestEventData(CachingProviderType.ToString(), Name, nameof(SetAllAsync), value.ToDictionary(k => k.Key, v => (object)v.Value), expiration));
            Exception e = null;
            try
            {
                await BaseSetAllAsync(value, expiration, cancellationToken);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteSetCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteSetCacheAfter(operationId);
                }
            }
        }

        public async Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteSetCacheBefore(new BeforeSetRequestEventData(CachingProviderType.ToString(), Name, nameof(SetAsync), new Dictionary<string, object> { { cacheKey, cacheValue } }, expiration));
            Exception e = null;
            try
            {
                await BaseSetAsync(cacheKey, cacheValue, expiration, cancellationToken);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteSetCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteSetCacheAfter(operationId);
                }
            }
        }

        public bool TrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            var operationId = s_diagnosticListener.WriteSetCacheBefore(new BeforeSetRequestEventData(CachingProviderType.ToString(), Name, nameof(TrySet), new Dictionary<string, object> { { cacheKey, cacheValue } }, expiration));
            Exception e = null;
            try
            {
                return BaseTrySet(cacheKey, cacheValue, expiration);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteSetCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteSetCacheAfter(operationId);
                }
            }
        }

        public async Task<bool> TrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var operationId = s_diagnosticListener.WriteSetCacheBefore(new BeforeSetRequestEventData(CachingProviderType.ToString(), Name, nameof(TrySetAsync), new Dictionary<string, object> { { cacheKey, cacheValue } }, expiration));
            Exception e = null;
            try
            {
                return await BaseTrySetAsync(cacheKey, cacheValue, expiration, cancellationToken);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WriteSetCacheError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WriteSetCacheAfter(operationId);
                }
            }
        }

        public TimeSpan GetExpiration(string cacheKey)
        {
            return BaseGetExpiration(cacheKey);
        }

        public async Task<TimeSpan> GetExpirationAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            return await BaseGetExpirationAsync(cacheKey, cancellationToken);
        }

        public ProviderInfo GetProviderInfo()
        {
            return BaseGetProviderInfo();
        }

        protected SearchKeyPattern ProcessSearchKeyPattern(string pattern)
        {
            var postfix = pattern.StartsWith("*");
            var prefix = pattern.EndsWith("*");

            var contains = postfix && prefix;

            if (contains)
            {
                return SearchKeyPattern.Contains;
            }

            if (postfix)
            {
                return SearchKeyPattern.Postfix;
            }

            if (prefix)
            {
                return SearchKeyPattern.Prefix;
            }

            return SearchKeyPattern.Exact;
        }

        protected string HandleSearchKeyPattern(string pattern)
        {
            return pattern.Replace("*", string.Empty);
        } 
    }
}
