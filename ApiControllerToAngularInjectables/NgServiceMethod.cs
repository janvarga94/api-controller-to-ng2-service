using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiControllerToAngularInjectables
{
    class NgServiceMethod : Method
    {
        public HttpRequestType _requestType;
        public string _apiUrl;

        private Method _extractDataCallbackMethod;
        private Method _handleCallbackMethod;
        private string _httpPrivateVariableName;

        private const string uriEncodationFunctionName = "encodeURIComponent";

        public NgServiceMethod(string name, string returnType, List<Attribute> inputParameters, 
            Method extractDataCallbackMethod,
            Method handleCallbackMethod,
            string httpPrivateVariableName,
            HttpRequestType requestType,
            string apiUrl)
                :base(name,returnType, inputParameters)
        {
            _extractDataCallbackMethod = extractDataCallbackMethod;
            _handleCallbackMethod = handleCallbackMethod;
            _httpPrivateVariableName = httpPrivateVariableName;

            _requestType = requestType;
            _apiUrl = apiUrl;

            //set all types to 'any'
            this.InputParameters = inputParameters.Select(ip => new Attribute() { Name = ip.Name, Type = "any" }).ToList();
        }

        public override string ToString()
        {
            string ngHttpRequestTypeName = "unknown";
            switch (_requestType)
            {
                case HttpRequestType.Delete:
                    ngHttpRequestTypeName = "delete";
                    break;
                case HttpRequestType.Get:
                    ngHttpRequestTypeName = "get";
                    break;
                case HttpRequestType.Put:
                    ngHttpRequestTypeName = "put";
                    break;
                case HttpRequestType.Post:
                    ngHttpRequestTypeName = "post";
                    break;
            }
              
            var openBrackets = "{";
            var closeBrackets = "}";
            var commaIfNeeded = InputParameters.Any() && (_requestType == HttpRequestType.Post || _requestType == HttpRequestType.Put) ? "," : "";
            var plusIfNeeded = InputParameters.Any() && (_requestType == HttpRequestType.Get || _requestType == HttpRequestType.Delete)? "+" : "";
           
            return $@"
                {Name}({string.Join(", ",InputParameters)}): Observable<{ReturnType}>{openBrackets}
                    return this.{_httpPrivateVariableName}.{ngHttpRequestTypeName}({GetHttpParameters()})
                        .map(this.{_extractDataCallbackMethod.Name})
                        .catch(this.{_handleCallbackMethod.Name});
                    {closeBrackets}
            ".Trim();
        }


        private string GetHttpParameters() {
            if (_requestType == HttpRequestType.Post || _requestType == HttpRequestType.Put)
            {
                if (InputParameters.Count > 0)
                    return $"'{_apiUrl}', {InputParameters[0].Name}";
                else
                    return $"'{_apiUrl}'";
            }
            else if (_requestType == HttpRequestType.Get || _requestType == HttpRequestType.Delete)
            {
                var inputMappedInRoute = InputParameters.Where(ip => _apiUrl.ToLower().Contains("{"+ip.Name.ToLower()+"}")).ToList();
                var inputNotMappedInRoute = InputParameters.Except(inputMappedInRoute).ToList();

                var replacedRouteParameters = _apiUrl;
                foreach (var inputMapped in inputMappedInRoute)
                {
                    var indexForReplacement = replacedRouteParameters.ToLower().IndexOf("{" + inputMapped.Name.ToLower() + "}");
                    replacedRouteParameters =
                        replacedRouteParameters.Substring(0, indexForReplacement) +
                         " \" + " + uriEncodationFunctionName + "(" + inputMapped.Name + ") + \"" +
                        replacedRouteParameters.Substring(indexForReplacement + inputMapped.Name.Length + 2);
                }
                var finishedReplaceRouteParametersPart = "\"" + replacedRouteParameters + "\"";
                var finishedNotInputMappedPart =$"{uriEncodationFunctionName}({string.Join(" + '&' + ", inputNotMappedInRoute.Select(inm => $"'{inm.Name}=' + {inm.Name}"))})";

                if (inputNotMappedInRoute.Any())
                    return finishedReplaceRouteParametersPart + " + \"?\" + " + finishedNotInputMappedPart;
                else{
                    return finishedReplaceRouteParametersPart;
                }
            }
            return $@"{_apiUrl}";
        }
    }
}
