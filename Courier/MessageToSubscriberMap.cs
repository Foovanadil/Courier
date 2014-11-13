using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

namespace Courier
{
	[DebuggerDisplay("Subscriber Count: {SubscriberCount}")]
	internal class MessageToSubscriberMap : IDisposable
	{
		//Store mappings with weak references to prevent leaks
		private readonly Dictionary<MessageToken, WeakSubscriber> Map = new Dictionary<MessageToken, WeakSubscriber>();

		internal void AddSubscriber(MessageToken token, Object callbackTarget, MethodInfo callbackMethod, 
			Object onErrorTarget, MethodInfo onErrorMethod, Type subscriberType)
		{
			if (token == null)
				throw new ArgumentNullException("token");

			if (callbackMethod == null)
				throw new ArgumentNullException("callbackMethod");

			lock (Map)
			{
				if (!Map.ContainsKey(token))
				{
					Map.Add(token, new WeakSubscriber(callbackTarget, callbackMethod, onErrorTarget, onErrorMethod, subscriberType));
				}
				else
				{
					Map[token] = new WeakSubscriber(callbackTarget, callbackMethod, onErrorTarget, onErrorMethod, subscriberType);
				}
			}
		}

		internal void RemoveSubscriber(MessageToken token)
		{
			lock (Map)
			{
				if (Map.ContainsKey(token))
				{
					Map.Remove(token);
				}
			}
		}

		internal IEnumerable<MulticastDelegate> GetSubscribers<T>(String message)
		{
			if (String.IsNullOrEmpty(message))
				throw new ArgumentNullException("message");

			List<MulticastDelegate> subscribers;
			lock (Map)
			{
				List<WeakSubscriber> weakSubscribers = Map.Where(subscriber => subscriber.Key.Message == message).Select(subscriber => subscriber.Value).ToList();
				
				weakSubscribers.ForEach(delegate(WeakSubscriber sub)
											{
												if (!VerifyParameterType<T>(sub))
												{
													ThrowMismatchTypeException<T>(sub);
												}
											});
				
				subscribers = new List<MulticastDelegate>(weakSubscribers.Count());

				//Get ONLY those subscribers that have valid signatures
				var validSubscribers = weakSubscribers.Where(VerifyParameterType<T>);

				foreach (var weakSubscriber in validSubscribers)
				{
					MulticastDelegate weakSub = weakSubscriber.CreateCallbackDelegate();
					if (weakSub != null)
					{
						subscribers.Add(weakSub);
					}
				}
			}
			return subscribers;
		}

		private static Type GetMethodArgumentType(WeakSubscriber subscriber)
		{
			var args = subscriber.CallbackMethod.GetGenericArguments() as IEnumerable<Type>;

			Type argType = typeof(Object);
			if (subscriber.CallbackMethod.GetParameters().Count() != 0)
			{
				argType = !args.Any() ? subscriber.CallbackMethod.GetParameters()[0].ParameterType : args.ElementAt(0).GetType();
			}

			return argType;
		}

		private static Boolean VerifyParameterType<T>(WeakSubscriber subscriber)
		{
			//Make sure that the callbackTarget delegate has the same type of T as the boadcaster is sending
			Type argType = GetMethodArgumentType(subscriber);
			return argType == typeof(T);
		}

		private static void ThrowMismatchTypeException<T>(WeakSubscriber subscriber)
		{
			String subscriberName = subscriber.CallbackMethod.Name;
			String expectedTypeName = GetMethodArgumentType(subscriber).FullName;

			//TODO: Find a good way to get the ultimate caller of this callbackMethod (the object that casued the broadcast)
			//This gets a little tricky to find the ultimate caller. We can build a StackFrame and roll up the tree to find the caller
			//but that is performance intensive and tricky to accuratley find the invoker.
			String broadcastedType = typeof(T).FullName;

			String errorMessage = String.Format("The subscriber: {0} expected a parameter of type: {1} but the Broadcaster sent a parameter of type: {2}", subscriberName, expectedTypeName, broadcastedType);
			Exception ex = new ArgumentException(errorMessage);

			if (subscriber.OnErrorMethod != null)
			{
				Delegate onError = subscriber.CreateOnErrorDelegate();
				if (onError != null)
				{
					onError.DynamicInvoke(ex);
				}
			}
		}

		internal Boolean IsSubscribed(MessageToken messageToken)
		{
			return Map.ContainsKey(messageToken);
		}

		#region IDisposable Members
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~MessageToSubscriberMap()
		{
			Dispose();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Map != null)
				{
					Map.Clear();
				}
			}
		}
		#endregion

		#region Debugger Support
		
		private Int32 SubscriberCount
		{
			get
			{
				return Map.Count();
			}
		}
		
		#endregion
	}
}