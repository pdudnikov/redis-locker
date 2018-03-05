using System.Threading;
using System.Threading.Tasks;

namespace RedisAsyncEvents
{
	public class SemaphoreWithKey
	{
		private readonly object _lock = new object();
		private readonly SemaphoreSlim _semaphore;
		private bool _disposed;

		public string Key { get; private set; }

		public SemaphoreWithKey(string key)
		{
			Key = key;
			_semaphore = new SemaphoreSlim(0, 1);
		}

		public Task<bool> WaitAsync(int timeout)
		{
			return _semaphore.WaitAsync(timeout);
		}

		public Task<bool> WaitAsync(int timeout, CancellationToken cancellationToken)
		{
			return _semaphore.WaitAsync(timeout, cancellationToken);
		}

		public void Release()
		{
			lock (_lock)
			{
				if (!_disposed)
				{
					_semaphore.Release();
				}
			}
		}

		public void Dispose()
		{
			lock (_lock)
			{
				if (!_disposed)
				{
					_semaphore.Dispose();
					_disposed = true;
				}
			}
		}
	}
}
