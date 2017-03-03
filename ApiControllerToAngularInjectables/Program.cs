using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiControllerToAngularInjectables
{
    class Program
    {
        static void Main(string[] args)
        {
            var extractDataInputParams = new List<Attribute>() { };
            var extractDataMethod = new Method("extractData", "void", extractDataInputParams);

            var handleErrorInputParams = new List<Attribute>() { };
            var handleErrorMethod = new Method("handleError", "void", extractDataInputParams);

            var ngServiceInputParams = new List<Attribute>() {
                new Attribute() { Name = "par1", Type = "CustomType" },
                new Attribute() { Name = "par2", Type = "School"},
            };
            var ngServiceMethod = new NgServiceMethod("getEntities", "Entits[]",
                ngServiceInputParams, 
                extractDataMethod,
                handleErrorMethod,
                "http",
                HttpRequestType.Get,
                "http:xDxD"
            );

            //     System.Windows.Forms.MessageBox.Show(ngServiceMethod.ToString());

            //   Console.WriteLine(ngServiceMethod.ToString());

            var file = new System.IO.StreamWriter(@"generated.ts");
            try
            {
                var translator = new CSharpTranslator();
                var result = translator.Generate(File.ReadAllText("apiController.cs"));
                file.Write(result);
            }
            finally
            {
                file.Close();
            }

         

        }
    }
}
