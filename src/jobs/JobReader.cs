using Enable_Now_Konnektor.src.misc;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Enable_Now_Konnektor.src.jobs
{
    internal class JobReader
    {
        internal static string JobDirectory = Path.Combine(Util.GetApplicationRoot(), "jobs");

        private readonly ILog _log = LogManager.GetLogger(typeof(JobReader));

        internal List<JobConfig> ReadAllJobConfigs()
        {
            IEnumerable<string> fullFileNames;
            try
            {
                fullFileNames = Directory.EnumerateFiles(JobDirectory);
            }
            catch (Exception e)
            {
                _log.Fatal( Util.GetFormattedResource("JobReaderMessage01", JobDirectory), e );
                return null;
            }
            int jobCount = fullFileNames.Count();
            List<JobConfig> jobsConfigs = new List<JobConfig>();
            Validator jobValidator = new Validator();
            for (int i = 0; i < jobCount; i++)
            {

                string jsonString = ReadFile(fullFileNames.ElementAt(i));
                if( jsonString == null ) { continue; }

                JobConfig jobConfig;
                try
                {
                    jobConfig = JsonConvert.DeserializeObject<JobConfig>(jsonString);
                }
                catch (Exception e)
                {
                    _log.Error( Util.GetFormattedResource("JobReaderMessage02", i), e );
                    continue;
                }
                

                if (jobValidator.ValidateJobConfig(jobConfig))
                {
                    jobsConfigs.Add(jobConfig);
                }
                else
                {
                    _log.Error( Util.GetFormattedResource("JobReaderMessage03", i));
                }
            }
            return jobsConfigs;
        }

        private string ReadFile(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                _log.Error( Util.GetFormattedResource("JobReaderMessage04", filePath), e );
                return null;
            }
        }
    }
}
