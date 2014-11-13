using System;
using System.Diagnostics;
using Xunit;

namespace Courier.Tests
{
	public class PerformanceTests
	{
		public static Mediator Messenger { get; set; }
		private static MessageToken MessageToken { get; set; }
		private const String MESSAGE_PAYLOAD = "Foo";
		private Int32 messageDispatchCount;
		public PerformanceTests()
		{
			 Messenger = new Mediator();
		}
	
		[Fact]
		public void TenThosandMessageDispatchesOneSubscriber()
		{
			messageDispatchCount = 0;
			//Register an endpoint for this message before sending it 
			MessageToken = Messenger.RegisterForMessage("TestMessage", new Action<String>(MessageHandler));
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			for (int i = 0; i < 10000; i++)
			{
				Messenger.BroadcastMessage("TestMessage", MESSAGE_PAYLOAD);
			}
			stopwatch.Stop();
			Assert.Equal(10000, messageDispatchCount);
			Console.WriteLine("Dispatched {0} messages to one subscriber in {1} miliseconds",messageDispatchCount,stopwatch.ElapsedMilliseconds);
		}

		[Fact]
		public void TenThousandSubscibersOneDispatch()
		{
			messageDispatchCount = 0;
			//Register an endpoint for this message before sending it 
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			for (int i = 0; i < 10000; i++)
			{
				Messenger.RegisterForMessage("TestMessage", new Action<String>(MessageHandler));
			}
			stopwatch.Stop();
			Console.WriteLine("Registered 10000 subscribers in {0} miliseconds",stopwatch.ElapsedMilliseconds);

			stopwatch.Restart();
			Messenger.BroadcastMessage("TestMessage", MESSAGE_PAYLOAD);
			stopwatch.Stop();

			Assert.Equal(10000, messageDispatchCount);
			Console.WriteLine("Dispatched {0} messages (1 per subscriber) to 10000 subscribers in {1} miliseconds", messageDispatchCount, stopwatch.ElapsedMilliseconds);
		}

		[Fact]
		public void FiveHunderedThousandDispatches()
		{
			messageDispatchCount = 0;
			//Register an endpoint for this message before sending it 
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			for (int i = 0; i < 100; i++)
			{
				Messenger.RegisterForMessage("TestMessage", new Action<String>(MessageHandler));
			}
			stopwatch.Stop();
			Console.WriteLine("Registered 100 subscribers in {0} miliseconds", stopwatch.ElapsedMilliseconds);

			stopwatch.Restart();
			for (int i = 0; i < 5000; i++)
			{
				Messenger.BroadcastMessage("TestMessage", MESSAGE_PAYLOAD);
			}
			stopwatch.Stop();

			Assert.Equal(500000, messageDispatchCount);

			double totalSeconds = (stopwatch.ElapsedMilliseconds/1000);
			double avgMessagesPerSecond = messageDispatchCount / totalSeconds;

			Console.WriteLine("Dispatched {0} messages (1 per subscriber) to 100 subscribers in {1} miliseconds", messageDispatchCount, stopwatch.ElapsedMilliseconds);

			Console.WriteLine("Messages/ Per second : {0}", avgMessagesPerSecond);
	
		}

		[Fact]
		public void FiveMillionDispatches()
		{
			messageDispatchCount = 0;
			//Register an endpoint for this message before sending it 
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			for (int i = 0; i < 1000; i++)
			{
				Messenger.RegisterForMessage("TestMessage", new Action<String>(MessageHandler));
			}
			stopwatch.Stop();
			Console.WriteLine("Registered 1000 subscribers in {0} miliseconds", stopwatch.ElapsedMilliseconds);

			stopwatch.Restart();
			for (int i = 0; i < 5000; i++)
			{
				Messenger.BroadcastMessage("TestMessage", MESSAGE_PAYLOAD);
			}
			stopwatch.Stop();

			Assert.Equal(5000000, messageDispatchCount);
			double totalSeconds = (stopwatch.ElapsedMilliseconds / 1000);
			double avgMessagesPerSecond = messageDispatchCount / totalSeconds;
			
			Console.WriteLine("Dispatched {0} messages (1 per subscriber) to 1000 subscribers in {1} miliseconds", messageDispatchCount, stopwatch.ElapsedMilliseconds);

			Console.WriteLine("Messages/ Per second : {0}", avgMessagesPerSecond);
		}

		public void MessageHandler(String param)
		{
			Assert.NotNull(param);
			Assert.Equal(MESSAGE_PAYLOAD, param);
			messageDispatchCount++;
		}
	}
}
