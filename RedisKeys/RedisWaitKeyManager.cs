using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisAsyncEvents
{
	public class RedisWaitKeyManager
	{
		private static readonly string Channel = typeof(WaitKeyManager).Name + "ReleaseChannel";

		private readonly ISubscriber _subscriber;
		private readonly int _timeout;
		private readonly WaitKeyManager _waitKeyManager;

		public RedisWaitKeyManager(ISubscriber subscriber, int timeout)
		{
			_subscriber = subscriber;
			_timeout = timeout;
			_waitKeyManager = new WaitKeyManager();

			subscriber.Subscribe(Channel, releaseKey);
		}

		public RedisWaitKeyManager(ISubscriber subscriber, int timeout, RedisWaitKeyManager instance)
		{
			_subscriber = subscriber;
			_timeout = timeout;
			_waitKeyManager = instance._waitKeyManager;

			subscriber.Subscribe(Channel, releaseKey);
		}

		private void releaseKey(RedisChannel channel, RedisValue value)
		{
			_waitKeyManager.ReleaseKey(value);
		}

		public Task Wait(string key)
		{
			return _waitKeyManager.WaitFor(key, _timeout);
		}

		public void Release(string key)
		{
			_waitKeyManager.ReleaseKey(key);
			_subscriber.Publish(Channel, key);
		}

		public bool IsConnected
		{
			get { return _subscriber.IsConnected(Channel); }
		}
	}

	public static class RedisWaitKeyManagerFactory
	{
		private static readonly object Lock = new object();
		private static RedisWaitKeyManager _instance;

		public static RedisWaitKeyManager Instance(ISubscriber subscriber)
		{
			lock (Lock)
			{
				if (_instance == null)
				{
					_instance = new RedisWaitKeyManager(subscriber, 20 * 1000);
				}
				if (!_instance.IsConnected)
				{
					_instance = new RedisWaitKeyManager(subscriber, 20 * 1000, _instance);
				}
				return _instance;
			}
		}
	}
}
