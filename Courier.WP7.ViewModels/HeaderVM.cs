using System;
using Courier.ViewModels;

namespace Courier.WP7.ViewModels
{
	public class HeaderVM : BaseViewModel
	{
		public String MessageContent { get; set; }

		public HeaderVM()
		{
			Mediator.RegisterForMessage("Content1Message", (Action<String>)OnContent1MessageReceived);
		}

		//NOTE: in Silverlight, due to the sandboxed execution model, your subscription callback MUST
		//be public. The mediator is unable to dynamically invoke non public methods (it results in an 
		//MethodAccessException). The mediator can't effectivley warn aganist this currently so the callback 
		//will just get ignored and your method will not be invoked. 
		public void OnContent1MessageReceived(String messageContent)
		{
			MessageContent = messageContent;
			PropertyChange("MessageContent");
		}
	}
}
