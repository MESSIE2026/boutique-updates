using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office.Word;

namespace BoutiqueRebuildFixed
{
    public partial class FormBackupRestore : FormBase
    {
        public FormBackupRestore()
        {
            InitializeComponent();

            this.Load += FormBackupRestore_Load;

            ConfigSysteme.OnLangueChange += RafraichirLangue;
            ConfigSysteme.OnThemeChange += RafraichirTheme;

        }

        private void groupBoxBackup_Enter(object sender, EventArgs e)
        {

        }

        private void FormBackupRestore_Load(object sender, EventArgs e)
        {
            ChargerBases();

            lblBackupStatus.Text = "";
            lblRestoreStatus.Text = "";

            progressBarBackup.Minimum = 0;
            progressBarBackup.Maximum = 100;
            progressBarBackup.Value = 0;

            progressBarRestore.Minimum = 0;
            progressBarRestore.Maximum = 100;
            progressBarRestore.Value = 0;

            RafraichirLangue();
            RafraichirTheme();
        }

        private void RafraichirLangue() => ConfigSysteme.AppliquerTraductions(this);
        private void RafraichirTheme() => ConfigSysteme.AppliquerTheme(this);

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ConfigSysteme.OnLangueChange -= RafraichirLangue;
            ConfigSysteme.OnThemeChange -= RafraichirTheme;
            base.OnFormClosed(e);
        }

        private static string GetSqlCmdPath()
        {
            // 1) tente PATH
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "sqlcmd",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var p = Process.Start(psi))
                {
                    string outp = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();

                    var first = (outp ?? "")
                        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(first) && File.Exists(first))
                        return first.Trim();
                }
            }
            catch { /* ignore */ }

            // 2) fallback: laisser "sqlcmd" (Windows va chercher)
            return "sqlcmd";
        }


        private string BuildBackupScriptFile(string databaseName)
        {
            string folder = BackupRoot();

            string sqlPath = Path.Combine(folder, "backup_auto.sql");
            string batPath = Path.Combine(folder, "backup_auto.bat");
            string logPath = Path.Combine(folder, "backup_auto_log.txt");

            // Server depuis ta connection string
            var csb = new SqlConnectionStringBuilder(ConfigSysteme.ConnectionString);
            string server = csb.DataSource; // ex: MESSIE-PC\SQLEXPRESS

            // SQL script : SANS COMPRESSION (Express compatible)
            File.WriteAllText(sqlPath, $@"
DECLARE @d nvarchar(8) = CONVERT(nvarchar(8), GETDATE(), 112);
DECLARE @path nvarchar(400) = '{folder.Replace(@"\", @"\\")}\{databaseName}_' + @d + '.bak';

BACKUP DATABASE [{databaseName}]
TO DISK = @path
WITH INIT, STATS = 10;
", Encoding.UTF8);

            // Auth SQLCMD selon config
            string authPart;
            if (ConfigSysteme.UseWindowsAuth)
            {
                authPart = "-E";
            }
            else
            {
                string u = (ConfigSysteme.Utilisateur ?? "").Replace("\"", "");
                string p = (ConfigSysteme.MotDePasse ?? "").Replace("\"", "");
                authPart = $"-U \"{u}\" -P \"{p}\"";
            }

            string sqlcmd = GetSqlCmdPath();

            // BAT script (propre)
            File.WriteAllText(batPath, $@"@echo off
setlocal

set ""SERVER={server}""
set ""SCRIPT={sqlPath}""
set ""LOG={logPath}""

echo =============================>>""%LOG%""
echo START %DATE% %TIME%>>""%LOG%""
echo SERVER=%SERVER%>>""%LOG%""
echo SCRIPT=%SCRIPT%>>""%LOG%""
echo =============================>>""%LOG%""

""{sqlcmd}"" -S ""%SERVER%"" {authPart} -i ""%SCRIPT%"" >>""%LOG%"" 2>&1

echo EXITCODE=%ERRORLEVEL%>>""%LOG%""

REM purge backups > 14 jours
forfiles /p ""{folder}"" /m *.bak /d -14 /c ""cmd /c del @path"" >>""%LOG%"" 2>&1

echo END %DATE% %TIME%>>""%LOG%""
endlocal
", Encoding.UTF8);

            return batPath;
        }



        // =========================================================
        // ✅ Chemin portable (valable chez tous les clients)
        // C:\ProgramData\BoutiqueRebuildFixed\Backups
        // =========================================================
        private static string BackupRoot()
        {
            string root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "BoutiqueRebuildFixed",
                "Backups"
            );

            Directory.CreateDirectory(root);
            return root;
        }

        private void ChargerBases()
        {
            cbDatabases.Items.Clear();

            using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT name FROM sys.databases WHERE database_id > 4", con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        cbDatabases.Items.Add(reader.GetString(0));
                }
            }

            if (cbDatabases.Items.Count > 0)
                cbDatabases.SelectedIndex = 0;
        }

        // =========================================================
        // ✅ BACKUP
        // =========================================================
        private void ExecuterBackup(string chemin, string databaseName)
        {
            try
            {
                var dir = Path.GetDirectoryName(chemin);
                if (!string.IsNullOrWhiteSpace(dir))
                    Directory.CreateDirectory(dir);

                using (SqlConnection con = new SqlConnection(ConfigSysteme.ConnectionString))
                {
                    con.Open();

                    // ✅ Express compatible : pas de COMPRESSION
                    string sql = $@"
BACKUP DATABASE [{databaseName}]
TO DISK = @Chemin
WITH INIT, STATS = 10;";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.Add("@Chemin", SqlDbType.NVarChar, 400).Value = chemin;
                        cmd.CommandTimeout = 0;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur lors de la sauvegarde : " + ex.Message, ex);
            }
        }

        private void CreerTacheBackupWindows(string batPath, bool interactiveOnly = true)
        {
            string taskName = "BoutiqueRebuildFixed_Backup_2300";

            // /IT = exécuter uniquement si l'utilisateur est connecté (évite SYSTEM + -E)
            string it = interactiveOnly ? " /IT" : "";

            string args =
                $"/Create /F /TN \"{taskName}\" /TR \"\\\"{batPath}\\\"\" /SC DAILY /ST 23:00 /RL HIGHEST{it}";

            var psi = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = args,

                // ✅ UAC elevation
                UseShellExecute = true,
                Verb = "runas",

                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using (var p = Process.Start(psi))
            {
                p.WaitForExit();
                if (p.ExitCode != 0)
                    throw new Exception(
                        "Impossible de créer la tâche Windows.\n" +
                        "✅ Lance en Administrateur ou accepte la fenêtre UAC.\n" +
                        "💡 Astuce: si /IT bloque, essaie interactiveOnly=false."
                    );
            }
        }



        private async void btnStartBackup_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBackupPath.Text))
            {
                MessageBox.Show("Veuillez choisir un chemin de sauvegarde.");
                return;
            }

            string databaseName = cbDatabases.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(databaseName))
            {
                MessageBox.Show("Veuillez sélectionner une base de données.");
                return;
            }

            btnStartBackup.Enabled = false;
            lblBackupStatus.Text = "Sauvegarde en cours...";
            progressBarBackup.Value = 10;

            try
            {
                await Task.Run(() => ExecuterBackup(txtBackupPath.Text, databaseName));

                progressBarBackup.Value = 100;
                lblBackupStatus.Text = "Sauvegarde terminée ✔";

                ConfigSysteme.AjouterAuditLog(
                    "Backup",
                    $"Sauvegarde DB {databaseName} vers {txtBackupPath.Text}",
                    "Succès"
                );
            }
            catch (Exception ex)
            {
                lblBackupStatus.Text = "Erreur lors de la sauvegarde ❌";
                MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);

                ConfigSysteme.AjouterAuditLog(
                    "Backup",
                    $"Sauvegarde DB {databaseName} vers {txtBackupPath.Text}",
                    $"Erreur: {ex.Message}"
                );
            }
            finally
            {
                btnStartBackup.Enabled = true;
            }
        }

        private void btnBrowseBackup_Click(object sender, EventArgs e)
        {
            string backupFolder = BackupRoot();

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.InitialDirectory = backupFolder;
                sfd.Filter = "Backup SQL (*.bak)|*.bak";

                string db = cbDatabases.SelectedItem?.ToString() ?? ConfigSysteme.BaseDeDonnees;
                sfd.FileName = $"{db}_{DateTime.Now:yyyyMMdd_HHmm}.bak";

                if (sfd.ShowDialog() == DialogResult.OK)
                    txtBackupPath.Text = sfd.FileName;
            }
        }

        private void btnBrowseRestore_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Backup SQL (*.bak)|*.bak";
                if (ofd.ShowDialog() == DialogResult.OK)
                    txtRestorePath.Text = ofd.FileName;
            }
        }

        private async void btnStartRestore_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRestorePath.Text))
            {
                MessageBox.Show("Veuillez choisir un fichier de sauvegarde.");
                return;
            }

            if (!File.Exists(txtRestorePath.Text))
            {
                MessageBox.Show("Le fichier sélectionné n'existe pas.");
                return;
            }

            if (MessageBox.Show(
                "ATTENTION : Cette opération remplacera totalement la base de données.\nContinuer ?",
                "Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            btnStartRestore.Enabled = false;
            lblRestoreStatus.Text = "Restauration en cours...";
            progressBarRestore.Value = 10;

            try
            {
                await Task.Run(() => ExecuterRestauration(txtRestorePath.Text));

                progressBarRestore.Value = 100;
                lblRestoreStatus.Text = "Restauration terminée ✔";

                ConfigSysteme.AjouterAuditLog(
                    "Restauration",
                    $"Restauration DB depuis {txtRestorePath.Text}",
                    "Succès"
                );

                MessageBox.Show("Restauration effectuée avec succès.\nRedémarrage recommandé.");
            }
            catch (Exception ex)
            {
                lblRestoreStatus.Text = "Erreur lors de la restauration ❌";
                MessageBox.Show($"Erreur lors de la restauration : {ex.Message}", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);

                ConfigSysteme.AjouterAuditLog(
                    "Restauration",
                    $"Restauration DB depuis {txtRestorePath.Text}",
                    $"Erreur: {ex.Message}"
                );
            }
            finally
            {
                btnStartRestore.Enabled = true;
            }
        }

        private void ExecuterRestauration(string cheminBak)
        {
            string db = ConfigSysteme.BaseDeDonnees;

            var csb = new SqlConnectionStringBuilder(ConfigSysteme.ConnectionString)
            {
                InitialCatalog = "master"
            };

            using (SqlConnection con = new SqlConnection(csb.ConnectionString))
            {
                con.Open();

                // 1) Lire les logical files du backup
                string logicalData = null;
                string logicalLog = null;

                using (var cmdFileList = new SqlCommand("RESTORE FILELISTONLY FROM DISK = @b", con))
                {
                    cmdFileList.Parameters.Add("@b", SqlDbType.NVarChar, 400).Value = cheminBak;
                    cmdFileList.CommandTimeout = 0;

                    using (var r = cmdFileList.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string logicalName = Convert.ToString(r["LogicalName"]);
                            string type = Convert.ToString(r["Type"]); // D ou L

                            if (type == "D" && logicalData == null) logicalData = logicalName;
                            if (type == "L" && logicalLog == null) logicalLog = logicalName;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(logicalData) || string.IsNullOrWhiteSpace(logicalLog))
                    throw new Exception("Impossible de lire les fichiers logiques du backup (FILELISTONLY).");

                // 2) Déterminer les chemins serveur (Data/Log)
                string serverDataPath = null;
                string serverLogPath = null;

                using (var cmdPaths = new SqlCommand(@"
SELECT 
    CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(400)) AS DataPath,
    CAST(SERVERPROPERTY('InstanceDefaultLogPath') AS nvarchar(400)) AS LogPath;", con))
                {
                    using (var r = cmdPaths.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            serverDataPath = Convert.ToString(r["DataPath"]);
                            serverLogPath = Convert.ToString(r["LogPath"]);
                        }
                    }
                }

                // fallback si null (certaines configs Express)
                if (string.IsNullOrWhiteSpace(serverDataPath))
                {
                    using (var cmdDbFiles = new SqlCommand(@"
SELECT TOP 1
    LEFT(physical_name, LEN(physical_name) - CHARINDEX('\', REVERSE(physical_name))) AS FolderPath
FROM sys.master_files
WHERE database_id = DB_ID(@db) AND type_desc='ROWS';", con))
                    {
                        cmdDbFiles.Parameters.Add("@db", SqlDbType.NVarChar, 128).Value = db;
                        serverDataPath = Convert.ToString(cmdDbFiles.ExecuteScalar());
                        serverLogPath = serverDataPath;
                    }
                }

                if (string.IsNullOrWhiteSpace(serverDataPath))
                    throw new Exception("Chemin DATA SQL introuvable sur ce serveur.");

                string mdfTarget = Path.Combine(serverDataPath, db + ".mdf");
                string ldfTarget = Path.Combine(serverLogPath ?? serverDataPath, db + "_log.ldf");

                // 3) Restore sûr : try/finally pour toujours remettre MULTI_USER
                string sqlRestore = $@"
ALTER DATABASE [{db}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

RESTORE DATABASE [{db}]
FROM DISK = @Chemin
WITH REPLACE,
     MOVE @LogicalData TO @Mdf,
     MOVE @LogicalLog  TO @Ldf,
     STATS = 10;
";

                try
                {
                    using (SqlCommand cmd = new SqlCommand(sqlRestore, con))
                    {
                        cmd.Parameters.Add("@Chemin", SqlDbType.NVarChar, 400).Value = cheminBak;
                        cmd.Parameters.Add("@LogicalData", SqlDbType.NVarChar, 200).Value = logicalData;
                        cmd.Parameters.Add("@LogicalLog", SqlDbType.NVarChar, 200).Value = logicalLog;
                        cmd.Parameters.Add("@Mdf", SqlDbType.NVarChar, 400).Value = mdfTarget;
                        cmd.Parameters.Add("@Ldf", SqlDbType.NVarChar, 400).Value = ldfTarget;

                        cmd.CommandTimeout = 0;
                        cmd.ExecuteNonQuery();
                    }
                }
                finally
                {
                    using (var cmdMulti = new SqlCommand($@"ALTER DATABASE [{db}] SET MULTI_USER;", con))
                    {
                        cmdMulti.CommandTimeout = 0;
                        cmdMulti.ExecuteNonQuery();
                    }
                }
            }
        }


        private void btnHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Sauvegarde : crée un fichier .bak de la base actuelle.\n" +
                "Restauration : remplace totalement la base par le fichier choisi.\n\n" +
                "⚠ Toujours sauvegarder avant restauration.",
                "Aide Backup / Restauration",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnPlanifierBackup_Click(object sender, EventArgs e)
        {
            string db = cbDatabases.SelectedItem?.ToString() ?? ConfigSysteme.BaseDeDonnees;

            try
            {
                string bat = BuildBackupScriptFile(db);

                // ✅ True => fonctionne bien avec -E (Windows Auth) si user connecté
                // ✅ False => utile si tu veux que ça tourne même sans session ouverte
                CreerTacheBackupWindows(bat, interactiveOnly: true);

                MessageBox.Show("Sauvegarde automatique activée (tous les jours à 23:00).", "OK",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                ConfigSysteme.AjouterAuditLog("BackupAuto", $"Planification backup auto 23h pour {db}", "Succès");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ConfigSysteme.AjouterAuditLog("BackupAuto", $"Planification backup auto 23h pour {db}", "Erreur: " + ex.Message);
            }
        }
    }
 }
