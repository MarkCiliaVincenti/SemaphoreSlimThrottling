using System.Threading.Tasks;
using Xunit;

namespace SemaphoreSlimThrottling.Tests
{
    public class Tests
    {
        [Fact]
        public async Task TestSemaphoreSlimThrottle()
        {
            var semaphoreSlimThrottle = new SemaphoreSlimThrottle(-2, 1);
            Assert.Equal(-2, semaphoreSlimThrottle.CurrentCount);
            semaphoreSlimThrottle.Release();
            Assert.Equal(-1, semaphoreSlimThrottle.CurrentCount);
            semaphoreSlimThrottle.Release();
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            semaphoreSlimThrottle.Release();
            Assert.Equal(1, semaphoreSlimThrottle.CurrentCount);
            await semaphoreSlimThrottle.WaitAsync();
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            semaphoreSlimThrottle.Release();

            semaphoreSlimThrottle = new SemaphoreSlimThrottle(-2, 1);
            Assert.Equal(-2, semaphoreSlimThrottle.CurrentCount);
            semaphoreSlimThrottle.Release(3);
            Assert.Equal(1, semaphoreSlimThrottle.CurrentCount);
            await semaphoreSlimThrottle.WaitAsync();
            Assert.Equal(0, semaphoreSlimThrottle.CurrentCount);
            semaphoreSlimThrottle.Release();
        }
    }
}