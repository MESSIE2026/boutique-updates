using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string root = AppDomain.CurrentDomain.BaseDirectory;

        // ✅ remonter à la racine de la solution (.sln)
        while (root != null && !Directory.GetFiles(root, "*.sln").Any())
        {
            var parent = Directory.GetParent(root);
            if (parent == null) break;
            root = parent.FullName;
        }

        if (root == null)
        {
            Console.WriteLine("❌ Racine de la solution introuvable.");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("📂 Solution détectée :");
        Console.WriteLine(root);
        Console.WriteLine();

        var files = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains(@"\bin\") && !f.Contains(@"\obj\"))
            .ToList();

        var rx = new Regex(
            @"\b(FROM|JOIN|INTO|UPDATE)\s+([a-zA-Z0-9_\.\[\]]+)",
            RegexOptions.IgnoreCase
        );

        var map = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var f in files)
        {
            string text;
            try
            {
                text = File.ReadAllText(f);
            }
            catch
            {
                continue;
            }

            foreach (Match m in rx.Matches(text))
            {
                var table = m.Groups[2].Value.Trim();
                if (!map.ContainsKey(f))
                    map[f] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                map[f].Add(table);
            }
        }

        // ===== AFFICHAGE =====
        foreach (var kv in map.OrderBy(k => k.Key))
        {
            Console.WriteLine("======================================");
            Console.WriteLine("📄 " + Path.GetFileName(kv.Key));

            foreach (var t in kv.Value.OrderBy(x => x))
                Console.WriteLine("   🔹 " + t);
        }

        Console.WriteLine("\n✅ Analyse terminée.");
        Console.WriteLine("Appuie sur une touche pour fermer.");
        Console.ReadKey();
    }
}