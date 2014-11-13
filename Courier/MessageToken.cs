using System;

namespace Courier
{
	/// <summary>
	/// A token returned when registering for a message. Hang on to this token if you want to explictly unsubscribe to from the message in the future
	/// </summary>
	public class MessageToken
	{
		/// <summary>
		/// The message name / key
		/// </summary>
		public String Message { get; set; }

		/// <summary>
		/// A unique ID generated for this subscription
		/// </summary>
		public Guid Id { get; set; }
		/// <summary>
		/// When this token was created
		/// </summary>
		public DateTime TimeAcquired { get; set; }

		private MessageToken(Guid id, DateTime timeAcquired, String message)
		{
			Id = id;
			TimeAcquired = timeAcquired;
			Message = message;
		}

		internal static MessageToken GenerateToken(String message)
		{
			return new MessageToken(Guid.NewGuid(), DateTime.Now,message);
		}

		#region Equals

		/// <summary>
		/// Equality comparasion for message tokens. Matches on Message.Id only
		/// </summary>
		/// <param name="obj">The MessageToken instance to compare to</param>
		/// <returns>true if equal; otherwise false</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			var token = obj as MessageToken;
			if (token == null)
				return false;

			return token.Id == Id;
		}

		/// <summary>
		/// Get the hash code for this MessageToken
		/// </summary>
		/// <returns>The hash code for this tokens Id property</returns>
		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
		#endregion

	}
}
