using System;
using System.Collections.Concurrent;
using FASTER.core;

namespace EasyCaching.FasterKv
{ 
    public class ClientSessionWrap : IDisposable
    {
        public ClientSession<SpanByte, SpanByte, SpanByte, SpanByteAndMemory, StoreContext, StoreFunctions> Session { get; }

        private readonly ConcurrentQueue<ClientSession<SpanByte, SpanByte, SpanByte, SpanByteAndMemory, StoreContext, StoreFunctions>> _innerPool;

        public ClientSessionWrap(
            ClientSession<SpanByte, SpanByte, SpanByte, SpanByteAndMemory, StoreContext, StoreFunctions> clientSession,
            ConcurrentQueue<ClientSession<SpanByte, SpanByte, SpanByte, SpanByteAndMemory, StoreContext, StoreFunctions>> innerPool)
        {
            Session = clientSession;
            _innerPool = innerPool;
        }
        
        public void Dispose()
        {
            Session.CompletePending(true);
            _innerPool.Enqueue(Session);
        }
    }
}