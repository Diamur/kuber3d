// mvp/Core/Log.cs
//
// Простое файловое логирование для диагностики рендера.

using System;
using System.Globalization;
using System.IO;

namespace kuber3d.Core
{
    public static class Log
    {
        private static readonly object Sync = new();
        private static readonly string LogDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Kuber3D");
        private static readonly string LogPath =
            Path.Combine(LogDirectory, "kuber3d-render.log");

        public static string CurrentLogPath => LogPath;

        public static void Info(string message)
        {
            Write("INFO", message);
        }

        public static void Error(string message)
        {
            Write("ERROR", message);
        }

        private static void Write(string level, string message)
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);
                var timestamp = DateTime.Now.ToString("O", CultureInfo.InvariantCulture);
                var line = $"{timestamp} [{level}] {message}{Environment.NewLine}";
                lock (Sync)
                {
                    File.AppendAllText(LogPath, line);
                }
            }
            catch
            {
                // Логирование не должно падать.
            }
        }
    }
}
