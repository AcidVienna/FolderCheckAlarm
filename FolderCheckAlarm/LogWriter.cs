using System;
using System.Configuration;
using System.IO;

namespace FolderCheckAlarm
{
    class LogWriter
    {
        public void WriteLogToFile(string Message)
        {
            var path = ConfigurationManager.AppSettings["LogFolder"];
            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var filepath = path + "\\" + DateTime.Now.ToString("yyyy") +
                                        "-" + DateTime.Now.ToString("MM") +
                                        "-" + DateTime.Now.ToString("dd") + "_FolderCheckAlarm.log";

            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (var sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (var sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}
