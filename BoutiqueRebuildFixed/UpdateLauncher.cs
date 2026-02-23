using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    internal static class UpdateLauncher
    {
        // ✅ URL RAW du version.json
        // Exemple: https://raw.githubusercontent.com/TON_USER/boutique-updates/main/version.json
        private const string VersionUrl = "https://raw.githubusercontent.com/TON_USER/boutique-updates/main/version.json";

        private class VersionInfo
        {
            public string version { get; set; }
            public string url { get; set; }
            public string sha256 { get; set; }
            public string notes { get; set; }
        }

        public static void TryLaunchUpdaterIfNeeded()
        {
            VersionInfo remote;

            // Télécharger version.json
            using (var wc = new WebClient())
            {
                wc.Headers.Add("Cache-Control", "no-cache");
                wc.Headers.Add("User-Agent", "BoutiqueRebuildFixed"); // évite certains blocages
                var json = wc.DownloadString(VersionUrl);
                remote = JsonConvert.DeserializeObject<VersionInfo>(json);
            }

            if (remote == null || string.IsNullOrWhiteSpace(remote.version) || string.IsNullOrWhiteSpace(remote.url))
                return;

            // ✅ Version locale = EXE principal
            var local = System.Reflection.Assembly.GetEntryAssembly()?.GetName()?.Version;
            var remoteV = ParseVersionSafe(remote.version);
            if (remoteV == null) return;

            if (local != null && local >= remoteV) return;

            // Lancer Updater.exe
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string updaterPath = Path.Combine(appDir, "Updater.exe");
            if (!File.Exists(updaterPath)) return;

            string exeToRestart = Path.Combine(appDir, "BoutiqueRebuildFixed.exe");

            var psi = new ProcessStartInfo
            {
                FileName = updaterPath,
                Arguments = $"\"{remote.url}\" \"{remote.sha256 ?? ""}\" \"{exeToRestart}\"",
                UseShellExecute = true,

                // ✅ Si tu installes dans Program Files, active runas (admin)
                // Si tu installes dans C:\BoutiqueRebuildFixed\ tu peux enlever.
                Verb = "runas"
            };

            Process.Start(psi);

            // Fermer l’app pour laisser Updater remplacer les fichiers
            Environment.Exit(0);
        }

        private static Version ParseVersionSafe(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            // accepte "1.0.15"
            var parts = s.Trim().Split('.');
            if (parts.Length == 3) s = s + ".0";

            Version v;
            return Version.TryParse(s, out v) ? v : null;
        }
    }
}