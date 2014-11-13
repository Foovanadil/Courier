using System;
using System.Linq;

namespace Courier.Rx
{
	/// <summary>
	/// A shim for hooking up to Rx. This classe handles converting traditional messages into an IObservable sequence.
	/// </summary>
	public static class MessageBroker
	{
		/// <summary>
		/// Regsiter for an IObservable sequence of messages from the mediator
		/// </summary>
		/// <typeparam name="T">The Type of payload you expect back</typeparam>
		/// <param name="messenger">The mediator</param>
		/// <param name="message">The message key to subscribe to</param>
		/// <returns>And IObservable sequence of messages for the given key / T combination</returns>
		public static IObservable<T> RegisterForMessage<T>(this Mediator messenger, String message)
		{
			//TODO: Guard aganist NULL T so the cast in the .Select won't explode in a ball or fire.
			return Observable.FromEvent<MessageBroadcastArgs>(handler => messenger.MessageBroadcast += handler,
			                                                     handler => messenger.MessageBroadcast -= handler)
																 .Where(msg => msg.EventArgs.Message == message)
																 .Select(msg => (T)msg.EventArgs.Payload);
		}
	}
}
