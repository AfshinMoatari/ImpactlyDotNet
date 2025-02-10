namespace API.Extensions
{
    public static class StringExtension
    {
        public static string IdUrlEncode(this string str)
        {
            var lower = str.ToLower();
            var replace = lower.Replace(" ", "-");
            return replace;
        }
    }
}