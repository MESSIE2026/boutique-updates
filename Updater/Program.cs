using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Updater
{
    internal class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 3) return 2;

            string zipUrl = args[0];
            string shaExpected = args[1];
            string exeToRestart = args[2];

            try
            {
                string appDir = Path.GetDirectoryName(exeToRestart);
                if (string.IsNullOrWhiteSpace(appDir) || !Directory.Exists(appDir))
                    return 3;

                string tempDir = Path.Combine(Path.GetTempPath(), "BoutiqueUpdate_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempDir);

                string zipPath = Path.Combine(tempDir, "update.zip");

                using (var wc = new WebClient())
                {
                    wc.Headers.Add("Cache-Control", "no-cache");
                    wc.Headers.Add("User-Agent", "BoutiqueRebuildFixed");
                    wc.DownloadFile(zipUrl, zipPath);
                }

                if (!string.IsNullOrWhiteSpace(shaExpected))
                {
                    string sha = ComputeSha256(zipPath);
                    if (!sha.Equals(shaExpected.Trim(), StringComparison.OrdinalIgnoreCase))
                        return 10;
                }

                string extractDir = Path.Combine(tempDir, "extract");
                Directory.CreateDirectory(extractDir);
                ZipFile.ExtractToDirectory(zipPath, extractDir);

                System.Threading.Thread.Sleep(800);
                CopyAll(extractDir, appDir);

                Process.Start(new ProcessStartInfo
                {
                    FileName = exeToRestart,
                    WorkingDirectory = appDir,
                    UseShellExecute = true
                });

                try { Directory.Delete(tempDir, true); } catch { }

                return 0;
            }
            catch
            {
                return 1;
            }
        }

        static void CopyAll(string sourceDir, string destDir)
        {
            foreach (var dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                string rel = dir.Substring(sourceDir.Length).TrimStart(Path.DirectorySeparatorChar);
                string target = Path.Combine(destDir, rel);
                if (!Directory.Exists(target))
                    Directory.CreateDirectory(target);
            }

            foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                string rel = file.Substring(sourceDir.Length).TrimStart(Path.DirectorySeparatorChar);
                string target = Path.Combine(destDir, rel);

                string name = Path.GetFileName(target).ToLowerInvariant();
                if (name == "config.json" || name.Contains("sqlconfig")) continue;

                File.Copy(file, target, true);
            }
        }

        static string ComputeSha256(string filePath)
        {
            using (var sha = SHA256.Create())
            using (var fs = File.OpenRead(filePath))
            {
                var hash = sha.ComputeHash(fs);
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
