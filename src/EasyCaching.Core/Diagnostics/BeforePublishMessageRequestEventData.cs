namespace EasyCaching.Core.Diagnostics
{
    public class BeforePublishMessageRequestEventData //: EventData
    {
        public BeforePublishMessageRequestEventData(string topic, Bus.EasyCachingMessage msg)
        {
            this.Topic = topic;
            this.Msg = msg;
        }

        public string Topic { get; set; }

        public Bus.EasyCachingMessage Msg { get; set; }
    }
}
