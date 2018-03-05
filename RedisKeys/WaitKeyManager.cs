using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RedisAsyncEvents
{
	public class WaitKeyManager
	{
		private readonly ConcurrentBag<SemaphoreWithKey> _events = new ConcurrentBag<SemaphoreWithKey>();

		public Task<bool> WaitFor(string key, int timeout)
		{
			return waitFor(key, timeout, null);
		}

		public Task<bool> WaitFor(string key, int timeout, CancellationToken cancellationToken)
		{
			return waitFor(key, timeout, cancellationToken);
		}

		private async Task<bool> waitFor(string key, int timeout, CancellationToken? cancellationToken)
		{
			var semaphoreWithKey = new SemaphoreWithKey(key);
			_events.Add(semaphoreWithKey);

			semaphoreWithKey.Release();

			Task<bool> task = cancellationToken != null ? semaphoreWithKey.WaitAsync(timeout, cancellationToken.Value) : semaphoreWithKey.WaitAsync(timeout);
			bool result = await task.ConfigureAwait(false);

			if (_events.TryTake(out semaphoreWithKey))
			{
				semaphoreWithKey.Dispose();
			}

			return result;
		}

		public void ReleaseKey(string key)
		{
			SemaphoreWithKey[] array = _events.Where(x => x.Key.Equals(key)).ToArray();
			foreach (SemaphoreWithKey autoResetEvent in array)
			{
				autoResetEvent.Release();
			}
		}
	}
}
