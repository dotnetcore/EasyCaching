namespace EasyCaching.Core.Diagnostics
{
    public class BeforeSubscribeMessageRequestEventData //: EventData
    {
        public BeforeSubscribeMessageRequestEventData(Bus.EasyCachingMessage msg)
        {
            this.Msg = msg;
        }

        public Bus.EasyCachingMessage Msg { get; set; }
    }
}
