using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Clc.Polaris.Api
{
    /// <summary>
    /// Helper extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Serialize an object to XML
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns>The input object serialized as XML</returns>
        public static string Serialize<T>(this T value)
        {
            var xmlserializer = new XmlSerializer(typeof(T));
            var stringWriter = new StringWriter();
            using (var writer = XmlWriter.Create(stringWriter))
            {
                xmlserializer.Serialize(writer, value);
                return stringWriter.ToString();
            }
        }

        public static void AddIfNotNull(this XElement root, string name, object val)
        {
            if (val != null) root.Add(new XElement(name, val));
        }
    }
}
