using Courier;

namespace Courier.ViewModels
{
	public class BaseViewModel : BindableObject
	{
		/// <summary>
		/// Handles subscribing and broadcasting messages in a decoupled way
		/// </summary>
		public static Mediator Mediator { get; set; }

		static BaseViewModel()
		{
			Mediator = new Mediator();
		}
	}
}