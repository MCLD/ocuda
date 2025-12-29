using System;
using System.ComponentModel;
using System.Reflection;

namespace Ocuda.Utility.Extensions
{
    public static class ObjectExtensions
    {
        public static string GetDescriptionAttributeText(this object item)
        {
            ArgumentNullException.ThrowIfNull(item);

            return item.GetType()
                .GetField(item.ToString())
                .GetCustomAttribute<DescriptionAttribute>()
                .Description;
        }
    }
}
