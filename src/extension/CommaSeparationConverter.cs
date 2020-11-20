using System;

namespace Enable_Now_Konnektor.src.extension
{
    class CommaSeparationConverter : IParameterConverter
    {
        public string[] TransformParameter(params string[] parameter)
        {
            if (parameter == null || parameter.Length == 0 || parameter[0] == null) { return Array.Empty<string>(); }
            return parameter[0].Split(';');
        }
    }
}
