using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Courier
{
	/// <summary>
	/// The base mediator class that handles subscriptions, message dispatching and message caching
	/// </summary>
	public class Mediator : IDisposable
	{
		#region private fields
		private readonly MessageToSubscriberMap Subscribers = new MessageToSubscriberMap();
		private readonly List<CachedMessage> CachedMessages = new List<CachedMessage>();
		#endregion

		#region private methods
		private void InternalBroadcastMessage<T>(String message, T parameter)
		{
			var subscriberList = Subscribers.GetSubscribers<T>(message) as List<MulticastDelegate>;

			if (subscriberList != null)
			{
				foreach (var subscriber in subscriberList)
				{
					if (parameter == null)
					{
						subscriber.DynamicInvoke();
					}
					else
					{
						subscriber.DynamicInvoke(parameter);
					}
				}


				SendMessageBroadcastEvent(message, parameter);
			}
		}

		private void BroadcastCachedMessages(String message, MulticastDelegate callback)
		{
			CleanOutCache();
			//Search the cache for matches messages
			List<CachedMessage> matches = CachedMessages.FindAllSL(action => action.Message == message);
			//If we find matches invoke the delegate passed in and pass the message payload
			foreach (var cachedMessage in matches)
			{
				if (callback.Method.GetParameters().Length == 0)
				{
					callback.DynamicInvoke();
				}
				else
				{
					callback.DynamicInvoke(cachedMessage.Parameter);
				}
				cachedMessage.ResendCount++;
			}
			CleanOutCache();
		}

		private MessageToken InternalRegisterForMessage(MulticastDelegate callback, MulticastDelegate onError, string message, Boolean excludeCachedMessages)
		{
			if (callback.Target == null)
				throw new InvalidOperationException("Delegate cannot be static");

			if (!excludeCachedMessages)
			{
				BroadcastCachedMessages(message, callback);
			}

			ParameterInfo[] parameters = callback.Method.GetParameters();

			if (parameters != null && parameters.Length > 1)
				throw new InvalidOperationException("The registered delegate should have 1 or fewer parameters");

			Type parameterType = (parameters == null || parameters.Length == 0) ? null : parameters[0].ParameterType;

			MessageToken token = MessageToken.GenerateToken(message);

			if (onError != null)
			{
				Subscribers.AddSubscriber(token, callback.Target, callback.Method, onError.Target, onError.Method, parameterType);
			}
			else
			{
				Subscribers.AddSubscriber(token, callback.Target, callback.Method, null, null, parameterType);
			}

			return token;
		}


		private void CleanOutCache()
		{
			//Remove any expired messages from the cache
			CachedMessages.RemoveAllSL(message => (message.CacheOptions.ExpirationDate < DateTime.Now) || (message.ResendCount >= message.CacheOptions.NumberOfResends));
		}
		#endregion

		#region internal event

		internal event EventHandler<MessageBroadcastArgs> MessageBroadcastEvent;
		internal event EventHandler<MessageBroadcastArgs> MessageBroadcast
		{
			add { MessageBroadcastEvent += value; }
			remove { MessageBroadcastEvent -= value; }
		}

		internal void SendMessageBroadcastEvent<T>(String message, T payload)
		{
			var args = new MessageBroadcastArgs(message, payload);
			if (MessageBroadcastEvent != null)
			{
				MessageBroadcastEvent(this, args);
			}
		}


		#endregion

		#region public methods

		#region register methods
		/// <summary>
		/// Register for the message by Type T. No name is given so one will be generated internally
		/// </summary>
		/// <param name="callback">The delegate to invoke when the specified message is raised</param>
		/// <returns>a MessageToken to be used to unregister for the message and to verify subscription</returns>
		public MessageToken RegisterForMessage<T>(Action<T> callback)
		{
			Type paramType = typeof(T);
			return InternalRegisterForMessage(callback, null, paramType.FullName, false);
		}

		/// <summary>
		/// Register for the message by Type T. No name is given so one will be generated internally
		/// </summary>
		/// <param name="callback">The delegate to invoke when the specified message is raised</param>
		/// <param name="onError">The delegate to invoke when an error has occured on message dispatch</param>
		/// <returns>a MessageToken to be used to unregister for the message and to verify subscription</returns>
		public MessageToken RegisterForMessage<T>(Action<T> callback, Action<Exception> onError)
		{
			Type paramType = typeof(T);
			return InternalRegisterForMessage(callback, onError, paramType.FullName, false);
		}

		/// <summary>
		/// Register for the specified message. When the specified message is broadcast the subscribers delegate
		/// will be invoked
		/// </summary>
		/// <param name="message">The message to subscribe to</param>
		/// <param name="callback">The delegate to invoke when the specified message is raised</param>
		/// <returns>a MessageToken to be used to unregister for the message and to verify subscription</returns>
		public MessageToken RegisterForMessage<T>(String message, Action<T> callback)
		{
			return InternalRegisterForMessage(callback, null, message, false);
		}

		/// <summary>
		/// Register for the specified message. When the specified message is broadcast the subscribers delegate
		/// will be invoked
		/// </summary>
		/// <param name="message">The message to subscribe to</param>
		/// <param name="callback">The delegate to invoke when the specified message is raised</param>
		/// <param name="onError">The delegate to invoke when an error has occured on message dispatch</param>
		/// <returns>a MessageToken to be used to unregister for the message and to verify subscription</returns>
		public MessageToken RegisterForMessage<T>(String message, Action<T> callback, Action<Exception> onError)
		{
			return InternalRegisterForMessage(callback, onError, message, false);
		}

		/// <summary>
		/// Register for the specified message. When the specified message is broadcast the subscribers delegate
		/// will be invoked
		/// </summary>
		/// <param name="message">The message to subscribe to</param>
		/// <param name="callback">The delegate to invoke when the specified message is raised</param>
		/// <param name="excludeCachedMessages">Opt out of receiving cached messages</param>
		/// <returns>a MessageToken to be used to unregister for the message and to verify subscription</returns>
		public MessageToken RegisterForMessage<T>(String message, Action<T> callback, Boolean excludeCachedMessages)
		{
			return InternalRegisterForMessage(callback, null, message, excludeCachedMessages);
		}

		/// <summary>
		/// Register for the specified message. When the specified message is broadcast the subscribers delegate
		/// will be invoked
		/// </summary>
		/// <param name="message">The message to subscribe to</param>
		/// <param name="callback">The delegate to invoke when the specified message is raised</param>
		/// <param name="onError">The delegate to invoke when an error has occured on message dispatch</param>
		/// <param name="excludeCachedMessages">Opt out of receiving cached messages</param>
		/// <returns>a MessageToken to be used to unregister for the message and to verify subscription</returns>
		public MessageToken RegisterForMessage<T>(String message, Action<T> callback, Action<Exception> onError, Boolean excludeCachedMessages)
		{
			return InternalRegisterForMessage(callback, onError, message, excludeCachedMessages);
		}

		/// <summary>
		/// Register for a message only by name.
		/// </summary>
		/// <param name="message">the message name</param>
		/// <param name="callback">the handler to raise when the message is broadcast</param>
		/// <returns></returns>
		public MessageToken RegisterForMessage(String message, Action callback)
		{
			return InternalRegisterForMessage(callback, null, message, false);
		}

		/// <summary>
		/// Register for a message only by name.
		/// </summary>
		/// <param name="message">the message name</param>
		/// <param name="callback">the handler to raise when the message is broadcast</param>
		/// <param name="onError">The delegate to invoke when an error has occured on message dispatch</param>
		/// <returns></returns>
		public MessageToken RegisterForMessage(String message, Action callback, Action<Exception> onError)
		{
			return InternalRegisterForMessage(callback, onError, message, false);
		}
		#endregion

		/// <summary>
		/// Remove the subscription from the mediator to prevent any future messages from being dispatched to the 
		/// specified tokens endpoint 
		/// </summary>
		/// <param name="token">The token that identifies the subscriber to remove</param>
		public void UnRegisterForMessage(MessageToken token)
		{
			if (token == null)
				throw new ArgumentNullException("token");

			Subscribers.RemoveSubscriber(token);
		}

		/// <summary>
		/// Look to see if the specified token is listed as a subscriber
		/// </summary>
		/// <param name="messageToken">The token to check</param>
		public Boolean IsSubscribed(MessageToken messageToken)
		{
			return Subscribers.IsSubscribed(messageToken);
		}

		/// <summary>
		/// Given a message will tell you if there is a message that matches in the cache
		/// </summary>
		/// <param name="message">The message to look for in the cache</param>
		/// <returns>True if there is one ion cache that matches, otherwise false</returns>
		public Boolean IsCached(String message)
		{
			//Make sure to clean out the cache before checking
			CleanOutCache();

			var item = CachedMessages.FirstOrDefault(action => action.Message == message);
			return item != default(CachedMessage);
		}

		/// <summary>
		/// Given a message token the mediator will remove the specified message from cache regardless of 
		/// what the cache settings are. Use this method when you are caching a message idefintley or if 
		/// you want preempt the normal cache expiration pipline
		/// </summary>
		/// <param name="token">The message token returned from the BroadcastMessage method call</param>
		/// <returns>True if the removal suceeded False if the removal failed (i.e. the message is still in cache)</returns>
		public Boolean RemoveFromCache(MessageToken token)
		{
			try
			{
				CachedMessages.RemoveAllSL(message => message.Token == token);

				var item = CachedMessages.FirstOrDefault(action => action.Token == token);
				return item != default(CachedMessage);
			}
			catch (ArgumentNullException)
			{
				return false;
			}
		}

		#region broadcast methods
		/// <summary>
		/// Given a parameter the mediator will broadcast this message to all subscribers that are registered
		/// </summary>
		/// <typeparam name="T">The type of the parameter (payload) being passed</typeparam>
		/// <param name="parameter">the payload of the message</param>
		public void BroadcastMessage<T>(T parameter)
		{
			Type paramType = typeof(T);
			InternalBroadcastMessage(paramType.FullName, parameter);
		}

		/// <summary>
		/// Given a message key, and a parameter the mediator will broadcast this message to all subscribers that are registered
		/// </summary>
		/// <typeparam name="T">The type of the parameter (payload) being passed</typeparam>
		/// <param name="message">The name of the message</param>
		/// <param name="parameter">the payload of the message</param>
		public void BroadcastMessage<T>(String message, T parameter)
		{
			InternalBroadcastMessage(message, parameter);
		}

		/// <summary>
		/// Given a message key, and cache settings the mediator will broadcast this message
		/// to all subscribers that are registered
		/// </summary>
		/// <param name="message">The name of the message</param>
		/// <param name="cacheOptions">The cache settings for this message</param>
		/// <returns>A message token for the cached message. This token can be used to explicitly remove the message from cache</returns>
		public MessageToken BroadcastMessage(String message, CacheSettings cacheOptions)
		{
			return BroadcastMessage<Object>(message, null, cacheOptions);
		}

		/// <summary>
		/// Given a message key, a parameter, and cache settings the mediator will broadcast this message
		/// to all subscribers that are registered
		/// </summary>
		/// <typeparam name="T">The type of the parameter (payload) being passed</typeparam>
		/// <param name="message">The name of the message</param>
		/// <param name="parameter">the payload of the message</param>
		/// <param name="cacheOptions">The cache settings for this message</param>
		/// <returns>A message token for the cached message. This token can be used to explicitly remove the message from cache</returns>
		public MessageToken BroadcastMessage<T>(String message, T parameter, CacheSettings cacheOptions)
		{
			InternalBroadcastMessage(message, parameter);
			//Cache the message for broadcast later
			var cachedMessage = new CachedMessage(message, parameter, cacheOptions);
			CachedMessages.Add(cachedMessage);
			return cachedMessage.Token;
		}

		/// <summary>
		/// Broadcast a message by name with no callback to invoke. This can be used a simple "ping" type notification where you don't care about a payload
		/// just that the message has been fired.
		/// </summary>
		/// <param name="message"></param>
		public void BroadcastMessage(String message)
		{
			InternalBroadcastMessage<Object>(message, null);
		}
		#endregion
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Dispose of the Mediator
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Explicit Mediator deconstructor
		/// </summary>
		~Mediator()
		{
			Dispose();
		}

		/// <summary>
		/// Dispose of the mediator. Flush the cach and dispose all the subscribers
		/// </summary>
		/// <param name="disposing">True is the object is in the process of being disposed via a call to the Dispose method</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (CachedMessages != null)
				{
					CachedMessages.Clear();
				}

				if (Subscribers != null)
				{
					var disposable = Subscribers as IDisposable;

					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
			}

		}
		#endregion
	}

	public static class MediatorFactory
	{
		private static readonly Mediator InternalMediator = new Mediator();

		public static Mediator GetStaticMediator()
		{
			return InternalMediator;
		}

		public static Mediator GetNewMediatorInstance()
		{
			return new Mediator();
		}
	}
}