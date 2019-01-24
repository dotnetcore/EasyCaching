namespace EasyCaching.PerformanceTests
{
    using BenchmarkDotNet.Running;

    class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.Run<SerializeSingleObject2BytesBenchmark>();
            //BenchmarkRunner.Run<SerializeMultiObject2BytesBenchmark>();
            //BenchmarkRunner.Run<SerializerSingleObject2ArraySegmentBenchmark>();
            //BenchmarkRunner.Run<SerializerMultiObject2ArraySegmentBenchmark>();
            BenchmarkRunner.Run<SetBenchmark>();
            BenchmarkRunner.Run<GetBenchmark>();
        }
    }
}
