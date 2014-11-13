using System;
using Courier.ViewModels;

namespace Courier.SL.ViewModels
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

		public void OnMessageReceivedWrongParamType(int obj)
		{
			//Do nothing (this will never get called because we registered for a message by name but gave the wrong param type)
			//This is to show how the OnError method below will get raised instead
		}

		public void OnError(Exception obj)
		{
			ErrorContent = obj.Message;
			PropertyChange("ErrorContent");
		}

        //NOTE: in Silverlight, due to the sandboxed execution model, your subscription callback MUST
        //be public. The mediator is unable to dynamically invoke non public methods (it results in an 
        //MethodAccessException). The mediator can't effectivley warn aganist this currently so the callback 
        //will just get ignored and your method will not be invoked. 
        public void OnContent1MessageReceived(String messageContent)
        {
            MessageContent = messageContent;
            PropertyChange("MessageContent");

            Mediator.UnRegisterForMessage(Token);
        }

    }
}
