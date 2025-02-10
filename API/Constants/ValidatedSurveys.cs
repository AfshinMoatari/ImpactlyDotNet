using System.Collections.Generic;
using static Nest.Language;

namespace API.Constants;

public static class ValidatedSurveys
{
    public static List<string> GetValidatedSurveyIdsAll()
    {
        return new List<string>()
        {
            "eq-5d-5l",
            "gse",
            "gseen",
            "kidsscreen10",
            "pss10",
            "pss10en",
            "qoladb",
            "qoladp",
            "rcads",
            "rcads47",
            "sdq",
            "swemwbs",
            "swemwbsen",
            "swls",
            "swls1",
            "swls5",
            "swls5en",
            "ucla3",
            "ucla3en",
            "who5",
            "who5en",
        };
    }
    public static List<string> GetValidatedSurveyIdsDanish()
    {
        return new List<string>()
        {
            "eq-5d-5l",
            "gse",
            "kidsscreen10",
            "pss10",
            "qoladb",
            "qoladp",
            "rcads",
            "rcads47",
            "sdq",
            "swemwbs",
            "swls",
            "swls1",
            "swls5",
            "ucla3",
            "who5",
            "who5en",
        };
    }
    public static List<string> GetValidatedSurveyIdsEnglish()
    {
        return new List<string>()
        {
            "gseen",
            "pss10en",
            "swemwbsen",
            "swls5en",
            "ucla3en",
            "who5en",
        };
    }

    public static bool ifValidatedSurvey(string language, string surveyId)
    {
        List<string> list;
        switch (language)
        {
            case Languages.Danish:
                list = GetValidatedSurveyIdsDanish();
                return list.Contains(surveyId);
            case Languages.English:
                list = GetValidatedSurveyIdsEnglish();
                return list.Contains(surveyId);
            default:
                list = GetValidatedSurveyIdsAll();
                return list.Contains(surveyId);
        }
    }
}