using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace FolderCheckAlarm
{
    class FileMonitor : IDisposable
    {
        private bool _disposed = false;
        private readonly SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);
        private static string _filename;
        private static string _eventPattern;
        private static LogWriter _LogWriter;
        private long _lastOffset = 0;

        public long Offset
        {
            get { return _lastOffset; }
        }

        public string Filename
        {
            get { return _filename; }
        }

        public FileMonitor(string filename, string eventPattern, LogWriter LogWriter)
        {
            _filename = filename;
            _eventPattern = eventPattern;
            _LogWriter = LogWriter;
        }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed state (managed objects).
                _safeHandle?.Dispose();
            }

            _disposed = true;
        }

        public void Monitor(FileSystemEventArgs e)
        {
            if (e.FullPath == null) return;

            try
            {
                using (FileStream fs = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fs.Seek(_lastOffset, SeekOrigin.Begin);
                    var myReader = new StreamReader(fs);
                    var AdditionalText = myReader.ReadToEnd();

                    Console.WriteLine(AdditionalText);

                    if (AdditionalText.Contains(_eventPattern))
                    {
                        _LogWriter.WriteLogToFile(DateTime.Now + " - ==== ALERT START ====");
                        _LogWriter.WriteLogToFile(DateTime.Now + " - PatternMatch in monitored file: " + _filename);

                        var _newMessage = new Message(_LogWriter);
                        _newMessage.AddMailDetail(e, @"!!! Alarm regarding monitored file !!! (Env: ");
                        _newMessage.SendMessage();

                    }

                    _lastOffset = fs.Length;
                 }
            }
            catch(Exception ex)
            {
                _LogWriter.WriteLogToFile(ex.Message);
            }
        }
    }
}