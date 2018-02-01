namespace EasyCaching.UnitTests
{
    using Dapper;
    using EasyCaching.SQLite;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using Xunit;
    using System.Threading.Tasks;

    public class SQLiteCachingTest : BaseCachingProviderTest
    {
        private readonly ISQLiteDatabaseProvider _dbProvider;

        public SQLiteCachingTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSQLiteCache(options =>
            {
                options.FileName = "";
                options.FilePath = "";
                options.CacheMode = Microsoft.Data.Sqlite.SqliteCacheMode.Default;
                options.OpenMode = Microsoft.Data.Sqlite.SqliteOpenMode.Memory;
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _dbProvider = serviceProvider.GetService<ISQLiteDatabaseProvider>();

            var conn = _dbProvider.GetConnection();
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            conn.Execute(ConstSQL.CREATESQL);

            _provider = new SQLiteCachingProvider(_dbProvider);
            _defaultTs = TimeSpan.FromSeconds(50);
        }

        [Fact]
        public void Get_Cached_Value_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            _provider.Set(cacheKey, cacheValue, _defaultTs);
            var val = _provider.Get<string>(cacheKey, null, _defaultTs);

            Assert.Equal(cacheValue, val.Value);
        }

        [Fact]
        public async Task Get_Cached_Value_Async_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);
            var val = await _provider.GetAsync<string>(cacheKey, null, _defaultTs);

            Assert.Equal(cacheValue, val.Value);
        }

        [Fact]
        public void Get_Cached_Value_Without_Retriever_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            _provider.Set(cacheKey, cacheValue, _defaultTs);
            var val = _provider.Get<string>(cacheKey);

            Assert.Equal(cacheValue, val.Value);
        }

        [Fact]
        public async Task Get_Cached_Value_Without_Retriever_Async_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);
            var val = await _provider.GetAsync<string>(cacheKey);

            Assert.Equal(cacheValue, val.Value);
        }

        [Fact]
        public void Get_Not_Cached_Value_Without_Retriever_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var val = _provider.Get<string>(cacheKey);

            Assert.Equal(default(string), val.Value);
        }

        [Fact]
        public async Task Get_Not_Cached_Value_Without_Retriever_Async_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var val = await _provider.GetAsync<string>(cacheKey);

            Assert.Equal(default(string), val.Value);
        }

        [Fact]
        public void Remove_Cached_Value_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            _provider.Set(cacheKey, cacheValue, _defaultTs);
            var valBeforeRemove = _provider.Get<string>(cacheKey, null, _defaultTs);
            Assert.NotNull(valBeforeRemove);

            _provider.Remove(cacheKey);
            var valAfterRemove = _provider.Get(cacheKey, () => "123", _defaultTs);
            Assert.Equal("123", valAfterRemove.Value);
        }

        [Fact]
        public async Task Remove_Cached_Value_Async_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);
            var valBeforeRemove = await _provider.GetAsync<string>(cacheKey, null, _defaultTs);
            Assert.NotNull(valBeforeRemove);

            await _provider.RemoveAsync(cacheKey);
            var valAfterRemove = await _provider.GetAsync<string>(cacheKey, async () => await Task.FromResult("123"), _defaultTs);
            Assert.Equal("123", valAfterRemove.Value);
        }


        [Fact]
        public void Refresh_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var tmp = _provider.Get<string>(cacheKey);
            Assert.Equal("value", tmp.Value);

            _provider.Refresh(cacheKey, "NewValue", _defaultTs);

            var act = _provider.Get<string>(cacheKey);

            Assert.Equal("NewValue", act.Value);
        }

        [Fact]
        public async Task Refresh_Async_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);

            var tmp = await _provider.GetAsync<string>(cacheKey);
            Assert.Equal("value", tmp.Value);

            await _provider.RefreshAsync(cacheKey, "NewValue", _defaultTs);

            var act = await _provider.GetAsync<string>(cacheKey);

            Assert.Equal("NewValue", act.Value);
        }

        [Fact]
        public void RemoveByPrefix_Should_Succeed()
        {
            var cacheKey1 = "PREFIX:1";
            var cacheValue1 = "value1";
            _provider.Set(cacheKey1, cacheValue1, _defaultTs);

            var cacheKey2 = "PREFIX:2";
            var cacheValue2 = "value2";
            _provider.Set(cacheKey2, cacheValue2, _defaultTs);


            var tmpRes1 = _provider.Get<string>(cacheKey1);
            var tmpRes2 = _provider.Get<string>(cacheKey2);

            Assert.Equal(cacheValue1, tmpRes1.Value);
            Assert.Equal(cacheValue2, tmpRes2.Value);

            _provider.RemoveByPrefix("PREFIX");

            var res1 = _provider.Get<string>(cacheKey1);
            var res2 = _provider.Get<string>(cacheKey2);

            Assert.False(res1.HasValue);
            Assert.False(res2.HasValue);
        }

        [Fact]
        public async Task RemoveByPrefixAsync_Should_Succeed()
        {
            var cacheKey1 = "PREFIX:1";
            var cacheValue1 = "value1";
            await _provider.SetAsync(cacheKey1, cacheValue1, _defaultTs);

            var cacheKey2 = "PREFIX:2";
            var cacheValue2 = "value2";
            await _provider.SetAsync(cacheKey2, cacheValue2, _defaultTs);


            var tmpRes1 = await _provider.GetAsync<string>(cacheKey1);
            var tmpRes2 = await _provider.GetAsync<string>(cacheKey2);

            Assert.Equal(cacheValue1, tmpRes1.Value);
            Assert.Equal(cacheValue2, tmpRes2.Value);

            await _provider.RemoveByPrefixAsync("PREFIX");

            var res1 = await _provider.GetAsync<string>(cacheKey1);
            var res2 = await _provider.GetAsync<string>(cacheKey2);

            Assert.False(res1.HasValue);
            Assert.False(res2.HasValue);
        }
    }
}
