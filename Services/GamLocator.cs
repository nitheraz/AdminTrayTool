using System.Diagnostics;

namespace AdminTrayTool.Services
{
    public static class GamLocator
    {
        // Embedded GAM7 folder inside AdminTrayTool installation
        private static readonly string EmbeddedGamPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "AdminTrayTool", "GAM7", "gam.exe");

        // Optional global GAM7 folder (if user installs manually)
        private static readonly string GlobalGamPath =
            Path.Combine("C:\\GAM7", "gam.exe");

        public static string? SelectedGamPath { get; private set; }

        public static async Task<string?> LocateGam()
        {
            // 1️⃣ Check embedded GAM7 first (MSI deployment)
            if (File.Exists(EmbeddedGamPath))
            {
                SelectedGamPath = EmbeddedGamPath;
                return SelectedGamPath;
            }

            // 2️⃣ Check global GAM7 folder (manual install)
            if (File.Exists(GlobalGamPath))
            {
                SelectedGamPath = GlobalGamPath;
                return SelectedGamPath;
            }

            // 3️⃣ Check PATH for gam.exe
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "where",
                        Arguments = "gam",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(output))
                {
                    string path = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)[0];
                    if (File.Exists(path))
                    {
                        SelectedGamPath = path;
                        return SelectedGamPath;
                    }
                }
            }
            catch
            {
                // Ignore PATH errors
            }

            // GAM not found anywhere
            SelectedGamPath = null;
            return null;
        }
    }
}
