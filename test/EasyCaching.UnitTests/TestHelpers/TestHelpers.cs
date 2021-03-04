namespace EasyCaching.UnitTests
{
    using FakeItEasy;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public class TestHelpers
    {
        public static string CreateTempFile(string contents)
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, contents);
            return tempFile;
        }

        public static Dictionary<string, string> GetMultiDict(string prefix = "")
        {
            return new Dictionary<string, string>()
            {
                {string.Concat(prefix,"key:1"), "value1"},
                {string.Concat(prefix,"key:2"), "value2"}
            };
        }

        public static  Func<string> CreateFakeDataRetriever(string result)
        {
            var func = A.Fake<Func<string>>();

            A.CallTo(() => func.Invoke()).Returns(result);

            return func;
        }

        public static  Func<string> CreateFakeDataRetrieverWithException(Exception exception)
        {
            var func = A.Fake<Func<string>>();

            A.CallTo(() => func.Invoke()).Throws(() => exception);

            return func;
        }

        public static Func<Task<string>> CreateFakeAsyncDataRetriever(string result)
        {
            var func = A.Fake<Func<Task<string>>>();
            
            A.CallTo(() => func.Invoke()).Returns(Task.FromResult(result));

            return func;
        }

        public static Func<Task<string>> CreateFakeAsyncDataRetrieverWithException(Exception exception)
        {
            var func = A.Fake<Func<Task<string>>>();
            
            A.CallTo(() => func.Invoke()).Throws(() => exception);

            return func;
        }
    }
}
