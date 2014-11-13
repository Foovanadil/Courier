using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Courier.ViewModels;

namespace Courier.SL.ViewModels
{
    public class ThreadedMessageVM : BaseViewModel
    {
        public ObservableCollection<String> ThreadedMessageResults { get; set; }
        public DelegateCommand<Object> ThreadMessages { get; set; }

        public ThreadedMessageVM()
        {
            ThreadedMessageResults = new ObservableCollection<String>();
            ThreadMessages = new DelegateCommand<Object>(OnThreadMessages);
        }

        private void OnThreadMessages(object obj)
        {
            //Spin off a background thread that casues a handful of messages to be broadcast and cached 
            //then register for the message afterwards and get the whole cache
            var worker = new BackgroundWorker();
            worker.RunWorkerCompleted += WorkerCompleted;
            worker.DoWork += BroadcastMessagesOnASeperateThread;
            worker.RunWorkerAsync();
        }

        private void BroadcastMessagesOnASeperateThread(object sender, DoWorkEventArgs e)
        {
            //Set the resend value to 1 to let the mediator clean up each message after we grab out of cache the first time
            var cache = new CacheSettings(1);
            for (int i = 0; i < 15; i++)
            {
                Mediator.BroadcastMessage("Threaded Message", String.Format("message number {0}", i), cache);
            }
        }

        private void WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Get all the messages that are cached
            MessageToken token = Mediator.RegisterForMessage<String>("Threaded Message", GetMessage);
            //Then unhook from the message
            Mediator.UnRegisterForMessage(token);
        }

        private void GetMessage(String payload)
        {
            //Pump each message payload to the UI thread and add it to the message collection so our items control 
            //will update to show the messages
            ThreadedMessageResults.Add(payload);
        }
    }
}
