using System;
using System.Linq;

using Xunit;

namespace Courier.Rx.Tests
{
	public class RxTests : IDisposable
	{
		public static Mediator Messenger { get; set; }
		private static MessageToken MessageToken { get; set; }
		
		public RxTests()
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
		public void SingeMessageObservable()
		{
			Int32 callCount = 0;

			IObservable<Int32> observable = Messenger.RegisterForMessage<Int32>("TestMessage");
			observable.Subscribe(s => callCount++);

			Messenger.BroadcastMessage("TestMessage", 1);
			Messenger.BroadcastMessage("TestMessage", 2);
			Messenger.BroadcastMessage("TestMessage", 3);
			Messenger.BroadcastMessage("TestMessage", 4);

			Assert.Equal(4, callCount);
		}

		[Fact]
		public void MultipleMessageObservable()
		{
			Int32 callCount = 0;
			IObservable<Int32> observable = Messenger.RegisterForMessage<Int32>("TestMessage");

			observable.Subscribe(msg => callCount++);

			Messenger.BroadcastMessage("TestMessage", 1);
			Messenger.BroadcastMessage("TestMessage", 2);
			Messenger.BroadcastMessage("TestMessage", 3);
			Messenger.BroadcastMessage("Foo", 4);
			Messenger.BroadcastMessage("Bar", 5);
			Messenger.BroadcastMessage("Foo", 6);
			Messenger.BroadcastMessage("TestMessage", 2);
			Messenger.BroadcastMessage("Blah", 8);
			Messenger.BroadcastMessage("TestMessage", 2);
			Messenger.BroadcastMessage("Foo", 12);

			Assert.Equal(5, callCount);
		}

		[Fact]
		public void ObservableFiltering()
		{
			Int32 callCount = 0;
			IObservable<Int32> observable = from msg in Messenger.RegisterForMessage<Int32>("TestMessage")
											 where msg.Equals(2)
											 select msg;

			observable.Subscribe(msg => callCount++);

			Messenger.BroadcastMessage("TestMessage", 1);
			Messenger.BroadcastMessage("TestMessage", 2);
			Messenger.BroadcastMessage("TestMessage", 3);
			Messenger.BroadcastMessage("Foo", 4);
			Messenger.BroadcastMessage("Bar", 5);
			Messenger.BroadcastMessage("Foo", 6);
			Messenger.BroadcastMessage("TestMessage", 2);
			Messenger.BroadcastMessage("Blah", 8);
			Messenger.BroadcastMessage("TestMessage", 2);
			Messenger.BroadcastMessage("Foo", 12);

			Assert.Equal(3, callCount);
		}

	}
}
