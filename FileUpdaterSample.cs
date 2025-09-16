
using log4net;

namespace Sample
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.Web.XmlTransform;


    public class FileUpdater
    {
        private static readonly ILog Log;

        static FileUpdater()
        {
            Log = new log4net.SimpleLogger(typeof(FileUpdater).FullName);
        }
        #region Constants


        private const string DeleteCommandExtension = ".del";


        private const string AddCommandExtension = ".add";

      
        private const string UpdateCommandExtension = ".upd";

        private const string XdtMergeCommandExtension = ".xmrg";


        private const string ExecuteCommandExtension = ".exc";

        private const string ExecuteCommandExtensionInitial = ".eini";

        private const string ExecuteCommandExtensionEnd = ".eend";

        private const string ExecuteCommandParamsExtension = ".params";

        private const string CannotTransformXdtMessage = "No se puede realizar la transformación del archivo .";

        #endregion

    
        #region Methods

       
        private static void BackupFile(string? backupDir, string targetFolder, string originalFileName, string command)
        {
            var backupFileName = originalFileName;
            var targetRelativeDirectory = string.Empty;

           
            if (File.Exists(originalFileName))
            {
                if (command == DeleteCommandExtension)
                {
                    command = UpdateCommandExtension;
                    targetRelativeDirectory = Path.GetDirectoryName(GetRelativePath(originalFileName, targetFolder)) ?? string.Empty;
                }
                
                if (command == ExecuteCommandExtensionInitial || command == ExecuteCommandExtension || command == ExecuteCommandExtensionEnd || command == ExecuteCommandParamsExtension)
                {
                    var originalDir3 = Path.GetDirectoryName(originalFileName) ?? string.Empty;
                    backupFileName = Path.Combine(originalDir3, Path.GetFileNameWithoutExtension(originalFileName));
                    targetRelativeDirectory = targetFolder;
                }
            }
            else if (command == DeleteCommandExtension)
            {
                var originalDir = Path.GetDirectoryName(originalFileName);
                if (!string.IsNullOrEmpty(originalDir) && !Directory.Exists(originalDir))
                {
                    Directory.CreateDirectory(originalDir);
                }

                File.WriteAllText(originalFileName, string.Empty);

                targetRelativeDirectory = Path.GetDirectoryName(GetRelativePath(originalFileName, targetFolder)) ?? string.Empty;
            }
            else if (command == UpdateCommandExtension)
            {

                command = DeleteCommandExtension;
                var originalDir2 = Path.GetDirectoryName(originalFileName);
                if (!string.IsNullOrEmpty(originalDir2) && !Directory.Exists(originalDir2))
                {
                    Directory.CreateDirectory(originalDir2);
                }

                File.WriteAllText(originalFileName, string.Empty);

                targetRelativeDirectory = Path.GetDirectoryName(GetRelativePath(originalFileName, targetFolder)) ?? string.Empty;
            }
            else
            {
                return;
            }

        
            var folderToBackupFile = Path.Combine(backupDir ?? string.Empty, targetRelativeDirectory ?? string.Empty);
            if (string.IsNullOrEmpty(folderToBackupFile)) folderToBackupFile = Path.GetTempPath();
            if (!Directory.Exists(folderToBackupFile))
            {
                Directory.CreateDirectory(folderToBackupFile);
            }
            
            File.Copy(originalFileName, Path.Combine(folderToBackupFile, Path.GetFileName(backupFileName)) + command, true);
        }


        private static string GetRelativePath(string filespec, string folder)
        {
            if (string.IsNullOrEmpty(filespec)) return string.Empty;
            if (string.IsNullOrEmpty(folder)) return filespec;

            Uri pathUri = new Uri(filespec);

            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }

            Uri folderUri = new Uri(folder);

            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

    
        public string UpdateFiles(string? sourceFolder, string? targetFolder, string? backupDir = null)
        {
            if (string.IsNullOrEmpty(sourceFolder) || string.IsNullOrEmpty(targetFolder))
            {
                return "Source or target folder is null or empty.";
            }

            var createBackup = !string.IsNullOrEmpty(backupDir);

            if (createBackup)
            {
                if (!string.IsNullOrEmpty(backupDir) && !Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }
            }

            try
            {
                Directory.EnumerateFiles(sourceFolder, "*" + ExecuteCommandExtensionInitial, SearchOption.AllDirectories).ToList().ForEach(file =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var targetRelativeDirectory = Path.GetDirectoryName(GetRelativePath(file, sourceFolder));
                    this.ExecuteBats(file, fileName, targetRelativeDirectory, createBackup, Path.Combine(backupDir ?? string.Empty, targetRelativeDirectory ?? string.Empty), ExecuteCommandExtensionInitial);
                });
                
                foreach (var file in Directory.EnumerateFiles(sourceFolder, "*.*", SearchOption.AllDirectories).Where(s => Path.GetExtension(s) != ExecuteCommandExtensionInitial && Path.GetExtension(s) != ExecuteCommandExtensionEnd).ToList())
                {
                    var fileExtension = Path.GetExtension(file);
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var targetRelativeDirectory = Path.GetDirectoryName(GetRelativePath(file, sourceFolder));
                    var targetFileName = Path.Combine(targetFolder, targetRelativeDirectory ?? string.Empty, fileName ?? string.Empty);

                    switch (fileExtension.ToLower())
                    {
                        case AddCommandExtension:
                            if (createBackup)
                            {
                                BackupFile(Path.Combine(backupDir ?? string.Empty, targetRelativeDirectory ?? string.Empty), targetFolder, targetFileName, DeleteCommandExtension);
                            }

                            this.CopyFile(file, targetFileName);
                            break;
                        case XdtMergeCommandExtension:
                            if (createBackup && File.Exists(targetFileName))
                            {
                                BackupFile(Path.Combine(backupDir ?? string.Empty, targetRelativeDirectory ?? string.Empty), targetFolder, targetFileName, UpdateCommandExtension);
                            }

                            this.MergeXDT(file, targetFileName);
                            break;
                        case UpdateCommandExtension:
                            if (createBackup)
                            {
                                BackupFile(Path.Combine(backupDir ?? string.Empty, targetRelativeDirectory ?? string.Empty), targetFolder, targetFileName, UpdateCommandExtension);
                            }

                            this.CopyFile(file, targetFileName);
                            break;
                        case DeleteCommandExtension:
                            if (createBackup)
                            {
                                BackupFile(Path.Combine(backupDir ?? string.Empty, targetRelativeDirectory ?? string.Empty), targetFolder, targetFileName, AddCommandExtension);
                            }

                            this.RemoveFile(targetFileName);
                            break;
                        case ExecuteCommandExtension:
                            this.ExecuteBats(file, fileName, targetRelativeDirectory, createBackup, Path.Combine(backupDir ?? string.Empty, targetRelativeDirectory ?? string.Empty), ExecuteCommandExtension);
                            break;
                    }
                }

                Directory.EnumerateFiles(sourceFolder, "*" + ExecuteCommandExtensionEnd, SearchOption.AllDirectories).ToList().ForEach(file =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var targetRelativeDirectory = Path.GetDirectoryName(GetRelativePath(file, sourceFolder));
                    this.ExecuteBats(file, fileName, targetRelativeDirectory, createBackup, Path.Combine(backupDir ?? string.Empty, targetRelativeDirectory ?? string.Empty), ExecuteCommandExtensionEnd);
                });
            }
            catch (Exception ex)
            {
                if (createBackup)
                {
                    var errorMessage = ex.ToString();

                  
                    var rollbackError = this.UpdateFiles(backupDir, targetFolder);
                    
                    return errorMessage + (string.IsNullOrEmpty(rollbackError) ? string.Empty : Environment.NewLine + "Rollback Error =>" + Environment.NewLine + rollbackError);
                }

                return ex.ToString();
            }

            return string.Empty;
        }
        
    // Customs
    // Implementé un ejecutor de prueba para scripts que sólo registra la ejecución en la simulación
    private void ExecuteBats(string file, string? fileName, string? targetRelativeDirectory, bool createBackup, string? backupDir, string extension)
        {
            try
            {
                Log.Info($"Ejecutando script: {file} (ext {extension})");
            }
            catch (Exception ex)
            {
                Log.Error("Error ejecutando script", ex);
                throw;
            }
        }


    // Customs
    // Implementé una copia simplificada de archivos para la simulación
    private void CopyFile(string sourceFile, string targetFile)
        {
           try
           {
               var targetDir = Path.GetDirectoryName(targetFile);
               if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
               File.Copy(sourceFile, targetFile + Path.GetExtension(sourceFile), true);
               Log.Info($"Copiado {sourceFile} -> {targetFile}");
           }
           catch (Exception ex)
           {
               Log.Error("Error copiando archivo", ex);
               throw;
           }
        }

    // Customs
    // Implementé la eliminación simplificada de archivos para la simulación
    private void RemoveFile(string targetFile)
        {
          try
          {
              if (File.Exists(targetFile))
              {
                  File.Delete(targetFile);
                  Log.Info($"Eliminado {targetFile}");
              }
          }
          catch (Exception ex)
          {
              Log.Error("Error eliminando archivo", ex);
              throw;
          }
        }


        private void MergeXDT(string sourceFile, string targetFile)
        {
            if (File.Exists(targetFile))
            {
                using (var target = new XmlTransformableDocument())
                {
                    target.PreserveWhitespace = true;
                    target.Load(targetFile);

                    using (var xdt = new XmlTransformation(sourceFile))
                    {
                        if (xdt.Apply(target))
                        {
                            target.Save(targetFile);
                        }
                        else
                        {
                            throw new XmlTransformationException(string.Format(CannotTransformXdtMessage, sourceFile));
                        }
                    }
                }
            }
        }

        #endregion
    }
}