using System.Collections.Generic;

namespace Enable_Now_Konnektor.src.jobs
{
    public class JobConfig
    {
        public string Id { get; set; }
        public string StartId { get; set; }

        public bool AutostartMetaMapping { get; set; } = true;
        public bool AutostartChildOverwrite { get; set; } = false;
        public string[] AutoStartMappingBlacklist { get; set; }

        public bool AttachementUrlOverwrite { get; set; }

        public string IndexerType { get; set; } = "Json";

        public bool IndexAttachements { get; set; } = true;
        public bool IndexSlides { get; set; } = true;
        public bool IndexGroups { get; set; } = true;
        public bool IndexProjects { get; set; } = true;

        public string PublicationSource { get; set; } = "Website";

        public string EntityPath { get; set; }
        public string EntityUrl { get; set; }
        public string ContentPath { get; set; }
        public string ContentUrl { get; set; }
        public string DemoUrl { get; set; }

        public bool EmailSend { get; set; } = false;
        public string EmailRecipient { get; set; }
        public string EmailSender { get; set; }
        public string EmailSubject { get; set; }
        public string EmailSmtpServer { get; set; }
        public int EmailPort { get; set; }

        public Dictionary<string, string> BlacklistFields { get; set; }
        public string[] MustHaveFields { get; set; }

        public Dictionary<string, string[]> GlobalMappings { get; set; }
        public Dictionary<string, string[]> ProjectMappings { get; set; }
        public Dictionary<string, string[]> SlideMappings { get; set; }
        public Dictionary<string, string[]> GroupMappings { get; set; }

        public int ThreadCount { get; set; } = 2;

        public int MaxErrorCount { get; set; } = 5;


    }
}
