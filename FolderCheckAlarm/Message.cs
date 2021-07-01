using MimeKit;
using MailKit.Net.Smtp;
using System;
using System.Configuration;
using System.IO;

namespace FolderCheckAlarm
{
    class Message : MimeMessage
    {
        
        private static LogWriter _myLogWriter;

        public Message(LogWriter myLogWriter)
        {
            _myLogWriter = myLogWriter;
        }

        public void AddMailDetail(FileSystemEventArgs e, string subject)
        {
            try
            {
                this.From.Add(new MailboxAddress(ConfigurationManager.AppSettings["Mail_InfoFrom"], ConfigurationManager.AppSettings["Mail_InfoFrom"]));

                var targets = ConfigurationManager.AppSettings["Mail_InfoTo"].Split(';');

                foreach (var target in targets)
                {
                    if (target.Length == 0) continue;
                    this.To.Add(new MailboxAddress("", target));
                    _myLogWriter.WriteLogToFile(DateTime.Now + " - Email alert released to: " + target);
                }

                this.Subject = subject + ConfigurationManager.AppSettings["MonitoringEnv"] + ")";

                this.Body = new TextPart("plain")
                {
                    Text = @"File in question: " + e.FullPath
                };
            }
            catch(Exception ex)
            {
                _myLogWriter.WriteLogToFile(DateTime.Now + " - " + ex.Message);
            }
        }

        public void SendMessage()
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, x) => true;
                    client.Connect(ConfigurationManager.AppSettings["Mail_SMTP"],
                        Convert.ToInt32(ConfigurationManager.AppSettings["Mail_Port"]), false);

                    if (ConfigurationManager.AppSettings["Mail_User"].Length > 0)
                    {
                        client.Authenticate(ConfigurationManager.AppSettings["Mail_User"],
                            ConfigurationManager.AppSettings["Mail_Password"]);
                    }
                    client.Send(this);
                    client.Disconnect(true);
                }

                _myLogWriter.WriteLogToFile(DateTime.Now + " - ===== ALERT END =====");
            }
            catch (Exception exception)
            {
                _myLogWriter.WriteLogToFile(DateTime.Now + " -  " + exception.Message);
                if (exception.InnerException != null)
                    _myLogWriter.WriteLogToFile(DateTime.Now + " -  " + exception.InnerException.Message);
                _myLogWriter.WriteLogToFile(DateTime.Now + " - ==== ALERT BROKEN ====");
            }
        }
    }
}
