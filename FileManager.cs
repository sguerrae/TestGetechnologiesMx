using System;
using log4net;

namespace Sample
{
    // Customs
    // Añadí este wrapper para reutilizar la implementación de FileManagerSample en la simulación
    public class FileManager
    {
        private static readonly ILog Log;
        private readonly FileManagerSample inner = new FileManagerSample();

        static FileManager()
        {
            Log = new log4net.SimpleLogger(typeof(FileManager).FullName);
        }

        public string UpdateFiles(string sourceFolder, string targetFolder, string? backupDir = null)
        {
            return inner.UpdateFiles(sourceFolder, targetFolder, backupDir);
        }

        public string RemoveDirectoryContents(string directory, string directoryToFilter = "", bool remove = false)
        {
            return inner.RemoveDirectoryContents(directory, directoryToFilter, remove);
        }
    }
}
