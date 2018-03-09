namespace EasyCaching.PerformanceTests
{    
    using BenchmarkDotNet.Running;

    class Program
    {
        static void Main(string[] args)
        {            
            BenchmarkRunner.Run<SerializerBenchmark>();
        }
    }
}
