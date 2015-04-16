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
    public class GwenConverter : JsonConverter , IDisposable
    {
        private Canvas canvas;
        private Canvas stashCv;

        public GwenConverter()
        {
            canvas = new Canvas(Defaults.Skin);
            stashCv = new Canvas(Defaults.Skin);
        }

        #region IDisposable implementation

        public void Dispose()
        {
            if (canvas != null)
            {
                canvas.Dispose();
                canvas = null;
            }

            if (stashCv != null)
            {
                stashCv.Dispose();
                stashCv = null;
            }

            GC.SuppressFinalize(this);
        }

        #endregion

        #region implemented abstract and virtual members of JsonConverter

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var type = value.GetType();
            if ((type == typeof(RadioButton)) || (type == typeof(RadioButtonGroup)) || type.IsGenericType)
                return;

            writer.WriteStartObject();

            writer.WritePropertyName("$type");
            
            serializer.Serialize(writer, type.Namespace + "." + type.Name + ", " + type.Assembly.GetName().Name);

            var props = value.GetType().GetProperties().Where(property => property.GetCustomAttributes(typeof(JsonPropertyAttribute), true).Length != 0);

            object o = GetObjectInstance(type, stashCv, value);

            foreach (var item in props)
            {
                var val = item.GetValue(value, null);
                if (val == null)
                    continue;
                bool checkButtonOrLabel = (type == typeof(Button) || type == typeof(Label)) && item.Name == "Children";
                bool bol = checkButtonOrLabel || val.Equals(type.GetProperty(item.Name).GetValue(o, null));

                if (bol)
                    continue;
                
                if (item.Name == "Margin")
                {
                    writer.WritePropertyName(item.Name);
                    writer.WriteStartObject();

                    FieldInfo[] finfo = typeof(Margin).GetFields(BindingFlags.Public | BindingFlags.Instance);

                    for (int i = 0; i < finfo.Length; i++)
                    {
                        
                        writer.WritePropertyName(finfo[i].Name);
                        serializer.Serialize(writer, finfo[i].GetValue(val));
                    }

                    writer.WriteEndObject();

                    continue;
                }
                else if (item.Name == "Padding")
                {
                    writer.WritePropertyName(item.Name);
                    writer.WriteStartObject();

                    FieldInfo[] finfo = typeof(Padding).GetFields(BindingFlags.Public | BindingFlags.Instance);

                    for (int i = 0; i < finfo.Length; i++)
                    {

                        writer.WritePropertyName(finfo[i].Name);
                        serializer.Serialize(writer, finfo[i].GetValue(val));
                    }

                    writer.WriteEndObject();

                    continue;
                }

                writer.WritePropertyName(item.Name);
                serializer.Serialize(writer, val);
            }

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsObject = JObject.Load(reader);
            Dictionary<string, JToken> properties = jsObject.Properties().ToDictionary(x => x.Name, x => x.Value);

            string[] strs = properties["$type"].Value<string>().Split(',').
                Where(x => !string.IsNullOrWhiteSpace(x) && x != ",").ToArray();

            for (int i = 0; i < strs.Length; i++)
            {
                strs[i] = strs[i].Trim();
            }

            Type t = Assembly.Load(strs[1]).GetType(strs[0]);

            object obj = GetObjectInstance(t, stashCv);

            foreach (var item in properties)
            {
                if (item.Key == "$type")
                    continue;

                PropertyInfo pinfo = t.GetProperty(item.Key);
                if (pinfo == null)
                    continue;

                object o = item.Value.Value<object>();

                if (item.Key == "Margin")
                {
                    var props = (o as JObject).Properties();

                    int left = 0, top = 0, right = 0, bottom = 0;

                    foreach (var prop in props)
                    {
                        if (prop.Name == "Left")
                            left = prop.Value.Value<int>();
                        if (prop.Name == "Top")
                            top = prop.Value.Value<int>();
                        if (prop.Name == "Right")
                            right = prop.Value.Value<int>();
                        if (prop.Name == "Bottom")
                            bottom = prop.Value.Value<int>();
                    }

                    Margin m = new Margin(left, top, right, bottom);

                    pinfo.SetValue(obj, m, null);
                }
                else if (item.Key == "Padding")
                {
                    var props = (o as JObject).Properties();

                    int left = 0, top = 0, right = 0, bottom = 0;

                    foreach (var prop in props)
                    {
                        if (prop.Name == "Left")
                            left = prop.Value.Value<int>();
                        if (prop.Name == "Top")
                            top = prop.Value.Value<int>();
                        if (prop.Name == "Right")
                            right = prop.Value.Value<int>();
                        if (prop.Name == "Bottom")
                            bottom = prop.Value.Value<int>();
                    }

                    Padding m = new Padding(left, top, right, bottom);

                    pinfo.SetValue(obj, m, null);
                }
                else
                {
                    if (item.Value.Type == JTokenType.Array)
                        o = (o as JArray).ToObject(pinfo.PropertyType);
                    else if (item.Value.Type == JTokenType.Object)
                        o = (o as JObject).ToObject(pinfo.PropertyType);
                    else
                        o = Convert.ChangeType(o, pinfo.PropertyType);

                    pinfo.SetValue(obj, o, null);
                }
            }

            return obj;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(ControlBase)) || objectType == typeof(Margin) || objectType == typeof(Padding);
        }

        #endregion

        public static object GetDefault(Type type)
        {
            if(type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        private object GetObjectInstance(Type t, Canvas cv, object obj = null)
        {
            object o = null;

            TypeSwitch.CaseInfo[] cinfo =
            { 
                TypeSwitch.Case<CollapsibleCategory>(() =>
                    {
                        o = Activator.CreateInstance(t, new CollapsibleList(new ScrollControl(cv)));
                    }),
                TypeSwitch.Case<DownArrow>(() =>
                    {
                        o = Activator.CreateInstance(t, new ComboBox(cv));
                    }),
                TypeSwitch.Case<PropertyRow>(() =>
                    {
                        o = Activator.CreateInstance(t, new object[] { cv, new Gwen.Control.Property.PropertyBase(cv) });
                    }),
                TypeSwitch.Case<PropertyRowLabel>(() =>
                    {
                        o = Activator.CreateInstance(t, new PropertyRow(cv, new Gwen.Control.Property.PropertyBase(cv)));
                    }),
                TypeSwitch.Case<Margin>(() =>
                    {
                        o = Activator.CreateInstance(t, new object[] { 0, 0, 0, 0 });
                    }),
                TypeSwitch.Case<Padding>(() =>
                    {
                        o = Activator.CreateInstance(t, new object[] { 0, 0, 0, 0 });
                    }),
                TypeSwitch.Default(() =>
                    {
                        o = Activator.CreateInstance(t, cv);
                    })
            };

            if (obj == null)
                TypeSwitch.Do(t, cinfo);
            else
                TypeSwitch.Do(obj, cinfo);

            return o;
        }
    }
}

