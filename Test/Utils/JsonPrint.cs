using Newtonsoft.Json;
using Xunit.Abstractions;

namespace Impactly.Test.Utils
{
    public static class JsonPrint
    {
        public static void PrintJson(this ITestOutputHelper outputHelper, object value)
        {
            outputHelper.WriteLine(JsonConvert.SerializeObject(value, Formatting.Indented));
        }
        
        public static void PrintJson(this ITestOutputHelper outputHelper, string identifier, object value)
        {
            var s = identifier + JsonConvert.SerializeObject(value, Formatting.Indented);
            outputHelper.WriteLine(s);
        }
    }
}