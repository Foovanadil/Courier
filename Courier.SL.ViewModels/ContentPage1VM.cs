using System;
using Courier.ViewModels;

namespace Courier.SL.ViewModels
{
    public class ContentPage1VM : BaseViewModel
    {
        public DelegateCommand<String> BroadcastMessage { get; set; }

        public ContentPage1VM()
        {
            BroadcastMessage = new DelegateCommand<String>(OnPublishMessage, CanPublishMessage);
        }

        private bool CanPublishMessage(String messageContent)
        {
            return true;
        }

        private void OnPublishMessage(String messageContent)
        {
            if (!String.IsNullOrEmpty(messageContent))
            {
                //NumberOfResend based caching example
                Mediator.BroadcastMessage("Content1Message", messageContent, new CacheSettings(1));
                //Time Based caching example
                //Mediator.BroadcastMessage("Content1Message", messageContent, new CacheSettings(DateTime.Now.Add(TimeSpan.FromSeconds(30))));
            }
        }


    }
}
