using System;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace UpdaterLib
{
    [Serializable]
    [SettingsSerializeAs(SettingsSerializeAs.String)]
    [TypeConverter(typeof(UpdateInfoConverter))]
    public class UpdateInfo
    {
        public string? Version { get; set; }
        public DateTime? Modified { get; set; }

        public UpdateInfo()
        {
            Version = null;
            Modified = null;
        }
    }

    internal class UpdateInfoConverter: TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return System.Text.Json.JsonSerializer.Deserialize<UpdateInfo>((string)value, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return System.Text.Json.JsonSerializer.Serialize(value);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
