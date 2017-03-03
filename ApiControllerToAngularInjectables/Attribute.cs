using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiControllerToAngularInjectables
{
    public class Attribute
    {
        public string Type;
        public string Name;

        public override string ToString()
        {
            return $"{ Name } : {Type}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Attribute) {
                var attr = (Attribute) obj;
                if (Type + Name == attr.Type + attr.Name)
                    return true;
            }
            return false;
        }
    }
}
