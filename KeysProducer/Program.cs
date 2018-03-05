using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using RedisAsyncEvents;
using StackExchange.Redis;

namespace KeysProducer
{
	class Program
	{
		static void Main(string[] args)
		{
			var options = new ConfigurationOptions();
			options.EndPoints.Add("127.0.0.1", 6379);
			var keyManager = RedisWaitKeyManagerFactory.Instance(ConnectionMultiplexer.Connect(options).GetSubscriber());

			Console.WriteLine("keys to lock");
			string[] key = (Console.ReadLine() ?? string.Empty).Split(',');

			while (!key.Contains("q"))
			{
				Task.WaitAll(key.Select(x => Task.Run(() =>
				{
					Console.WriteLine("key locked: " + x + " " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
					keyManager.Wait(x).Wait();
					Console.WriteLine("key released: " + x + " " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
				})).ToArray());

				Console.WriteLine("keys to lock");
				key = (Console.ReadLine() ?? string.Empty).Split(',');
			}
		}
	}
}
