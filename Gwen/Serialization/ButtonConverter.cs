using System;
using Gwen.Control;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gwen.Serialization
{
    public class ButtonConverter : JsonConverter
    {
        #region implemented abstract and virtual members of JsonConverter

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var obj = value as Button;
            writer.WriteStartObject();

            var jObject = (JObject)JToken.FromObject(value);

            foreach (var item in jObject.Properties())
            {
                if (item.Name == "Children")
                    continue;
                
                writer.WritePropertyName(item.Name);
                serializer.Serialize(writer, item.Value);
            }

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Not needed");
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Button).IsAssignableFrom(objectType);
        }

        public override bool CanRead { get { return false; } }

        #endregion
    }
}

