namespace EasyCaching.UnitTests
{
    using System.Collections.Generic;
    using System.IO;

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
    }
}
