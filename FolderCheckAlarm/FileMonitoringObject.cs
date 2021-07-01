using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FolderCheckAlarm
{
    class FileMonitoringObject : IDisposable
    {
        private bool _disposed = false;
        private readonly SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);
        private FileSystemSafeWatcher _fileSystemWatcher;
        private FileMonitor _fileMonitor;
        LogWriter _logWriter;
        private readonly string _observationPattern;

        public FileMonitoringObject(string folder, LogWriter logWriter, string observationPattern)
        {
            _logWriter = logWriter;
            _fileSystemWatcher = new FileSystemSafeWatcher(folder);
            _observationPattern = observationPattern;

            _fileSystemWatcher.Changed += MyWatcher_Changed;
            _fileSystemWatcher.Created += MyWatcher_Created;
            _fileSystemWatcher.Deleted += MyWatcher_Deleted;
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        public FileSystemSafeWatcher FileSystemWatcher
        {
            get { return _fileSystemWatcher; }
        }

        private void MyWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e == null || sender == null) return;
            _fileMonitor.Monitor(e);
        }

        private void MyWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (e == null || sender == null) return;

            _logWriter.WriteLogToFile(DateTime.Now +
                                      " - File created/monitored: " +
                                      e.FullPath +
                                      " - observation pattern: " +
                                      _observationPattern);

            _fileMonitor = new FileMonitor(e.FullPath, _observationPattern, _logWriter);
            _fileMonitor.Monitor(e);
            
        }

        private void MyWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (e == null || sender == null) return;

            try
            {
                _logWriter.WriteLogToFile(DateTime.Now + " - File(Monitoring) deleted (for) [no Alert]: " + e.FullPath);
                if(_fileMonitor != null) _fileMonitor.Dispose();
                if(this != null) this.Dispose();
            }
            catch(Exception ex)
            {
                _logWriter.WriteLogToFile(ex.Message);
            }
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
    }
}
