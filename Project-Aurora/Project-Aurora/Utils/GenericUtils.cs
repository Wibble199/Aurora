using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Utils
{
    public static class TypeExtensions
    {
        public static object TryClone(this object self, bool deep = false)
        {
            if (self is ICloneable)
                return ((ICloneable)self).Clone();
            else if (deep) {
                var json = JsonConvert.SerializeObject(self, Formatting.None, JSONUtils.SerializerSettings);
                return JsonConvert.DeserializeObject(json, self.GetType(), JSONUtils.SerializerSettings);
            } else
                return self;
        }
    }
}
