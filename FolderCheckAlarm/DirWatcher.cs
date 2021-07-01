using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FolderCheckAlarm
{
    class DirWatcher : IDisposable
    {
        private bool _disposed = false;
        private readonly SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);
        private readonly FileSystemSafeWatcher _myFolderWatcher;
        LogWriter _logWriter;

        public FileSystemSafeWatcher FolderWatcher 
        { 
            get 
            {
                return _myFolderWatcher;
            } 
        }

        public DirWatcher(string path, LogWriter logWriter)
        {
            _logWriter = logWriter;

            _myFolderWatcher = new FileSystemSafeWatcher(path);
            _myFolderWatcher.Created += MyWatcher_Created;
            _myFolderWatcher.Deleted += MyWatcher_Deleted;
            _myFolderWatcher.EnableRaisingEvents = true;
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

        private void MyWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (e == null || sender == null) return;

            _logWriter.WriteLogToFile(DateTime.Now + " - File deleted [no Alert]: " + e.FullPath);
            this.Dispose();
        }

        private void MyWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (e == null || sender == null) return;

            _logWriter.WriteLogToFile(DateTime.Now + " - ==== ALERT START ====");
            _logWriter.WriteLogToFile(DateTime.Now + " - File created: " + e.FullPath);

            var _newMessage = new Message(_logWriter);
            _newMessage.AddMailDetail(e, @"!!! Alarm regarding monitored folder !!! (Env: ");
            _newMessage.SendMessage();

        }
    }
}
