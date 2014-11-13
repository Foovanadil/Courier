using System;
using System.Diagnostics;

namespace Courier
{
	[DebuggerDisplay("Message: {Message}, Parameter: {Parameter}")]
	internal class CachedMessage : IDisposable
	{
		public MessageToken Token { get; private set; }
		public String Message { get; private set; }
		public Object Parameter { get; private set; }
		public CacheSettings CacheOptions { get; private set; }
		public Int32 ResendCount { get; set; }

		public CachedMessage(String message, Object parameter, CacheSettings cacheOptions)
		{
			CacheOptions = cacheOptions;
			Message = message;
			Parameter = parameter;
			Token = MessageToken.GenerateToken(message);
		}

		#region IDisposable Members
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~CachedMessage()
		{
			Dispose();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				var disposable = CacheOptions as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}

				var disposableParam = Parameter as IDisposable;
				if (disposableParam != null)
				{
					disposableParam.Dispose();
				}

			}

		}
		#endregion
	}

	/// <summary>
	/// Class for managing cache settings for a specific message
	/// </summary>
	[DebuggerDisplay("Expiration Date: {ExpirationDate}, NumberOfResends: {NumberOfResends}")]
	public class CacheSettings
	{
		/// <summary>
		/// A timestamp of when this message should be expunged from cache
		/// </summary>
		public DateTime ExpirationDate { get; private set; }
		/// <summary>
		/// The number of times this message should be broadcast from cache before being expunged
		/// </summary>
		public Int32 NumberOfResends { get; private set; }

		/// <summary>
		/// Create an instance of the cache settings class specifying the expiration date
		/// </summary>
		/// <param name="expiration">A timestamp of when this message should be expunged from cache</param>
		public CacheSettings(DateTime expiration)
		{
			ExpirationDate = expiration;
			NumberOfResends = Int32.MaxValue;
		}

		/// <summary>
		/// Create and instance of the cache settings class specifying the number of times to rebroadcast this message before it is expunged from the cache
		/// </summary>
		/// <param name="timesToResend">The number of times this message should be broadcast from cache before being expunged</param>
		public CacheSettings(Int32 timesToResend)
		{
			NumberOfResends = timesToResend;
			ExpirationDate = DateTime.MaxValue;
		}	

		/// <summary>
		/// ONLY use this overload if you want a message to stay in cache indefinitely. If you do this
		/// you must make sure to explicitly remove the message from cache
		/// </summary>
		public CacheSettings()
		{
			ExpirationDate = DateTime.MaxValue;
			NumberOfResends = Int32.MaxValue;
		}
	}
}