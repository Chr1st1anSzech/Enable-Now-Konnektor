using System;
using System.Collections.Generic;
using System.Text;

namespace Enable_Now_Konnektor.src.extension
{
    public interface IParameterConverter
    {
        public string[] TransformParameter(params string[] parameter);
    }
}
