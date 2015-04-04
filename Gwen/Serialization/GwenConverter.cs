using System;
using System.Linq;
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
            writer.WriteStartObject();

            writer.WritePropertyName("$type");
            var type = value.GetType();
            serializer.Serialize(writer, type.Namespace + "." + type.Name + ", " + type.Assembly.GetName().Name);

            var props = value.GetType().GetProperties().Where(property => property.GetCustomAttributes(typeof(JsonPropertyAttribute), true).Length != 0);

            var b = Activator.CreateInstance(type, new Canvas(Defaults.Skin));
            //Button b = new Button(new Canvas(Defaults.Skin));

            foreach (var item in props)
            {
                var val = item.GetValue(value, null);
                var def = GetDefault(item.PropertyType);
                if (val == null && def == null)
                    continue;
                bool bol = val.Equals(def) || item.Name == "Children" || val.Equals(type.GetProperty(item.Name).GetValue(b, null));
                if (bol)
                    continue;
                
                writer.WritePropertyName(item.Name);
                serializer.Serialize(writer, val);
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

        public static object GetDefault(Type type)
        {
            if(type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}

