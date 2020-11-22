using Enable_Now_Konnektor.src.jobs;
using Enable_Now_Konnektor.src.statistic;
using log4net;
using System;
using System.Net.Mail;

namespace Enable_Now_Konnektor.src.misc
{
    internal class MailClient
    {
        private readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly JobConfig jobConfig;

        internal MailClient(JobConfig jobConfig)
        {
            this.jobConfig = jobConfig;
        }

        internal void SendMail()
        {
            SmtpClient smtpClient = new SmtpClient(jobConfig.EmailSmtpServer, jobConfig.EmailPort);
            if (!jobConfig.EmailSend) { return; }
            StatisticService service = StatisticService.GetService(jobConfig.Id);
            string text = Util.GetFormattedResource("MailClientMessage01",
                jobConfig.Id,
                DateTime.Now,
                service.FoundDocumentsCount,
                service.IndexedDocumentsCount,
                service.RemovedDocumentsCount);
            try
            {
                smtpClient.SendAsync(
                    jobConfig.EmailSender,
                    jobConfig.EmailRecipient,
                    jobConfig.EmailSubject,
                    text,
                    jobConfig.Id);
            }
            catch (Exception e)
            {
                log.Error(Util.GetFormattedResource("MailClientMessage02"), e);
            }
        }
    }
}
