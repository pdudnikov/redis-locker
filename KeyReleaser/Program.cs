using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using RedisAsyncEvents;
using StackExchange.Redis;

namespace KeyReleaser
{
	class Program
	{
		static void Main(string[] args)
		{
			var options = new ConfigurationOptions();
			options.EndPoints.Add("127.0.0.1", 6379);
			var keyManager = RedisWaitKeyManagerFactory.Instance(ConnectionMultiplexer.Connect(options).GetSubscriber());

			Console.WriteLine("keys to release");

			string[] key = (Console.ReadLine() ?? string.Empty).Split(',');
			while (!key.Contains("q"))
			{
				Task.WaitAll(key.Select(x => Task.Run(() =>
				{
					keyManager.Release(x);
					Console.WriteLine("key released: " + x + " " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
				})).ToArray());

				Console.WriteLine("keys to release");
				key = (Console.ReadLine() ?? string.Empty).Split(',');
			}
		}
	}
}
