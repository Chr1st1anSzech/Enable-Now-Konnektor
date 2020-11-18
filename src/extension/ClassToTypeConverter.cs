using System;
using System.Collections.Generic;
using System.Text;

namespace Enable_Now_Konnektor.src.extension
{
    class ClassToTypeConverter : IParameterConverter
    {
        public string[] TransformParameter(params string[] parameter)
        {
            if(parameter == null || parameter.Length == 0) { return Array.Empty<string>(); }
            string[] values = new string[1];
            switch (parameter[0])
            {
                case "project":
                    {
                        values[0] = "Projekt";
                        break;
                    }
                case "slide":
                    {
                        values[0] = "Buchseite";
                        break;
                    }
                case "group":
                    {
                        values[0] = "Gruppe";
                        break;
                    }
                case "media":
                    {
                        values[0] = "Medien";
                        break;
                    }
                default:
                    {
                        values[0] = "Anderes";
                        break;
                    }
            }
            return values;
        }
    }
}
