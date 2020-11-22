using Enable_Now_Konnektor.src.jobs;
using log4net;
using System;
using System.Net.Mail;

namespace Enable_Now_Konnektor.src.misc
{
    internal class MailClient
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(MailClient));
        private readonly JobConfig _jobConfig;
        private readonly SmtpClient _smtpClient;

        internal MailClient(JobConfig jobConfig)
        {
            _jobConfig = jobConfig;
            _smtpClient = new SmtpClient(_jobConfig.EmailSmtpServer, _jobConfig.EmailPort);
        }

        internal void SendMail(string additionalText)
        {
            if( !_jobConfig.EmailSend) { return; }
            try
            {
                _smtpClient.SendAsync(
                    _jobConfig.EmailSender,
                    _jobConfig.EmailRecipient,
                    _jobConfig.EmailSubject,
                    additionalText,
                    _jobConfig.Id);
            }
            catch (Exception e)
            {
                _log.Error( Util.GetFormattedResource("MailClientMessage01"),e );
            }
        }
    }
}
