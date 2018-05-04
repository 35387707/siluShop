using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace silushop.Utils
{
    public static class NewtonJSONHelper
    {
        public static string SerializeObject(this object obj, string dateformart = null)
        {
            if (!string.IsNullOrEmpty(dateformart))
            {
                IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();
                timeFormat.DateTimeFormat = dateformart;
                return JsonConvert.SerializeObject(obj, Formatting.Indented, timeFormat);
            }

            return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        public static T DeserializeObject<T>(this string data)
        {
            return JsonConvert.DeserializeObject<T>(data, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}