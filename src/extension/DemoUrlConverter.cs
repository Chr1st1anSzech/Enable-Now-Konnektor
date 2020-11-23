using System;
using System.Collections.Generic;
using System.Text;

namespace Enable_Now_Konnektor.src.extension
{
    class DemoUrlConverter : IParameterConverter
    {
        public string[] TransformParameter(params string[] parameter)
        {
            if (parameter == null || parameter.Length == 0 || parameter[0] == null || parameter[1] == null) { return Array.Empty<string>(); }
            string id = parameter[0];
            string demoUrlPattern = parameter[1];
            return new string[] { demoUrlPattern.Replace("${Id}", id) };
        }
    }
}
