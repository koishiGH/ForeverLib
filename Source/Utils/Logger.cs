using System;

namespace ForeverLib.Utils
{
    public class Logger
    {
        private readonly string _source;

        public Logger(string source)
        {
            _source = source;
        }

        public void Log(string message)
        {
            Console.WriteLine($"[{_source}] {message}");
        }

        public void Warn(string message)
        {
            Console.WriteLine($"[{_source}][WARN] {message}");
        }

        public void Error(string message)
        {
            Console.WriteLine($"[{_source}][ERROR] {message}");
        }
    }
} 