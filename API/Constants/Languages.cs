using static API.Extensions.LanguageAttributeExtension;

namespace API.Constants;

public static class Languages
{
    [MultiCulturalDescription(new string[] { "en-US", "en-GB", "da-DK" })]
    public const string Danish = "Danish";
    [MultiCulturalDescription(new string[] { "en-US", "en-GB", "da-DK" })]
    public const string English = "English";
    public const string Default = Danish;

    public const string ISO8601DateFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";


    public static int FindLanguageIndex(string language)
    {
        var fields = typeof(Languages).GetFields();

        for (int i = 0; i < fields.Length; i++)
        {
            var fieldValue = (string)fields[i].GetValue(null);
            if (fieldValue == language)
            {
                return i;
            }
        }

        return -1;
    }
}