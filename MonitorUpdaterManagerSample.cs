


namespace Sample
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using log4net;

    public static class MonitorUpdaterManagerSample
    {
        #region Constantes


        private const string MonitorServiceNameKey = "monitorsk";
        public const string UpdaterMonitorInstallationFolder = "monSelfUpdater";
    // Customs: usar ruta relativa dentro del directorio del ejecutable
    private static readonly string MonitorUpdatesPath = System.IO.Path.Combine(AppContext.BaseDirectory, "monitor_updates");
        public const string UpdaterMonitorFolder = "actualizaciones";
    public static string InstalledRollbackFilesPath = MonitorUpdatesPath;
        
        private static readonly ILog Log;

        // Customs
        // Inicializo el logger para el manager del monitor (simulación)
        static MonitorUpdaterManagerSample()
        {
            Log = new log4net.SimpleLogger(typeof(MonitorUpdaterManagerSample).FullName);
        }

        #endregion

       

        #region Metodos
        
        public static void UpdateMonitor(string monitorFilesLocation, string installationFolder, string version)
        {
            try
            {
                var winServiceManager = new WindowsServiceManager();
                Log.Info("====================  [INICIO]  ========================");
                Log.Info("[Simulación] Iniciando las actualizaciones al monitor...");
                
                try
                {
                    Process[] processes = Process.GetProcessesByName("psample");

                    if (processes.Any())
                    {
                        Log.Info("Cerrando el monitor de actualizaciones...");
                        foreach (var proc in processes)
                        {
                            proc.Kill();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("[Importante] Ocurrió un error al intentar terminar el proceso del monitor de actualizaciones.", ex);
                }

           
                var backupPath = System.IO.Path.Combine(MonitorUpdatesPath, "Backup", Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6));
                if (!System.IO.Directory.Exists(backupPath))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(backupPath);
                    }
                    catch
                    {
                        backupPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), UpdaterMonitorFolder, "Backup", Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6));
                        if (!System.IO.Directory.Exists(backupPath))
                        {
                            System.IO.Directory.CreateDirectory(backupPath);
                        }
                    }
                }

                var fileManager = new FileManager();
                
                var result = fileManager.UpdateFiles(monitorFilesLocation.Trim(new char[] { '"' }), installationFolder.Trim(new char[] { '"' }), backupPath);
                bool updateError = false;

                if (!string.IsNullOrEmpty(result))
                {
                    updateError = true;
                    Log.Error(result);

                    result = null;
                }

          
                if (updateError)
                {
                    Log.Info("Realizando rollback de las actualizaciones al monitor...");
                    
                    result = fileManager.UpdateFiles(backupPath, installationFolder);
                    fileManager.RemoveDirectoryContents(backupPath);

                    if (!string.IsNullOrEmpty(result))
                    {
                        Log.Info("MonitorUpdater - " + result);
                    }
                    else
                    {
                        Log.Info("Terminado rollback de las actualizaciones al monitor...");
                    }

              

                    return;
                }

                fileManager.RemoveDirectoryContents(backupPath);
                fileManager.RemoveDirectoryContents(monitorFilesLocation.Trim(new char[] { '"' }));
                System.IO.Directory.Delete(backupPath, true);
                System.IO.Directory.Delete(monitorFilesLocation.Trim(new char[] { '"' }), true);
                
                ReleaseUpdateMonitorTask();

                Log.Info("====================  [FIN]  ========================");
                Log.Info("Actualizaciones al monitor terminadas...");
            }
            catch (Exception ex)
            {
                Log.Error("Ocurrió un error durante el proceso de actualización del Monitor.", ex);
            }
        }
        
        private static void ReleaseUpdateMonitorTask()
        {
           // elimina la tarea
        }

       #endregion
    }
}
