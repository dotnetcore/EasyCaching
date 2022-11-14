using FASTER.core;

namespace EasyCaching.FasterKv;

public class StoreContext
{
    private Status _status;
    private SpanByteAndMemory _output;

    internal void Populate(ref Status status, ref SpanByteAndMemory output)
    {
        _status = status;
        _output = output;
    }

    internal void FinalizeRead(out Status status, out SpanByteAndMemory output)
    {
        status = _status;
        output = _output;
    }
}

public class StoreFunctions : SpanByteFunctions<StoreContext>
{
    public override void ReadCompletionCallback(ref SpanByte key,
        ref SpanByte input,
        ref SpanByteAndMemory output,
        StoreContext ctx,
        Status status,
        RecordMetadata recordMetadata)
    {
        ctx?.Populate(ref status, ref output);
    }
}