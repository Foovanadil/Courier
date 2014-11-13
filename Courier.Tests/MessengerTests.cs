using System;
using System.Threading;

using Xunit;

namespace Courier.Tests
{
	public class MessengerTests : IDisposable
	{

		public static Mediator Messenger { get; set; }
		private static MessageToken MessageToken { get;  set; }
		private const String MESSAGE_PAYLOAD = "Foo";

		public MessengerTests()
		{
			Messenger = new Mediator();
		}
		
		public void Dispose()
		{
			if (Messenger != null)
			{
				var disposable = Messenger as IDisposable;
				disposable.Dispose();
				
			}
			MessageToken = null;
		}
		
		[Fact]
		public void BroadcastMessage()
		{
			//Register an endpoint for this message before sending it 
			MessageToken = Messenger.RegisterForMessage("TestMessage", new Action<String>(MessageHandler));
			Messenger.BroadcastMessage("TestMessage", MESSAGE_PAYLOAD);
		}

		[Fact]
		public void BroadcastMessageEmptyMessageName()
		{
			Int32 callCount = 0;
			//Register an endpoint for this message before sending it 
			MessageToken = Messenger.RegisterForMessage<String>(String.Empty, msg => callCount++ );
			Assert.Throws<ArgumentNullException>(() => Messenger.BroadcastMessage(String.Empty, MESSAGE_PAYLOAD));
		}

		[Fact]
		public void RegisterForMessageByType()
		{
			Int32 callCount = 0;
			//Register an endpoint for this message before sending it 
			MessageToken = Messenger.RegisterForMessage<String>(msg => callCount++);
			Messenger.BroadcastMessage<String>(MESSAGE_PAYLOAD);
			Assert.Equal(1, callCount);
		}

		[Fact]
		public void RegisterForMessageByTypeWithMultipleSubscribers()
		{
			Int32 callCount = 0;
			//Register mutiple callbacks for this message before sending it 
			Messenger.RegisterForMessage<String>(msg => callCount++);
			Messenger.RegisterForMessage<String>(msg => callCount++);
			Messenger.RegisterForMessage<String>(msg => callCount++);
			Messenger.RegisterForMessage<String>(msg => callCount++);

			Messenger.BroadcastMessage<String>(MESSAGE_PAYLOAD);
			Assert.Equal(4, callCount);
		}

		[Fact]
		public void RegisterForMessageByTypeWithMismatch()
		{
			Int32 callCount = 0;
			//Register an endpoint for this message before sending it 
			MessageToken = Messenger.RegisterForMessage<String>(msg => callCount++);
			
			//Make sure the different types don't collide
			Messenger.BroadcastMessage<String>(MESSAGE_PAYLOAD);
			Messenger.BroadcastMessage<Int32>(15);
			Messenger.BroadcastMessage<Double>(0.0);
			Messenger.BroadcastMessage<Boolean>(false);

			Assert.Equal(1, callCount);
		}

		[Fact]
		public void RegisterForMessageNoPayload()
		{
			Int32 callCount = 0;
			Messenger.RegisterForMessage("Foo", () => callCount++);
			Messenger.BroadcastMessage("Foo");
			Assert.Equal(1, callCount);
		}

		[Fact]
		public void MultiSubscriberRegisterForMessageNoPayload()
		{
			Int32 callCount = 0;
			Messenger.RegisterForMessage("Foo", () => callCount++);
			Messenger.RegisterForMessage("Foo", () => callCount++);
			Messenger.RegisterForMessage("Foo", () => callCount++);
			Messenger.RegisterForMessage("Foo", () => callCount++);
			Messenger.RegisterForMessage("Foo", () => callCount++);

			Messenger.BroadcastMessage("Foo");
			Assert.Equal(5, callCount);
		}

		[Fact]
		public void MultiSubscriberMultiMessageNoPayload()
		{
			Int32 callCountFoo = 0;
			Int32 callCountBar = 0;
			Int32 callCountBaz = 0;

			Messenger.RegisterForMessage("Foo", () => callCountFoo++);
			Messenger.RegisterForMessage("Foo", () => callCountFoo++);
			Messenger.RegisterForMessage("Bar", () => callCountBar++);
			Messenger.RegisterForMessage("Bar", () => callCountBar++);
			Messenger.RegisterForMessage("Baz", () => callCountBaz++);

			Messenger.BroadcastMessage("Foo");
			Messenger.BroadcastMessage("Bar");
			Messenger.BroadcastMessage("Baz");

			Assert.Equal(2, callCountFoo);
			Assert.Equal(2, callCountBar);
			Assert.Equal(1, callCountBaz);
		}

		[Fact]
		public void RegisterForMessageNoPayloadMisMatch()
		{
			Int32 callCount = 0;

			Messenger.RegisterForMessage("Foo", () => callCount++);
			Messenger.BroadcastMessage("Bar");

			Assert.Equal(0, callCount);
		}

		[Fact]
		public void ResendCachedMessage()
		{
			//Broadcast a message and mark it for caching
			var cacheSettings = new CacheSettings(1);
			Messenger.BroadcastMessage("TestMessage", MESSAGE_PAYLOAD, cacheSettings);

			Assert.True(Messenger.IsCached("TestMessage"));
			//Register an endpoint for this message after sending it to force a resend so we can test the resend caching functionality
			Messenger.RegisterForMessage("TestMessage", new Action<String>(MessageHandler));
			Assert.False(Messenger.IsCached("TestMessage"));
		}

		[Fact]
		public void IndefinitleyCacheMessage()
		{
			//Broadcast a message and mark it for caching
			var cacheSettings = new CacheSettings();
			Messenger.BroadcastMessage("TestMessage", MESSAGE_PAYLOAD, cacheSettings);
			Assert.True(Messenger.IsCached("TestMessage"));

			Int32 callCount = 0;
			//Register an endpoint for this message after sending it to force a resend so we can test the resend caching functionality
			Messenger.RegisterForMessage("TestMessage", () => callCount++);
			Assert.True(Messenger.IsCached("TestMessage"));
			Messenger.RegisterForMessage("TestMessage", () => callCount++);
			Assert.True(Messenger.IsCached("TestMessage"));
			Messenger.RegisterForMessage("TestMessage", () => callCount++);
			Assert.True(Messenger.IsCached("TestMessage"));

			Assert.Equal(3, callCount);
		}

		[Fact]
		public void IndefinitleyCacheMessageExplicitRemove()
		{
			//Broadcast a message and mark it for caching
			var cacheSettings = new CacheSettings();
			MessageToken token = Messenger.BroadcastMessage("TestMessage", MESSAGE_PAYLOAD, cacheSettings);
			Assert.True(Messenger.IsCached("TestMessage"));

			Int32 callCount = 0;
			
			//Register an endpoint for this message after sending it to force a resend so we can test the resend caching functionality
			Messenger.RegisterForMessage("TestMessage", () => callCount++);
			Assert.True(Messenger.IsCached("TestMessage"));
			Messenger.RegisterForMessage("TestMessage", () => callCount++);
			Assert.True(Messenger.IsCached("TestMessage"));
			
			//Pull the message out of cache
			Messenger.RemoveFromCache(token);
			Assert.False(Messenger.IsCached("TestMessage"));

			Assert.Equal(2, callCount);
		}


		[Fact]
		public void ResendCacheMessagePremptiveRemove()
		{
			//Broadcast a message and mark it for caching
			var cacheSettings = new CacheSettings(3);

			MessageToken token = Messenger.BroadcastMessage("TestMessage", MESSAGE_PAYLOAD, cacheSettings);
			Assert.True(Messenger.IsCached("TestMessage"));

			Int32 callCount = 0;
			Messenger.RegisterForMessage("TestMessage", () => callCount++);
			Assert.True(Messenger.IsCached("TestMessage"));

			Messenger.RegisterForMessage("TestMessage", () => callCount++);
			Assert.True(Messenger.IsCached("TestMessage"));

			//Pull the message out of cache
			Messenger.RemoveFromCache(token);
			Assert.False(Messenger.IsCached("TestMessage"));
			//This should not cause a cached resend
			Messenger.RegisterForMessage("TestMessage", () => callCount++);

			Assert.Equal(2, callCount);
		}


		[Fact]
		public void OptOutOfCachedMessage()
		{
			//Broadcast a message and mark it for caching
			var cacheSettings = new CacheSettings(1);
			Messenger.BroadcastMessage("TestMessage", MESSAGE_PAYLOAD, cacheSettings);

			Assert.True(Messenger.IsCached("TestMessage"));
			Int32 callCount = 0;
			//Register an endpoint for this message after sending it to force a resend so we can test the resend caching functionality
			Messenger.RegisterForMessage<String>("TestMessage", msg => callCount++ ,true);
			Assert.Equal(0, callCount);
			Assert.True(Messenger.IsCached("TestMessage"));
		}

		[Fact]
		public void UnregisterForMessage()
		{
			MessageToken = Messenger.RegisterForMessage("TestMessage", new Action<String>(MessageHandler));
			Assert.True(Messenger.IsSubscribed(MessageToken));

			Messenger.UnRegisterForMessage(MessageToken);
			Assert.False(Messenger.IsSubscribed(MessageToken));
		}

		[Fact]
		public void TimeBasedCacheMessage()
		{
			//Broadcast a message and mark it for caching
			//Cache the message for 2 seconds
			var cacheSettings = new CacheSettings(DateTime.Now.AddSeconds(2));
			Messenger.BroadcastMessage("TestMessage", MESSAGE_PAYLOAD, cacheSettings);

			Assert.True(Messenger.IsCached("TestMessage"));
			Thread.Sleep(TimeSpan.FromSeconds(3));
			Assert.False(Messenger.IsCached("TestMessage"));
		}


		[Fact]
		public void MultipleSubscribersOneMessage()
		{
			Int32 callCount = 0;
			
			//Add multiple subscribers to one message
			Messenger.RegisterForMessage("MultiSubscriber",new Action<String>(s => callCount++));
			Messenger.RegisterForMessage("MultiSubscriber", new Action<String>(s => callCount++));

			Messenger.BroadcastMessage("MultiSubscriber", MESSAGE_PAYLOAD);
			
			Assert.Equal(2, callCount);
		}

		[Fact]
		public void OneSubscriberMultipleMessages()
		{
			Int32 callCount = 0;

			var callback = new Action<String>(s => callCount++);

			//Add multiple subscribers to one message
			Messenger.RegisterForMessage("MultiMessage1", callback);
			Messenger.RegisterForMessage("MultiMessage2", callback);
			Messenger.RegisterForMessage("MultiMessage3", callback);

			Messenger.BroadcastMessage("MultiMessage1", MESSAGE_PAYLOAD);
			Messenger.BroadcastMessage("MultiMessage2", MESSAGE_PAYLOAD);
			Messenger.BroadcastMessage("MultiMessage3", MESSAGE_PAYLOAD);

			Assert.Equal(3, callCount);
		}

		
		private int exceptionHandlerCallCount;
		[Fact]
		public void MismatchedTypesBetweenBroadcastAndSubscriber()
		{
			Int32 callCount = 0;
			exceptionHandlerCallCount = 0;
			var callback = new Action<String>(s => callCount++);
			//Expecting a message with a payload of type String
			Messenger.RegisterForMessage("MismatchMessage", callback, ExceptionHandler);
			//broadcasting a message with a payload of type Int32
			//This should throw an argument exception
			Messenger.BroadcastMessage("MismatchMessage", 15);
			Assert.Equal(1, exceptionHandlerCallCount);
		}
		
        
		public void ExceptionHandler(Exception ex)
		{
			Assert.IsAssignableFrom<ArgumentException>(ex);
			Console.WriteLine(ex.Message);
			exceptionHandlerCallCount++;
		}
		public void MessageHandler(String param)
		{
			Assert.NotNull(param);
			Assert.Equal(MESSAGE_PAYLOAD, param);
		}

	}
}
