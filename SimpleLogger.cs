using System;
namespace log4net
{
    public interface ILog
    {
        void Info(string message);
        void Info(string message, Exception ex);
        void Error(string message);
        void Error(string message, Exception ex);
    }

    // Customs
    // Añadí este logger simple para sustituir temporalmente a log4net en la simulación
    public class SimpleLogger : ILog
    {
        private readonly string name;
        public SimpleLogger(string? name)
        {
            this.name = name ?? "<null>";
        }

        public SimpleLogger(Type type) : this(type?.FullName) { }

        public void Info(string message) => Console.WriteLine($"[INFO] [{name}] {message}");
        public void Info(string message, Exception ex) => Console.WriteLine($"[INFO] [{name}] {message} - {ex}");
        public void Error(string message) => Console.WriteLine($"[ERROR] [{name}] {message}");
        public void Error(string message, Exception ex) => Console.WriteLine($"[ERROR] [{name}] {message} - {ex}");
    }
}
