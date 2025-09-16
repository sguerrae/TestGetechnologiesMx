using System;
using System.IO;
namespace Sample
{
    // Customs - Añadí este runner dummy para simular el proceso de actualización
    class Program
    {
        static void Main(string[] args)
        {
            // Creo una carpeta de origen con archivos de ejemplo para la simulación (ubicada en el directorio del ejecutable)
            string baseDir = AppContext.BaseDirectory;
            string sourceFolder = Path.Combine(baseDir, "source_updates");
            if (!Directory.Exists(sourceFolder)) Directory.CreateDirectory(sourceFolder);

            File.WriteAllText(Path.Combine(sourceFolder, "config.ini.upd"), "Configuracion actualizada");
            File.WriteAllText(Path.Combine(sourceFolder, "readme.txt.add"), "Archivo nuevo de lectura");
            File.WriteAllText(Path.Combine(sourceFolder, "appsettings.xml.xmrg"), "<config><setting>valor</setting></config>");
            File.WriteAllText(Path.Combine(sourceFolder, "oldfile.dat.del"), "");

            // Creo la carpeta de destino donde se aplicarán los cambios (ubicada en el directorio del ejecutable)
            string installationFolder = Path.Combine(baseDir, "installation_folder");
            if (!Directory.Exists(installationFolder)) Directory.CreateDirectory(installationFolder);

            // Creo un archivo existente que será actualizado
            File.WriteAllText(Path.Combine(installationFolder, "config.ini"), "Configuracion anterior");

            Console.WriteLine($"Archivos de origen creados en: {sourceFolder}");
            Console.WriteLine($"Carpeta de instalación: {installationFolder}\n");

            Console.WriteLine("Iniciando simulación del proceso de actualización :)...\n");

            MonitorUpdaterManagerSample.UpdateMonitor(sourceFolder, installationFolder, "1.0.0");

            Console.WriteLine($"\nResultado de la actualización: Éxito [Si se pudo!!])");
            Console.WriteLine("\n[Customs] - Simulación terminada. ");
            Console.WriteLine("\nPresiona cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}
