using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace SemaphoreSlimThrottling
{
    /// <summary>
    /// Limits the number of threads that can access a resource or pool of resources concurrently,
    /// whilst allowing negative initialCount initialization.
    /// </summary>
    [ComVisible(false)]
    [DebuggerDisplay("Current Count = {CurrentCount}")]
    public class SemaphoreSlimThrottle : SemaphoreSlim
    {
        private volatile int _throttleCount;
        private readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="SemaphoreSlimThrottle"/> class, specifying the initial number of requests that can be granted concurrently.
        /// </summary>
        /// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public SemaphoreSlimThrottle(int initialCount)
            : base(initialCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SemaphoreSlimThrottle"/> class, specifying the initial number of requests that can be granted concurrently.
        /// </summary>
        /// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently. Accepts negative numbers unlike <see cref="SemaphoreSlim"/>.</param>
        /// <param name="maxCount">The maximum number of requests for the semaphore that can be granted concurrently.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public SemaphoreSlimThrottle(int initialCount, int maxCount)
            : base(Math.Max(0, initialCount), maxCount)
        {
            _throttleCount = Math.Min(0, initialCount);
        }

        /// <summary>
        /// Gets the number of remaining threads that can enter the <see cref="SemaphoreSlimThrottle"/> object.
        /// </summary>
        /// <returns>
        /// The number of remaining threads that can enter the semaphore.
        /// </returns>
        public new int CurrentCount => _throttleCount + base.CurrentCount;

        /// <summary>
        /// Releases the <see cref="SemaphoreSlimThrottle"/> object once.
        /// </summary>
        /// <returns>The previous count of the <see cref="SemaphoreSlimThrottle"/>.</returns>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="SemaphoreFullException"/>
        public new int Release()
        {
            if (_throttleCount < 0)
            {
                lock (_lock)
                {
                    if (_throttleCount < 0)
                    {
                        _throttleCount++;
                        return _throttleCount - 1;
                    }
                }
            }
            return base.Release();
        }

        /// <summary>
        /// Releases the <see cref="SemaphoreSlimThrottle"/> object a specified number of times.
        /// </summary>
        /// <returns>The previous count of the <see cref="SemaphoreSlimThrottle"/>.</returns>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="SemaphoreFullException"/>
        public new int Release(int releaseCount)
        {
            if (releaseCount < 1)
            {
                base.Release(releaseCount); // throws exception
            }

            if (releaseCount + _throttleCount <= 0)
            {
                lock (_lock)
                {
                    if (releaseCount + _throttleCount <= 0)
                    {
                        _throttleCount += releaseCount;
                        return _throttleCount - releaseCount;
                    }
                }
            }

            if (_throttleCount < 0)
            {
                lock (_lock)
                {
                    if (_throttleCount < 0)
                    {
                        int output = CurrentCount;
                        base.Release(releaseCount + _throttleCount);
                        _throttleCount = 0;
                        return output;
                    }
                }
            }

            return base.Release(releaseCount);
        }
    }
}