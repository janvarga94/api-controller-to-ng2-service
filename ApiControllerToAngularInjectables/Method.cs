using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiControllerToAngularInjectables
{
    class Method
    {
        public List<Attribute> InputParameters;
        public string ReturnType;
        public string Name;

        public Method(string name, string returnType, List<Attribute> inputParameters)
        {
            if (name == null || returnType == null || inputParameters == null) throw new Exception("all construct parameters are mandatory");
            Name = name;
            ReturnType = returnType;
            InputParameters = inputParameters;
        }

    }
}
