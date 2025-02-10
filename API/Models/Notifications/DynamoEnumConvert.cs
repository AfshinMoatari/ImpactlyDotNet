using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace API.Models.Notifications
{
    /// <summary>
    /// Converts an enum to and from a DynamoDBEntry.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    public class DynamoEnumConverter<TEnum> : IPropertyConverter
    {
        /// <summary>
        /// Converts a DynamoDBEntry to an enum value.
        /// </summary>
        /// <param name="entry">The DynamoDBEntry to convert.</param>
        /// <returns>The converted enum value.</returns>
        public object FromEntry(DynamoDBEntry entry)
        {
            string valueAsString = entry.AsString();
            TEnum valueAsEnum = (TEnum)System.Enum.Parse(typeof(TEnum), valueAsString, ignoreCase: true);
            return valueAsEnum;
        }

        /// <summary>
        /// Converts an enum value to a DynamoDBEntry.
        /// </summary>
        /// <param name="value">The enum value to convert.</param>
        /// <returns>The DynamoDBEntry representing the enum value.</returns>
        public DynamoDBEntry ToEntry(object value)
        {
            string valueAsString = value.ToString();
            DynamoDBEntry entry = new Primitive(valueAsString);
            return entry;
        }
    }
}