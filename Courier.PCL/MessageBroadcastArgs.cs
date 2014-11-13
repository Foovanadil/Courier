using System;

namespace Courier
{
    internal class MessageBroadcastArgs : EventArgs
    {
        public String Message { get; set; }
        public Object Payload { get; set; }

        internal MessageBroadcastArgs(String message, Object payload)
        {
            Message = message;
            Payload = payload;
        }
    }
}