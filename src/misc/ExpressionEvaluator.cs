using Enable_Now_Konnektor.src.extension;
using Enable_Now_Konnektor_Bibliothek.src.config;
using Enable_Now_Konnektor_Bibliothek.src.service;
using log4net;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Enable_Now_Konnektor_Bibliothek.src.misc
{
    internal class ExpressionEvaluator
    {
        private static readonly ILog s_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Prüft, ob es sich bei dem Text, um einen Ausdruck, der interpretiert werden soll, handelt.
        /// </summary>
        /// <param name="textToExamine">Der Text, der geprüft werden soll.</param>
        /// <param name="expression">Der Ausdruck, falls einer gefunden wird. Andernfalls wird null zurückgegeben.</param>
        /// <returns>Wahr, wenn es ein Ausdruck ist.</returns>
        internal static bool IsExpression(string textToExamine, out string expression)
        {
            if (string.IsNullOrWhiteSpace(textToExamine)) { expression = null; return false; }

            string expressionPattern = @"(?<=\${).+(?=})";
            Match expressionMatch = Regex.Match(textToExamine, expressionPattern);
            bool isExpression = expressionMatch.Success;
            expression = isExpression ? expressionMatch.Value : null;
            return isExpression;
        }

        /// <summary>
        /// Prüft, ob es sich bei dem Ausdruck, um eine Variable handelt.
        /// </summary>
        /// <param name="expression">Der Ausdruck, der geprüft werden soll.</param>
        /// <param name="variableName">Die Variable, falls eine gefunden wird. Andernfalls wird null zurückgegeben.</param>
        /// <returns>Wahr, wenn es eine Variable ist.</returns>
        internal static bool IsVariableExpression(string expression, out string variableName)
        {
            Config cfg = ConfigManager.GetConfigManager().ConnectorConfig;
            string variablePattern = $"^({cfg.EntityIdentifier}|{cfg.LessonIdentifier}|{cfg.SlideIdentifier})\\w+$";
            Match variableMatch = Regex.Match(expression, variablePattern);
            bool isVariableExpression = variableMatch.Success;
            variableName = isVariableExpression ? variableMatch.Value : null;
            return isVariableExpression;
        }

        internal static bool IsConverterExpression(string expression, out string converterClassName, out string[] converterParameterNames)
        {
            Match converterClassNameMatch = Regex.Match(expression, "^\\w+(?=\\(([^,]+,?)+\\)$)");
            converterClassName = converterClassNameMatch.Success ? converterClassNameMatch.Value : null;
            Match parametersMatch = Regex.Match(expression, "(?<=^\\w+\\()([^,]+,?)+(?=\\)$)");
            converterParameterNames = parametersMatch.Success ? parametersMatch.Value.Split(',') : null;
            return converterClassNameMatch.Success && parametersMatch.Success;
        }


        internal static string[] EvaluateAsConverter(string converterClassName, params string[] parameters)
        {
            Type converterType = GetConverterType(converterClassName);
            if (converterType == null) { return Array.Empty<string>(); }

            try
            {
                IParameterConverter converter = Activator.CreateInstance(converterType) as IParameterConverter;
                MethodInfo method = converterType.GetMethod("TransformParameter");
                return method.Invoke(converter, new object[] { parameters }) as string[];
            }
            catch (Exception exception)
            {
                s_log.Warn(LocalizationService.FormatResourceString("ExpressionEvaluatorMessage01", converterClassName, parameters), exception);
            }
            return Array.Empty<string>();
        }

        private static Type GetConverterType(string converterClassName)
        {
            Type type = typeof(IParameterConverter);
            Type converterType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => type.IsAssignableFrom(t) && t.IsClass && t.Name.Equals(converterClassName));
            return converterType;
        }

    }
}
