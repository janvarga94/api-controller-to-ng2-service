using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ApiControllerToAngularInjectables
{
    class CSharpTranslator
    {
        private const string recognitionRegExp = @"\[\s*Http(.+)\s*\][\s\S]*?\[\s*Route\s*\(\s*" +"\"(.+)\"" + @"\s*\)\s*\]\s*.+public\s+(.+)\s+(.+)\s*\((.*)\)";
        private const string recogintionOfBaseApiRoute = @"\[\s*RoutePrefix\s*\(\s*" + "\"(.*)\"" + @"\s*\)\s*\]";
        private const string classNameRecognition = @"public\s*class\s*(.+)\s*:";
     

        public string Generate(string inputFileText)
        {
            var extractDataInputParams = new List<Attribute>() { };
            var extractDataMethod = new Method("extractData", "void", extractDataInputParams);

            var handleErrorInputParams = new List<Attribute>() { };
            var handleErrorMethod = new Method("handleError", "void", extractDataInputParams);

            MatchCollection matches = Regex.Matches(inputFileText, recognitionRegExp);

            var methods = new List<NgServiceMethod>();
            foreach (Match match in matches)
            {
                var fullMatch = match.Groups[0].Value;
                var httMethodType = match.Groups[1].Value;
                var apiUrl = match.Groups[2].Value;
                var returnType = match.Groups[3].Value;
                var methodName = match.Groups[4].Value;
                var parameters = match.Groups[5].Value;

                var finalParameters = getParameters(parameters);
                var finalApiUrl = getApiUrl(apiUrl, finalParameters);
                var finalRequestType = GetRequestType(httMethodType);

                methods.Add(new NgServiceMethod(
                    methodName, "any", finalParameters, extractDataMethod,
                    handleErrorMethod, "http", finalRequestType, finalApiUrl)
                );

            }


            var open = "{";
            var close = "}";
            return $"{getImports()}@Injectable()\nexport class {getClassNameReplacesControllerWordWithService(inputFileText)}{open}\n{generateConstructor()}\n{string.Join("\n\n", methods.Select(m => m.ToString()))}\n{generateExtractDataFunction()}{generateHandleErrorFunction()}{close}";
        }

        private string generateConstructor() {
            return @"
                constructor (private http: Http) {}
            ";
        }

        private string generateExtractDataFunction() {
            return @"
                private handleError (error: Response | any) {
                    let errMsg: string;
                    if (error instanceof Response) {
                      const body = error.json() || '';
                      const err = body.error || JSON.stringify(body);
                      errMsg = `${error.status} - ${error.statusText || ''} ${err}`;
                    } else {
                      errMsg = error.message ? error.message : error.toString();
                    }
                    console.error(errMsg);
                    return Observable.throw(errMsg);
                  }
                ";
        }

        private string generateHandleErrorFunction()
        {
            return @"
                private extractData (res: Response) {
                    let body = res.json();
                    return body.data || { };
                }
                ";
        }

        private string getImports() {
            return @"
                import { Injectable } from '@angular/core';
                import { Http, Response } from '@angular/http';
                import { Observable }  from 'rxjs/Observable';
                import { Subject }    from 'rxjs/Subject';
            ";
        }

        private string getApiUrl(string formerUrl, List<Attribute> detectedParameters)
        {
            return formerUrl;
        }

        private string getClassNameReplacesControllerWordWithService(string inputFileText) {
            var name = getClassName(inputFileText);
            return name.Replace("Controller", "Service");
        }

        private string getClassName(string inputFileText) {
            MatchCollection matches = Regex.Matches(inputFileText, classNameRecognition);
            if (matches.Count > 0)
            {
                if (matches[0].Groups.Count > 1)
                {
                    return matches[0].Groups[1].Value;
                }
            }
            return "";
        }

        private string getBasePath(string inputFileText)
        {
            MatchCollection matches = Regex.Matches(inputFileText, recogintionOfBaseApiRoute);
            if(matches.Count > 0)
            {
                if(matches[0].Groups.Count > 1)
                {
                    return matches[0].Groups[1].Value;
                }
            }
            return "";
        }

        private List<Attribute> getParameters(string parameters)
        {
            var attributes = new List<Attribute>();

            var splitedParameters = parameters.Split(',').Select(p => p.Trim());
            foreach(var splited in splitedParameters){
                var nameTypeSplit = splited.Split(' ').Select(p => p.Trim());
                if(nameTypeSplit.Count() == 2)
                {
                    attributes.Add(new Attribute() { Name = nameTypeSplit.ElementAt(1), Type = nameTypeSplit.ElementAt(0) });
                }
            }

            return attributes;
        }

        private HttpRequestType GetRequestType(String requestType)
        {
            switch (requestType)
            {
                case "Post": 
                    return HttpRequestType.Post;
                case "Get":
                    return HttpRequestType.Get;
                case "Put":
                    return HttpRequestType.Put;
                case "Delete":
                    return HttpRequestType.Delete;

                default:
                    return HttpRequestType.Unknown;
            }
        }
    }
}
