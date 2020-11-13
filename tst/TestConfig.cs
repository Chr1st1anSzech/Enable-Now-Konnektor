using Enable_Now_Konnektor.src.config;
using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.jobs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Enable_Now_Konnektor.tst
{
    public class TestConfig
    {
        public static void CreateSettings()
        {

            Config config = new Config();
            config.ConverterUrl = "http://v970w134.now-it.drv:9605/json/index?method=delete&param0=";
            config.FetchUrl = "http://v970w134.now-it.drv:9605/json/index?method=delete&param0=";
            config.IndexUrl = "http://v970w134.now-it.drv:9605/json/index?method=delete&param0=";
            config.RemoveUrl = "http://v970w134.now-it.drv:9605/json/index?method=delete&param0=";

            Dictionary<string, string[]> global = new Dictionary<string, string[]>(){
                { "str.bla", ( new string [] { "hahaha", "hohoho" } ) }};

            Dictionary<string, string[]> project = new Dictionary<string, string[]>(){
                { "str.aaa", ( new string [] { "rerg", "mghrthrt" } ) }};

            Dictionary<string, string[]> slide = new Dictionary<string, string[]>(){
                { "str.ccc", ( new string [] { "hahgggaha", "hhhh" } ) }};

            Dictionary<string, string[]> group = new Dictionary<string, string[]>(){
                { "str.ddd", ( new string [] { "fff", "hohoho" } ) }};

            Dictionary<string, string> indexingBlacklist = new Dictionary<string, string>(){
                { "str.ddd", "fff"} };

            JobConfig js = new JobConfig()
            {
                AttachementUrlOverwrite = true,
                AutostartMetaMapping = true,
                AutoStartMappingBlacklist = new string[] { "aaaa", "bbb" },
                AutostartChildOverwrite = true,
                ContentUrl = "https://gezeigt-wie.now-it.drv/index.html#${Class}!${Id}",
                DemoUrl = "https://gezeigt-wie.now-it.drv/index.html?show=project!${Id}:demo",
                EmailPort = 12,
                EmailRecipient = "c@c.de",
                EmailSend = true,
                EmailSender = "b@b.de",
                EmailSmtpServer = "hhttt.now-it.drv",
                EmailSubject = "aaaaaa",
                EntityUrl = "https://gezeigt-wie.now-it.drv/${Class}/${Id}/${File}",
                Id = "hahahaha",
                IndexAttachements = true,
                IndexGroups = false,
                IndexProjects = false,
                IndexSlides = true,
                StartId = "GR_389F860B088563B1",
                MustHaveFields = new string[] { "vvvvvv", "ddddd" },
                BlacklistFields = indexingBlacklist,
                GlobalMappings = global,
                ProjectMappings = project,
                SlideMappings = slide,
                GroupMappings = group,
                ThreadCount = 5
            };

            ConfigWriter writer = new ConfigWriter();
            writer.SaveConfig(config);
        }

        public static void ReadSettings()
        {
            var settings = ConfigReader.LoadConnectorConfig();
        }
    }
}
