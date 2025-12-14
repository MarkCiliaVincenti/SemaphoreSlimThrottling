using Xunit;

namespace SemaphoreSlimThrottling.Tests
{
    public class Tests
    {
        [Fact]
        public async Task TestSemaphoreSlimThrottleWithMaxCount()
        {
            var semaphoreSlimThrottle = new SemaphoreSlimThrottle(-2, 1);
            Assert.Equal(-2, semaphoreSlimThrottle.CurrentCount);
            Assert.Equal(-2, semaphoreSlimThrottle.Release());
            Assert.Equal(-1, semaphoreSlimThrottle.CurrentCount);
            Assert.Equal(-1, semaphoreSlimThrottle.Release());
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            Assert.Equal(0, semaphoreSlimThrottle.Release());
            Assert.Equal(1, semaphoreSlimThrottle.CurrentCount);
            await semaphoreSlimThrottle.WaitAsync(Timeout.Infinite);
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            Assert.Equal(0, semaphoreSlimThrottle.Release());
            await semaphoreSlimThrottle.WaitAsync(TimeSpan.Zero);
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            Assert.Equal(0, semaphoreSlimThrottle.Release());
            await semaphoreSlimThrottle.WaitAsync(Timeout.Infinite, CancellationToken.None);
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            Assert.Equal(0, semaphoreSlimThrottle.Release());
            await semaphoreSlimThrottle.WaitAsync(TimeSpan.Zero, CancellationToken.None);
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            Assert.Equal(0, semaphoreSlimThrottle.Release());

            semaphoreSlimThrottle = new SemaphoreSlimThrottle(-2, 1);
            Assert.Equal(-2, semaphoreSlimThrottle.CurrentCount);
            Assert.Equal(-2, semaphoreSlimThrottle.Release(3));
            Assert.Equal(1, semaphoreSlimThrottle.CurrentCount);
            await semaphoreSlimThrottle.WaitAsync();
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            Assert.Equal(0, semaphoreSlimThrottle.Release());

            semaphoreSlimThrottle = new SemaphoreSlimThrottle(1, 1);
            var waitHandle = semaphoreSlimThrottle.AvailableWaitHandle;
            semaphoreSlimThrottle.Dispose();
        }

        [Fact]
        public async Task TestSemaphoreSlimThrottleWithoutMaxCount()
        {
            var semaphoreSlimThrottle = new SemaphoreSlimThrottle(1);
            Assert.Equal(1, semaphoreSlimThrottle.CurrentCount);
            semaphoreSlimThrottle.Wait();
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            semaphoreSlimThrottle.Release(1);
            Assert.Equal(1, semaphoreSlimThrottle.CurrentCount);
            semaphoreSlimThrottle.Wait(Timeout.Infinite);
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            semaphoreSlimThrottle.Release(1);
            Assert.Equal(1, semaphoreSlimThrottle.CurrentCount);
            semaphoreSlimThrottle.Wait(TimeSpan.Zero);
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            semaphoreSlimThrottle.Release(1);
            Assert.Equal(1, semaphoreSlimThrottle.CurrentCount);
            semaphoreSlimThrottle.Wait(Timeout.Infinite, CancellationToken.None);
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            semaphoreSlimThrottle.Release(1);
            Assert.Equal(1, semaphoreSlimThrottle.CurrentCount);
            semaphoreSlimThrottle.Wait(TimeSpan.Zero, CancellationToken.None);
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            semaphoreSlimThrottle.Release(1);
            Assert.Equal(1, semaphoreSlimThrottle.CurrentCount);
            semaphoreSlimThrottle.Wait(CancellationToken.None);
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            semaphoreSlimThrottle.Release(1);
            Assert.Equal(1, semaphoreSlimThrottle.CurrentCount);
            await semaphoreSlimThrottle.WaitAsync(CancellationToken.None);
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            semaphoreSlimThrottle.Dispose();
        }

        [Fact]
        public async Task ConcurrencyTest()
        {
            var semaphoreSlimThrottle = new SemaphoreSlimThrottle(-2, 100);
            semaphoreSlimThrottle.Release(52);
            var concurrency = 50;
            var tasks = Enumerable.Range(1, concurrency)
                .Select(async i =>
                {
                    await semaphoreSlimThrottle.WaitAsync();
                    semaphoreSlimThrottle.Release();
                });
            await Task.WhenAll(tasks.AsParallel());

            Assert.Equal(50, semaphoreSlimThrottle.CurrentCount);
        }

        [Fact]
        public async Task ConcurrencyTestAggressive()
        {
            var semaphoreSlimThrottle = new SemaphoreSlimThrottle(-98, 100);
            var tasks = Enumerable.Range(1, 100)
                .Select(async i =>
                {
                    semaphoreSlimThrottle.Release();
                });
            await Task.WhenAll(tasks.AsParallel());
            var concurrency = 50;
            tasks = Enumerable.Range(1, concurrency)
                .Select(async i =>
                {
                    await semaphoreSlimThrottle.WaitAsync();
                    semaphoreSlimThrottle.Release();
                });
            await Task.WhenAll(tasks.AsParallel());

            Assert.Equal(2, semaphoreSlimThrottle.CurrentCount);
        }

        [Fact]
        public async Task ConcurrencyTestNonThrottle()
        {
            var semaphoreSlimThrottle = new SemaphoreSlimThrottle(-3, 100);
            var tasks = Enumerable.Range(1, 51)
                .Select(async i =>
                {
                    await Task.Delay(1);
                    semaphoreSlimThrottle.Release();
                }).ToList().AsParallel();
            await Task.WhenAll(tasks);
            var concurrency = 50;
            tasks = Enumerable.Range(1, concurrency)
                .Select(async i =>
                {
                    await semaphoreSlimThrottle.WaitAsync();
                    semaphoreSlimThrottle.Release();
                }).ToList().AsParallel();
            await Task.WhenAll(tasks);

            Assert.Equal(48, semaphoreSlimThrottle.CurrentCount);
        }
    }
}