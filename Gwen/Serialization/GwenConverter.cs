using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gwen.Control;
using Gwen.ControlInternal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gwen.Serialization
{
    public class GwenConverter : JsonConverter
    {
        private static Canvas canvas;

        ~GwenConverter()
        {
            if (canvas != null)
            {
                canvas.Dispose();
                canvas = null;
            }
        }

        #region implemented abstract and virtual members of JsonConverter

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("$type");
            var type = value.GetType();
            serializer.Serialize(writer, type.Namespace + "." + type.Name + ", " + type.Assembly.GetName().Name);

            var props = value.GetType().GetProperties().Where(property => property.GetCustomAttributes(typeof(JsonPropertyAttribute), true).Length != 0);

            ControlBase b = null;

            TypeSwitch.Do(value,
                TypeSwitch.Case<CollapsibleCategory>(() => {
                    b = (CollapsibleCategory)Activator.CreateInstance(type, new CollapsibleList(new ScrollControl(new Canvas(Defaults.Skin))));
                }),
                TypeSwitch.Case<DownArrow>(() => {
                    b = (DownArrow)Activator.CreateInstance(type, new ComboBox(new Canvas(Defaults.Skin)));
                }),
                TypeSwitch.Case<PropertyRow>(() => {
                    Canvas c = new Canvas(Defaults.Skin);
                    b = (PropertyRow)Activator.CreateInstance(type, new object[] { c, new Gwen.Control.Property.PropertyBase(c) });
                }),
                TypeSwitch.Case<PropertyRowLabel>(() => {
                    Canvas c = new Canvas(Defaults.Skin);
                    b = (PropertyRowLabel)Activator.CreateInstance(type, new PropertyRow(c, new Gwen.Control.Property.PropertyBase(c)));
                }),
                TypeSwitch.Default(() => {
                    b = (ControlBase)Activator.CreateInstance(type, new Canvas(Defaults.Skin));
                })
            );

            foreach (var item in props)
            {
                var val = item.GetValue(value, null);
                var def = GetDefault(item.PropertyType);
                if (val == null && def == null)
                    continue;
                bool checkButtonOrLabel = (type == typeof(Button) || type == typeof(Label)) && item.Name == "Children";
                bool bol = val.Equals(def) || checkButtonOrLabel || val.Equals(type.GetProperty(item.Name).GetValue(b, null));

                var temp = def as IDisposable;
                if (temp != null)
                    temp.Dispose();

                if (bol)
                    continue;

                writer.WritePropertyName(item.Name);
                serializer.Serialize(writer, val);
            }

            writer.WriteEndObject();

            b.GetCanvas().Dispose();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (canvas == null)
            {
                canvas = new Canvas(Defaults.Skin);
            }

            JObject jsObject = JObject.Load(reader);
            Dictionary<string, JToken> properties = jsObject.Properties().ToDictionary(x => x.Name, x => x.Value);

            string[] strs = properties["$type"].Value<string>().Split(',').
                Where(x => !string.IsNullOrWhiteSpace(x) && x != ",").ToArray();

            for (int i = 0; i < strs.Length; i++) {
                strs[i] = strs[i].Trim();
            }

            Type t = Assembly.Load(strs[1]).GetType(strs[0]);

            ControlBase obj;

            if (t == typeof(Canvas))
            {
                obj = (Canvas)Activator.CreateInstance(t, Defaults.Skin);

                while (canvas.Children.Count > 0)
                {
                    canvas.Children[0].Parent = obj;
                }

                canvas.Dispose();
                canvas = null;
            }
            else {
                obj = (ControlBase)Activator.CreateInstance(t, canvas);
            }

            foreach (KeyValuePair<string, JToken> item in properties)
            {
                if (item.Key == "$type")
                    continue;

                PropertyInfo pinfo = t.GetProperty(item.Key);
                if (pinfo == null)
                    continue;

                object o = item.Value.Value<object>();

                if (item.Value.Type == JTokenType.Array)
                    o = (o as JArray).ToObject(pinfo.PropertyType);
                else
                    o = Convert.ChangeType(o, pinfo.PropertyType);

                pinfo.SetValue(obj, o, null);
            }

            return obj;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(ControlBase));
        }

        //public override bool CanRead { get { return true; } }

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

