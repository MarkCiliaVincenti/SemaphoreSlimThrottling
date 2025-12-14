using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SemaphoreSlimThrottling;

/// <summary>
/// Limits the number of threads that can access a resource or pool of resources concurrently,
/// whilst allowing negative initialCount initialization.
/// </summary>
[ComVisible(false)]
[DebuggerDisplay("Current Count = {CurrentCount}")]
public class SemaphoreSlimThrottle : IDisposable
{
    internal volatile int _throttleCount;
    private readonly Lock _lock = LockFactory.Create();
    private bool _throttleEnabled;
    private readonly SemaphoreSlim _semaphoreSlim;

    /// <summary>
    /// Initializes a new instance of <see cref="SemaphoreSlimThrottle"/>, specifying the initial number of requests that can be granted concurrently.
    /// </summary>
    /// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently.</param>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public SemaphoreSlimThrottle(int initialCount)
    {
        _semaphoreSlim = new(initialCount);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SemaphoreSlimThrottle"/>, specifying the initial number of requests that can be granted concurrently.
    /// </summary>
    /// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently. Accepts negative numbers unlike <see cref="SemaphoreSlim"/>.</param>
    /// <param name="maxCount">The maximum number of requests for the semaphore that can be granted concurrently.</param>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SemaphoreSlimThrottle(int initialCount, int maxCount)
    {
        if (initialCount < 0)
        {
            _throttleCount = initialCount;
            _throttleEnabled = true;
            _semaphoreSlim = new SemaphoreSlim(0, maxCount);
        }
        else
        {
            _semaphoreSlim = new SemaphoreSlim(initialCount, maxCount);
        }
    }

    /// <summary>
    /// Gets the number of remaining threads that can enter the <see cref="SemaphoreSlimThrottle"/> object.
    /// </summary>
    /// <returns>
    /// The number of remaining threads that can enter the semaphore.
    /// </returns>
    public int CurrentCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (!_throttleEnabled) ? _semaphoreSlim.CurrentCount : _throttleCount + _semaphoreSlim.CurrentCount;
    }

    /// <summary>
    /// Releases the <see cref="SemaphoreSlimThrottle"/> object once.
    /// </summary>
    /// <returns>The previous count of the <see cref="SemaphoreSlimThrottle"/>.</returns>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="SemaphoreFullException"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Release() => Release(1);

    /// <summary>
    /// Releases the <see cref="SemaphoreSlimThrottle"/> object a specified number of times.
    /// </summary>
    /// <returns>The previous count of the <see cref="SemaphoreSlimThrottle"/>.</returns>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <exception cref="SemaphoreFullException"/>
    public int Release(int releaseCount)
    {
        // using bool property to avoid unnecessary volatile accesses in happy path
        if (!_throttleEnabled || releaseCount < 1)
        {
            return _semaphoreSlim.Release(releaseCount);
        }

        int remainingCount;
        int returnCount = 0;
        _lock.Enter();
        var throttleCount = _throttleCount;
        if (throttleCount == 0 || !_throttleEnabled) // Different thread released them all
        {
            remainingCount = releaseCount;
        }
        else if (releaseCount + throttleCount < 0) // Releasing less than throttle; just decrease
        {
            _throttleCount += releaseCount;
            _lock.Exit();
            return throttleCount;
        }
        else // releasing all the throttles
        {
            _throttleCount = 0;
            _throttleEnabled = false;
            returnCount = throttleCount;
            remainingCount = releaseCount + throttleCount;
        }
        _lock.Exit();

        // doing outside lock
        if (remainingCount > 0) // call into base if more locks to be released
        {
            return _semaphoreSlim.Release(remainingCount) + returnCount;
        }

        return returnCount + _semaphoreSlim.CurrentCount;
    }

    /// <summary>
    /// Releases all resources used by the current instance of the <see cref="SemaphoreSlimThrottle"/> class.
    /// </summary>
    public void Dispose()
    {
        _semaphoreSlim.Dispose();
        GC.SuppressFinalize(this);
    }

    #region direct
    /// <summary>
    /// <inheritdoc cref="SemaphoreSlim.AvailableWaitHandle"/>
    /// </summary>
    /// <returns>
    /// <inheritdoc cref="SemaphoreSlim.AvailableWaitHandle"/>
    /// </returns>
    /// <exception cref="ObjectDisposedException"/>
    public WaitHandle AvailableWaitHandle
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _semaphoreSlim.AvailableWaitHandle;
    }

    /// <summary>
    /// <inheritdoc cref="SemaphoreSlim.Wait()"/>
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Wait() => _semaphoreSlim.Wait();

    /// <summary>
    /// <inheritdoc cref="SemaphoreSlim.Wait(int)"/>
    /// </summary>
    /// <param name="millisecondsTimeout"><inheritdoc cref="SemaphoreSlim.Wait(int)"/></param>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <returns><inheritdoc cref="SemaphoreSlim.Wait(int)"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Wait(int millisecondsTimeout) => _semaphoreSlim.Wait(millisecondsTimeout);

    /// <summary>
    /// <inheritdoc cref="SemaphoreSlim.Wait(int, CancellationToken)"/>
    /// </summary>
    /// <param name="millisecondsTimeout"><inheritdoc cref="SemaphoreSlim.Wait(int, CancellationToken)" path="/param[@name='millisecondsTimeout']"/></param>
    /// <param name="cancellationToken"><inheritdoc cref="SemaphoreSlim.Wait(int, CancellationToken)" path="/param[@name='cancellationToken']"/></param>
    /// <exception cref="OperationCanceledException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <exception cref="ObjectDisposedException"/>
    /// <returns><inheritdoc cref="SemaphoreSlim.Wait(int, CancellationToken)"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken) => _semaphoreSlim.Wait(millisecondsTimeout, cancellationToken);

    /// <summary>
    /// <inheritdoc cref="SemaphoreSlim.Wait(TimeSpan)"/>
    /// </summary>
    /// <param name="timeout"><inheritdoc cref="SemaphoreSlim.Wait(TimeSpan)"/></param>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <exception cref="ObjectDisposedException"/>
    /// <returns><inheritdoc cref="SemaphoreSlim.Wait(TimeSpan)"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Wait(TimeSpan timeout) => _semaphoreSlim.Wait(timeout);

    /// <summary>
    /// <inheritdoc cref="SemaphoreSlim.Wait(TimeSpan, CancellationToken)"/>
    /// </summary>
    /// <param name="timeout"><inheritdoc cref="SemaphoreSlim.Wait(TimeSpan, CancellationToken)" path="/param[@name='timeout']"/></param>
    /// <param name="cancellationToken"><inheritdoc cref="SemaphoreSlim.Wait(int, CancellationToken)" path="/param[@name='cancellationToken']"/></param>
    /// <exception cref="OperationCanceledException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <exception cref="ObjectDisposedException"/>
    /// <returns><inheritdoc cref="SemaphoreSlim.Wait(TimeSpan, CancellationToken)"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Wait(TimeSpan timeout, CancellationToken cancellationToken) => _semaphoreSlim.Wait(timeout, cancellationToken);

    /// <summary>
    /// <inheritdoc cref="SemaphoreSlim.Wait(CancellationToken)"/>
    /// </summary>
    /// <param name="cancellationToken"><inheritdoc cref="SemaphoreSlim.Wait(CancellationToken)"/></param>
    /// <exception cref="OperationCanceledException"/>
    /// <exception cref="ObjectDisposedException"/>
    /// <returns><inheritdoc cref="SemaphoreSlim.Wait(CancellationToken)"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Wait(CancellationToken cancellationToken) => _semaphoreSlim.Wait(cancellationToken);

    /// <summary>
    /// <inheritdoc cref="SemaphoreSlim.WaitAsync()"/>
    /// </summary>
    /// <returns><inheritdoc cref="SemaphoreSlim.WaitAsync()"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WaitAsync() => _semaphoreSlim.WaitAsync();

    /// <summary>
    /// <inheritdoc cref="SemaphoreSlim.WaitAsync(int)"/>
    /// </summary>
    /// <param name="millisecondsTimeout"><inheritdoc cref="SemaphoreSlim.WaitAsync(int)"/></param>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <returns><inheritdoc cref="SemaphoreSlim.WaitAsync(int)"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<bool> WaitAsync(int millisecondsTimeout) => _semaphoreSlim.WaitAsync(millisecondsTimeout);

    /// <summary>
    /// <inheritdoc cref="SemaphoreSlim.WaitAsync(int, CancellationToken)"/>
    /// </summary>
    /// <param name="millisecondsTimeout"><inheritdoc cref="SemaphoreSlim.WaitAsync(int, CancellationToken)" path="/param[@name='millisecondsTimeout']"/></param>
    /// <param name="cancellationToken"><inheritdoc cref="SemaphoreSlim.WaitAsync(int, CancellationToken)" path="/param[@name='cancellationToken']"/></param>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="OperationCanceledException"/>
    /// <returns><inheritdoc cref="SemaphoreSlim.WaitAsync(int, CancellationToken)"/></returns>        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken) => _semaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken);

    /// <summary>
    /// <inheritdoc cref="SemaphoreSlim.Wait(CancellationToken)"/>
    /// </summary>
    /// <param name="cancellationToken"><inheritdoc cref="SemaphoreSlim.Wait(CancellationToken)"/></param>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="OperationCanceledException"/>
    /// <returns><inheritdoc cref="SemaphoreSlim.Wait(CancellationToken)"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task WaitAsync(CancellationToken cancellationToken) => _semaphoreSlim.WaitAsync(cancellationToken);

    /// <summary>
    /// <inheritdoc cref="SemaphoreSlim.WaitAsync(TimeSpan)"/>
    /// </summary>
    /// <param name="timeout"><inheritdoc cref="SemaphoreSlim.WaitAsync(TimeSpan)"/></param>
    /// <exception cref="ObjectDisposedException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <returns><inheritdoc cref="SemaphoreSlim.WaitAsync(TimeSpan)"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<bool> WaitAsync(TimeSpan timeout) => _semaphoreSlim.WaitAsync(timeout);

    /// <summary>
    /// <inheritdoc cref="SemaphoreSlim.WaitAsync(TimeSpan, CancellationToken)"/>
    /// </summary>
    /// <param name="timeout"><inheritdoc cref="SemaphoreSlim.WaitAsync(TimeSpan, CancellationToken)" path="/param[@name='timeout']"/></param>
    /// <param name="cancellationToken"><inheritdoc cref="SemaphoreSlim.WaitAsync(int, CancellationToken)" path="/param[@name='cancellationToken']"/></param>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <exception cref="OperationCanceledException"/>
    /// <returns><inheritdoc cref="SemaphoreSlim.WaitAsync(TimeSpan, CancellationToken)"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken) => _semaphoreSlim.WaitAsync(timeout, cancellationToken);
    #endregion direct
}