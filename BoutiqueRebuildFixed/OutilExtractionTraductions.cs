using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    internal class OutilExtractionTraductions
    {
        public static void ExtraireDepuisProjet(string dossierProjet, string cheminJson)
        {
            var textes = new HashSet<string>();

            var fichiers = Directory.GetFiles(
                dossierProjet,
                "*.Designer.cs",
                SearchOption.AllDirectories
            );

            Regex regex = new Regex(@"Text\s*=\s*""([^""]+)""");

            foreach (var fichier in fichiers)
            {
                string contenu = File.ReadAllText(fichier);

                foreach (Match match in regex.Matches(contenu))
                {
                    string texte = match.Groups[1].Value.Trim();

                    if (!string.IsNullOrWhiteSpace(texte))
                        textes.Add(texte);
                }
            }

            var json = JsonConvert.SerializeObject(textes, Formatting.Indented);
            File.WriteAllText(cheminJson, json);
        }
    }
}