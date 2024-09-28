using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace Jeopardy.Util.Json
{
    /// <summary>
    /// A <see cref="JsonConverter{TInterface}"/> that converts any instance of <see cref="TInterface"/> to <see cref="TClass"/> to allow for instantiation.
    /// </summary>
    /// <typeparam name="TInterface">The interface to convert to and from JSON.</typeparam>
    /// <typeparam name="TClass">An implementation of <see cref="TInterface"/> that this class has read / write access to.</typeparam>
    public class InterfaceConverter<TInterface, TClass> : JsonConverter<TInterface> where TClass : TInterface, new()
    {
        public override TInterface? ReadJson(JsonReader reader, Type objectType, TInterface? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            TClass? result = Activator.CreateInstance<TClass>();
            PropertyInfo[] writableProperties = typeof(TClass).GetProperties();
            PropertyInfo[] neededProperties = typeof(TInterface).GetProperties();
            foreach (PropertyInfo info in writableProperties.Intersect(neededProperties, new PropertyInfoComparer()))
            {
                if (!info.CanWrite)
                    throw new InvalidOperationException($"Could not write to \"{info.Name}\" property of {typeof(TClass)}.");

                object? value = jo[info.Name]?.ToObject(info.PropertyType);
                typeof(TClass).GetProperty(info.Name)?.SetValue(result, value);
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, TInterface? value, JsonSerializer serializer)
        {
            JObject jo = [];
            PropertyInfo[] neededProperties = typeof(TInterface).GetProperties();
            foreach (PropertyInfo info in neededProperties)
            {
                if (!info.CanRead)
                    throw new InvalidOperationException($"Could not read from \"{info.Name}\" property of {typeof(TInterface)}.");

                object? propVal = info.GetValue(value);
                if (propVal is not null)
                    jo.Add(info.Name, JToken.FromObject(propVal, serializer));
            }

            jo.WriteTo(writer);
        }

        private class PropertyInfoComparer : IEqualityComparer<PropertyInfo>
        {
            public bool Equals(PropertyInfo? x, PropertyInfo? y)
            {
                return x?.Name == y?.Name;
            }

            public int GetHashCode(PropertyInfo obj)
            {
                return obj.Name.GetHashCode();
            }
        }
    }
}
