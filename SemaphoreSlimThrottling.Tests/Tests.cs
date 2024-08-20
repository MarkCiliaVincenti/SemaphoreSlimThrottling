using System.Threading.Tasks;
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
            await semaphoreSlimThrottle.WaitAsync();
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            Assert.Equal(0, semaphoreSlimThrottle.Release());

            semaphoreSlimThrottle = new SemaphoreSlimThrottle(-2, 1);
            Assert.Equal(-2, semaphoreSlimThrottle.CurrentCount);
            Assert.Equal(-2, semaphoreSlimThrottle.Release(3));
            Assert.Equal(1, semaphoreSlimThrottle.CurrentCount);
            await semaphoreSlimThrottle.WaitAsync();
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            Assert.Equal(0, semaphoreSlimThrottle.Release());
        }

        [Fact]
        public async Task TestSemaphoreSlimThrottleWithoutMaxCount()
        {
            var semaphoreSlimThrottle = new SemaphoreSlimThrottle(-2);
            Assert.Equal(-2, semaphoreSlimThrottle.CurrentCount);
            Assert.Equal(-2, semaphoreSlimThrottle.Release());
            Assert.Equal(-1, semaphoreSlimThrottle.CurrentCount);
            Assert.Equal(-1, semaphoreSlimThrottle.Release());
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            Assert.Equal(0, semaphoreSlimThrottle.Release());
            Assert.Equal(1, semaphoreSlimThrottle.CurrentCount);
            await semaphoreSlimThrottle.WaitAsync();
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            Assert.Equal(0, semaphoreSlimThrottle.Release());

            semaphoreSlimThrottle = new SemaphoreSlimThrottle(-2);
            Assert.Equal(-2, semaphoreSlimThrottle.CurrentCount);
            Assert.Equal(-2, semaphoreSlimThrottle.Release(3));
            Assert.Equal(1, semaphoreSlimThrottle.CurrentCount);
            await semaphoreSlimThrottle.WaitAsync();
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            Assert.Equal(0, semaphoreSlimThrottle.Release());

            semaphoreSlimThrottle = new SemaphoreSlimThrottle(1);
            semaphoreSlimThrottle.Dispose();
        }
    }
}