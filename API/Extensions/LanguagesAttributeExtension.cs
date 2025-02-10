using API.Constants;
using Microsoft.IdentityModel.Tokens;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace API.Extensions
{
    public interface ILanguageAttributeExtension
    {
        public string[] GetLanguageSpecificCultures(Type languages, string language);
        public string GetDescriptionByLanguage(Type enumType, Enum value, string language);
    }


    public class LanguageAttributeExtension : ILanguageAttributeExtension
    {
        public string[] GetLanguageSpecificCultures(Type languages, string language)
        {
            var attribute = (MultiCulturalDescription)languages.GetFields().Where(x => x.Name.Equals(language)).FirstOrDefault().GetCustomAttribute(typeof(MultiCulturalDescription), false);

            return (attribute == null || attribute.Cultures.IsNullOrEmpty()) ? null : attribute.Cultures;
        }

        public string GetDescriptionByLanguage(Type enumType, Enum value, string language)
        {
            var member = enumType.GetMember(value.ToString()).FirstOrDefault();
            if (member != null)
            {
                var attribute = (MultiCulturalDescription)member.GetCustomAttribute(typeof(MultiCulturalDescription), false);
                if (attribute != null && attribute.Cultures != null && attribute.Cultures.Length > 0)
                {
                    var index = Languages.FindLanguageIndex(language);
                    if (index >= 0 && index < attribute.Cultures.Length)
                    {
                        return attribute.Cultures[index];
                    }
                }
            }
            return string.Empty;
        }

        private string GetEnumDescription(Type enumType, Enum value)
        {
            var member = enumType.GetMember(value.ToString()).FirstOrDefault();
            if (member != null)
            {
                var attribute = (MultiCulturalDescription)member.GetCustomAttribute(typeof(MultiCulturalDescription), false);
                if (attribute != null && attribute.Cultures != null && attribute.Cultures.Length > 0)
                {
                    return attribute.Cultures[0];
                }
            }
            return value.ToString();
        }

        [AttributeUsage(AttributeTargets.All)]
        public class MultiCulturalDescription : DescriptionAttribute
        {
            public string[] Cultures { get; set; }

            public MultiCulturalDescription(string[] cultures)
            {
                Cultures = cultures;
            }
        }
    }
}
