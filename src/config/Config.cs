using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Enable_Now_Konnektor.src.config
{
    public class Config
    {
        public string IndexUrl { get; set; }
        public string RemoveUrl { get; set; }
        public string ConverterUrl { get; set; }
        public string FetchUrl { get; set; }

        public string ProxyUrl { get; set; } = "http://proxy.de";
        public int ProxyPort { get; set; } = 0;
        public bool UseProxy { get; set; } = false;

        public string StringIdentifier { get; set; } = "_str";
        public string FacetIdentifier { get; set; } = "_facet";
        public string LongIdentifier { get; set; } = "_long";

        public string UidFieldName { get; set; } = "uid";
        public string ClassFieldName { get; set; } = "class";
        public string UrlFieldName { get; set; } = "url";
        public string DateFieldName { get; set; } = "datelastmodified";
        public string BodyFieldName { get; set; } = "body";
        public string MimeTypeFieldName { get; set; } = "mimetype";
        public string ApplicationFieldName { get; set; } = "application";
        public string ContentTypeFieldName { get; set; } = "contenttype";

        public string LessonIdentifier { get; set; } = "L_";
        public string EntityIdentifier { get; set; } = "E_";
        public string SlideIdentifier { get; set; } = "S_";

        public string AutostartIdentifier { get; set; } = "autostart";
        public string AssetsIdentifier { get; set; } = "assets";
        public string UidIdentifier { get; set; } = "uid";
        public string TypeIdentifier { get; set; } = "type";
        public string DocuIdentifier { get; set; } = "Docu";
        public string FileNameIdentifier { get; set; } = "fileName";

        public string ConverterFieldsIdentifier { get; set; } = "fields";
        public Dictionary<string, string> ConverterApplicationMapping { get; set; } = new Dictionary<string, string>()
        {
            { "Adobe Acrobat Document", "PDF" },
            { "Microsoft Word", "Word"},
            {"Microsoft PowerPoint", "PowerPoint" }
        };
        public string ConverterApplicationDefaultMapping { get; set; } = "HTML";

        public int MaxErrorCount { get; set; } = 5;
        public int MaxMinutesRuntime { get; set; } = 30;
    }
}
