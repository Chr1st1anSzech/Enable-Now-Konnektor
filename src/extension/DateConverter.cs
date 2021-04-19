using Enable_Now_Konnektor_Bibliothek.src.jobs;
using Enable_Now_Konnektor_Bibliothek.src.misc;
using System;

namespace Enable_Now_Konnektor.src.extension
{
    class DateConverter : IParameterConverter
    {
        public string[] TransformParameter(params string[] parameter)
        {
            bool isInvalid = parameter == null ||
                parameter.Length == 0 ||
                parameter[0] == null ||
                ( !JobManager.GetJobManager().SelectedJobConfig.UseTodayWhenDateEmpty && string.IsNullOrWhiteSpace(parameter[0]) );
            if (isInvalid) { return Array.Empty<string>(); }

            string[] values = new string[1];
            values[0] = Util.ConvertToUnixTime(parameter[0]);
            return values;
        }
    }
}
