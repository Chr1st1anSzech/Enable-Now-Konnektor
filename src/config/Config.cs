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

        public string StringIdentifier = "_str";
        public string FacetIdentifier = "_facet";
        public string LongIdentifier = "_long";

        public string UidFieldName { get; } = "uid";
        public string ClassFieldName { get; } = "class";
        public string UrlFieldName { get; } = "url";
        public string DateFieldName { get; } = "datelastmodified";
        public string HasFieldName { get; } = "url";
        public string EsIdFieldName { get; } = "esid";
        public string BodyFieldName { get; } = "body";
        public string MimeTypeFieldName { get; } = "mimetype";
        public string ApplicationFieldName { get; } = "application";
        public string ContentTypeFieldNAme { get; } = "contenttype";


        public char LessonIdentifier { get; } = '%';
        public char EntityIdentifier { get; } = '$';
        public char SlideIdentifier { get; } = '&';

        public string AutostartIdentifier { get; } = "autostart";
        public string AssetsIdentifier { get; } = "assets";
        public string UidIdentifier { get; } = "uid";
        public string TypeIdentifier { get; } = "type";
        public string DocuIdentifier { get; } = "Docu";
        public string FileNameIdentifier { get; } = "fileName";
        public string ConverterFieldsIdentifier { get; } = "fields";

        public Dictionary<string, string> ConverterApplicationMapping { get; } = new Dictionary<string, string>()
        {
            { "Adobe Acrobat Document", "PDF" },
            { "Microsoft Word", "Word"},
            {"Microsoft PowerPoint", "PowerPoint" }
        };
        public string ConverterApplicationDefaultMapping { get; } = "HTML";
    }
}
