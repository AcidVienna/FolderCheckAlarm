using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.ServiceProcess;

namespace FolderCheckAlarm
{
    public partial class Service1 : ServiceBase
    {
        private readonly string[] folders = ConfigurationManager.AppSettings["MonitoringFolder"].Split(';');
        private readonly List<FileSystemWatcher> _myWatchers = new List<FileSystemWatcher>();
                
        private LogWriter _myLogWriter = new LogWriter();

        public Service1()
        {
            InitializeComponent();

            foreach (var folder in folders)
            {
                if (folder.Length == 0) continue;
                _myWatchers.Add(new FileSystemWatcher(folder));
            }
        }
        
        protected override void OnStart(string[] args)
        {
            _myLogWriter.WriteLogToFile(DateTime.Now + " - Service is started...");
            _myLogWriter.WriteLogToFile(DateTime.Now + " - Monitored Environment: " + ConfigurationManager.AppSettings["MonitoringEnv"]);

            foreach (var watcher in _myWatchers)
            {
                _myLogWriter.WriteLogToFile(DateTime.Now + " - Monitored Folder: " + watcher.Path);
                watcher.Created += MyWatcher_Created;
                watcher.Deleted += MyWatcher_Deleted;
                watcher.EnableRaisingEvents = true;
            }
            
            _myLogWriter.WriteLogToFile(DateTime.Now + " - Email alerting target(s): " + ConfigurationManager.AppSettings["Mail_InfoTo"]);
            _myLogWriter.WriteLogToFile(DateTime.Now + " - Email sender: " + ConfigurationManager.AppSettings["Mail_InfoFrom"]);

        }
        
        protected override void OnStop()
        {
            _myLogWriter.WriteLogToFile(DateTime.Now + " - Service is stopped!");
        }
        
        private void MyWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (e == null || sender == null) return;

            _myLogWriter.WriteLogToFile(DateTime.Now + " - File deleted (no action): " + e.FullPath);
         
        }

        private void MyWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (e == null || sender == null) return;

            _myLogWriter.WriteLogToFile(DateTime.Now + " - ==== ALERT START ====");
            _myLogWriter.WriteLogToFile(DateTime.Now + " - File created: " + e.FullPath);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(ConfigurationManager.AppSettings["Mail_InfoFrom"], ConfigurationManager.AppSettings["Mail_InfoFrom"]));

            var targets = ConfigurationManager.AppSettings["Mail_InfoTo"].Split(';');

            foreach (var target in targets)
            {
                if (target.Length == 0) continue;
                message.To.Add(new MailboxAddress("", target));
                _myLogWriter.WriteLogToFile(DateTime.Now + " - Email alert released to: " + target);
            }

            message.Subject = @"!!! Alarm regarding monitored folder !!! (Env: " + ConfigurationManager.AppSettings["MonitoringEnv"] + ")";

            message.Body = new TextPart("plain")
            {
                Text = @"File created: " + e.FullPath
            };

            try
            {
                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, x) => true;
                    client.Connect(ConfigurationManager.AppSettings["Mail_SMTP"],
                        Convert.ToInt32(ConfigurationManager.AppSettings["Mail_Port"]), false);

                    if(ConfigurationManager.AppSettings["Mail_User"].Length > 0)
                    {
                            client.Authenticate(ConfigurationManager.AppSettings["Mail_User"],
                                ConfigurationManager.AppSettings["Mail_Password"]);
                    }
                    client.Send(message);
                    client.Disconnect(true);
                }

                _myLogWriter.WriteLogToFile(DateTime.Now + " - ==== ALERT END ====");
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
