﻿using EasyCaching.Core.DistributedLock;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace EasyCaching.UnitTests.DistributedLock
{
    public abstract class DistributedLockTest
    {
        private readonly string _name;
        private readonly IDistributedLockFactory _lockFactory;
        private readonly ITestOutputHelper _output;

        protected DistributedLockTest(string name, IDistributedLockFactory lockFactory, ITestOutputHelper output)
        {
            _name = name;
            _lockFactory = lockFactory;

            _output = output;
        }

        [Fact]
        public void Lock_Release()
        {
            var @lock = _lockFactory.CreateLock(_name, Guid.NewGuid().ToString());
            try
            {
                Assert.True(@lock.Lock(60000));
            }
            finally
            {
                @lock.Release();
            }
        }

        [Fact]
        public async Task LockAsync_ReleaseAsync()
        {
            var @lock = _lockFactory.CreateLock(_name, Guid.NewGuid().ToString());
            try
            {
                Assert.True(await @lock.LockAsync(60000));
            }
            finally
            {
                await @lock.ReleaseAsync();
            }
        }

        [Fact]
        public Task Lock_After_lock()
        {
            var key = Guid.NewGuid().ToString();
            const int timeout = 1000;

            var handle = new AutoResetEvent(false);

            return Task.WhenAll(Task.Run(() => Lock_After_lock1(key, timeout, handle)), Task.Run(() => Lock_After_lock2(key, timeout, handle)));
        }

        private async Task Lock_After_lock1(string key, int timeout, EventWaitHandle handle)
        {
            var @lock = _lockFactory.CreateLock(_name, key);
            try
            {
                Assert.True(await @lock.LockAsync(timeout));

                handle.Set();

                await Task.Delay(100);

                handle.WaitOne(10000);
            }
            finally
            {
                await @lock.ReleaseAsync();
            }
        }

        private async Task Lock_After_lock2(string key, int timeout, EventWaitHandle handle)
        {
            var @lock = _lockFactory.CreateLock(_name, key);
            try
            {
                handle.WaitOne(10000);

                Assert.False(await @lock.LockAsync(timeout));
            }
            finally
            {
                handle.Set();
            }
        }

        [Fact]
        public async Task Repeat_Lock()
        {
            using (var lck = _lockFactory.CreateLock(_name, Guid.NewGuid().ToString()))
            using (var lck2 = _lockFactory.CreateLock(_name, Guid.NewGuid().ToString()))
            {
                Assert.True(await lck.LockAsync(200));
                Assert.True(await lck2.LockAsync(200));

                try
                {
                    Assert.False(await lck.LockAsync(200));

                    Assert.True(false, "Fail");
                }
                catch (DistributedLockException ex)
                {
                    Assert.Equal("锁释放前请不要重复锁", ex.Message);
                }

                await lck.ReleaseAsync();

                Assert.True(await lck.LockAsync(200));
            }
        }

        [Fact]
        public async Task Multi_Thread_Lock()
        {
            const int timeout = 200;

            var lck = _lockFactory.CreateLock(_name, Guid.NewGuid().ToString());
            var handle = new AutoResetEvent(false);

            _ = Task.Run(async () =>
            {
                _output.WriteLine("Start Lock1");

                await lck.LockAsync(timeout);

                handle.Set();

                await Task.Delay(100);

                _output.WriteLine("Start release1");

                await lck.ReleaseAsync();

                handle.Set();
            });

            try
            {
                handle.WaitOne(10000);

                _output.WriteLine("Start Lock2");

                Assert.False(await lck.LockAsync(timeout));

                Assert.True(false, "Fail");
            }
            catch (DistributedLockException ex)
            {
                Assert.Equal("锁释放前请不要重复锁", ex.Message);
            }
            finally
            {
                handle.WaitOne(10000);

                _output.WriteLine("Start Lock3");

                Assert.True(await lck.LockAsync(timeout));

                _output.WriteLine("Start release3");

                await lck.ReleaseAsync();
            }
        }

        [Fact]
        public async Task Lock_With_CancellationToken()
        {
            using (var lock1 = _lockFactory.CreateLock(_name, nameof(Lock_With_CancellationToken)))
            using (var lock2 = _lockFactory.CreateLock(_name, nameof(Lock_With_CancellationToken)))
            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(1500);

                Assert.True(await lock1.LockAsync(100, cts.Token));

                Assert.Throws<OperationCanceledException>(() => lock2.Lock(2000, cts.Token));
            }
        }
    }

    public abstract class DistributedLockV2Test
    {
        protected IDistributedLockFactory _lockFactory;
        private readonly ITestOutputHelper _output;

        protected DistributedLockV2Test(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Lock_Release()
        {
            var @lock = _lockFactory.CreateLock(Guid.NewGuid().ToString());
            try
            {
                Assert.True(@lock.Lock(60000));
            }
            finally
            {
                @lock.Release();
            }
        }

        [Fact]
        public async Task LockAsync_ReleaseAsync()
        {
            var @lock = _lockFactory.CreateLock(Guid.NewGuid().ToString());
            try
            {
                Assert.True(await @lock.LockAsync(60000));
            }
            finally
            {
                await @lock.ReleaseAsync();
            }
        }

        [Fact]
        public Task Lock_After_lock()
        {
            var key = Guid.NewGuid().ToString();
            const int timeout = 1000;

            var handle = new AutoResetEvent(false);

            return Task.WhenAll(Task.Run(() => Lock_After_lock1(key, timeout, handle)), Task.Run(() => Lock_After_lock2(key, timeout, handle)));
        }

        private async Task Lock_After_lock1(string key, int timeout, EventWaitHandle handle)
        {
            var @lock = _lockFactory.CreateLock(key);
            try
            {
                Assert.True(await @lock.LockAsync(timeout));

                handle.Set();

                await Task.Delay(100);

                handle.WaitOne(10000);
            }
            finally
            {
                await @lock.ReleaseAsync();
            }
        }

        private async Task Lock_After_lock2(string key, int timeout, EventWaitHandle handle)
        {
            var @lock = _lockFactory.CreateLock(key);
            try
            {
                handle.WaitOne(10000);

                Assert.False(await @lock.LockAsync(timeout));
            }
            finally
            {
                handle.Set();
            }
        }

        [Fact]
        public async Task Repeat_Lock()
        {
            using (var lck = _lockFactory.CreateLock(Guid.NewGuid().ToString()))
            using (var lck2 = _lockFactory.CreateLock(Guid.NewGuid().ToString()))
            {
                Assert.True(await lck.LockAsync(200));
                Assert.True(await lck2.LockAsync(200));

                try
                {
                    Assert.False(await lck.LockAsync(200));

                    Assert.True(false, "Fail");
                }
                catch (DistributedLockException ex)
                {
                    Assert.Equal("锁释放前请不要重复锁", ex.Message);
                }

                await lck.ReleaseAsync();

                Assert.True(await lck.LockAsync(200));
            }
        }

        [Fact]
        public async Task Multi_Thread_Lock()
        {
            const int timeout = 200;

            var lck = _lockFactory.CreateLock(Guid.NewGuid().ToString());
            var handle = new AutoResetEvent(false);

            _ = Task.Run(async () =>
            {
                _output.WriteLine("Start Lock1");

                await lck.LockAsync(timeout);

                handle.Set();

                await Task.Delay(100);

                _output.WriteLine("Start release1");

                await lck.ReleaseAsync();

                handle.Set();
            });

            try
            {
                handle.WaitOne(10000);

                _output.WriteLine("Start Lock2");

                Assert.False(await lck.LockAsync(timeout));

                Assert.True(false, "Fail");
            }
            catch (DistributedLockException ex)
            {
                Assert.Equal("锁释放前请不要重复锁", ex.Message);
            }
            finally
            {
                handle.WaitOne(10000);

                _output.WriteLine("Start Lock3");

                Assert.True(await lck.LockAsync(timeout));

                _output.WriteLine("Start release3");

                await lck.ReleaseAsync();
            }
        }

        [Fact]
        public async Task Lock_With_CancellationToken()
        {
            using (var lock1 = _lockFactory.CreateLock(nameof(Lock_With_CancellationToken)))
            using (var lock2 = _lockFactory.CreateLock(nameof(Lock_With_CancellationToken)))
            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(1500);

                Assert.True(await lock1.LockAsync(100, cts.Token));

                Assert.Throws<OperationCanceledException>(() => lock2.Lock(2000, cts.Token));
            }
        }
    }
}
