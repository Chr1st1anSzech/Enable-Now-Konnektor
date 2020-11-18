using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.extension;
using log4net;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Enable_Now_Konnektor.src.misc
{
    class ExpressionEvaluator
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool IsExpression(string textToExamine, out string expression)
        {
            string expressionPattern = @"(?<=\${).+(?=})";
            Match expressionMatch = Regex.Match(textToExamine, expressionPattern);
            bool isExpression = expressionMatch.Success;
            expression = isExpression ? expressionMatch.Value : null;
            return isExpression;
        }


        public bool IsVariableExpression(string expression, out string variableName)
        {
            Config cfg = ConfigReader.LoadConnectorConfig();
            string variablePattern = $"^({cfg.EntityIdentifier}|{cfg.LessonIdentifier}|{cfg.SlideIdentifier})\\w+$";
            Match variableMatch = Regex.Match(expression, variablePattern);
            bool isVariableExpression = variableMatch.Success;
            variableName = isVariableExpression ? variableMatch.Value : null;
            return isVariableExpression;
        }

        public bool IsConverterExpression(string expression, out string converterClassName, out string converterVariableName)
        {
            Config cfg = ConfigReader.LoadConnectorConfig();
            string variablePattern = $"({cfg.EntityIdentifier}|{cfg.LessonIdentifier}|{cfg.SlideIdentifier})\\w+";
            string converterPattern = $"^\\w+\\({variablePattern}\\)$";
            Match converterMatch = Regex.Match(expression, converterPattern);
            bool isConverterExpression = converterMatch.Success;
            converterClassName = isConverterExpression ? converterMatch.Value.Split('(')[0] : null;
            converterVariableName = isConverterExpression ? converterMatch.Value.Split('(')[1].Replace(")", "") : null;
            return isConverterExpression;
        }


        public string[] EvaluateAsConverter(string value, string converterClassName)
        {
            Type converterType = GetConverterType(converterClassName);
            if (converterType == null) { return Array.Empty<string>(); }

            try
            {
                var o = Activator.CreateInstance(converterType) as IParameterConverter;
                MethodInfo method = converterType.GetMethod("TransformParameter");
                return method.Invoke(o, new object[] { new string[] { value } }) as string[];
            }
            catch (Exception e)
            {
                log.Warn(Util.GetFormattedResource("ExpressionEvaluatorMessage01", converterClassName, value), e);
            }
            return Array.Empty<string>();
        }

        private static Type GetConverterType(string converterClassName)
        {
            Type type = typeof(IParameterConverter);
            Type converterType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .FirstOrDefault(p => type.IsAssignableFrom(p) && p.IsClass && p.Name.Equals(converterClassName));
            return converterType;
        }

    }
}
