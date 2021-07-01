using System;
using System.Configuration;
using System.ServiceProcess;

namespace FolderCheckAlarm
{
    public partial class Service1 : ServiceBase
    {
        private readonly string[] _folders = ConfigurationManager.AppSettings["MonitoringFolder"].Split(';');
        private readonly string[] _fileFolders = ConfigurationManager.AppSettings["FileMonitoringFolder"].Split(';');

        LogWriter _myLogWriter = new LogWriter();

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _myLogWriter.WriteLogToFile(DateTime.Now + " - FolderCheckAlarm Windows Service started!");
            _myLogWriter.WriteLogToFile(DateTime.Now + " - Monitored Environment: " + ConfigurationManager.AppSettings["MonitoringEnv"]);

            foreach (var folder in _folders)
            {
                if (folder.Length == 0) continue;
                var tempWatcher = new DirWatcher(folder, _myLogWriter);
                _myLogWriter.WriteLogToFile(DateTime.Now + " - Monitored Folder: " + folder);
            }

            foreach (var filefolder in _fileFolders)
            {
                if (filefolder.Length == 0) continue;
                var tempObject = new FileMonitoringObject(filefolder, _myLogWriter, ConfigurationManager.AppSettings["FileMonitoringPattern"]);
                _myLogWriter.WriteLogToFile(DateTime.Now + " - Filemonitoring in folder: " + filefolder);
            }

            _myLogWriter.WriteLogToFile(DateTime.Now + " - Email alerting target(s): " + ConfigurationManager.AppSettings["Mail_InfoTo"]);
            _myLogWriter.WriteLogToFile(DateTime.Now + " - Email sender: " + ConfigurationManager.AppSettings["Mail_InfoFrom"]);

        }

        protected override void OnStop()
        {
            _myLogWriter.WriteLogToFile(DateTime.Now + " - FolderCheckAlarm Windows Service stopped!");
        }
    }
}

