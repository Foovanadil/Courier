using System;

namespace Courier.ViewModels
{
	public class FooterVM : BaseViewModel
	{
		public String MessageContent { get; set; }
		public String ErrorContent { get; set; }
		private MessageToken Token { get; set; }

		public FooterVM()
		{
			Token = Mediator.RegisterForMessage("Content1Message", (Action<String>)OnContent1MessageReceived);
			//Hook up for a message with a valid name but invalid param type (int) this will show how onError works
			Mediator.RegisterForMessage<int>("Content1Message", OnMessageReceivedWrongParamType, OnError);
		}

		private void OnMessageReceivedWrongParamType(int obj)
		{
			//Do nothing (this will never get called because we registered for a message by name but gave the wrong param type)
			//This is to show how the OnError method below will get raised instead
		}

		private void OnError(Exception obj)
		{
			ErrorContent = obj.Message;
			PropertyChange("ErrorContent");
		}

		private void OnContent1MessageReceived(String messageContent)
		{
			MessageContent = messageContent;
			PropertyChange("MessageContent");

			Mediator.UnRegisterForMessage(Token);
		}
		
	}
}
