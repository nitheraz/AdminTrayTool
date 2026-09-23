namespace AdminTrayTool
{
    static class Logger
    {
        private static readonly string LogPath;

        static Logger()
        {
            try
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AdminTrayTool");
                Directory.CreateDirectory(dir);
                LogPath = Path.Combine(dir, "admintraytool.log");
            }
            catch
            {
                LogPath = Path.Combine(Path.GetTempPath(), "admintraytool.log");
            }
        }

        public static void LogInfo(string msg)
        {
            try { File.AppendAllText(LogPath, $"[INFO] {DateTime.Now:O} {msg}\r\n"); } catch { }
        }

        public static void LogError(string msg)
        {
            try { File.AppendAllText(LogPath, $"[ERR ] {DateTime.Now:O} {msg}\r\n"); } catch { }
        }
    }
}
