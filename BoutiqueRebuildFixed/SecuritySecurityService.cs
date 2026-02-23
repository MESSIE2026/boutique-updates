using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BoutiqueRebuildFixed.Models;
using BoutiqueRebuildFixed.Services;

namespace BoutiqueRebuildFixed.Security
{
    public static class SecurityService
    {
        public static AuthResult TryAuthorize(Form owner, AuthRequest req)
        {
            if (req == null) return AuthResult.Deny("Requête d'autorisation invalide.");
            if (string.IsNullOrWhiteSpace(req.ActionCode)) return AuthResult.Deny("ActionCode manquant.");

            // 0) Contexte entreprise (si tu as AppContext / POS)
            // Mets ce que tu as déjà :
            // req.IdEntreprise ??= AppContext.IdEntreprise; etc.

            // 1) Admin direct (si tu veux forcer signature même admin, retire ce bloc)
            if (ConfigSysteme.RolesSecurite.EstAdmin(SessionEmploye.Poste))
                return AuthResult.Ok();

            // 2) Permission DB (ton système)
            bool autoriseDb =
                ConfigSysteme.EstModuleAutoriseDb(SessionEmploye.Poste, req.ActionCode)
                || (req.ActionCode.StartsWith("OPEN_MODULE_", StringComparison.OrdinalIgnoreCase)
                    && ConfigSysteme.EstModuleAutoriseDb(SessionEmploye.Poste, req.ActionCode.Replace("OPEN_MODULE_", "")));

            // 3) Règles grandes entreprises (simple mais efficace)
            // 3.1) si action critique => signature obligatoire
            bool forceSignature = req.AlwaysSignature || req.RiskLevel >= 2;

            // 3.2) si montant => règle seuil (exemple: > 10 000 => signature)
            if (req.Amount.HasValue && req.Amount.Value >= 10000m)
                forceSignature = true;

            // 3.3) scope (magasin/région/entreprise) : si tu veux bloquer hors scope
            // Ici on laisse passer, mais tu peux ajouter un check si tu stockes les scopes en DB.

            // 4) Si accès contrôle OFF et non autorisé => REFUS
            if (!ConfigSysteme.AccesControleOn && !autoriseDb)
            {
                AuditService.Fail("Accès refusé",
                    $"{req.Title} | Action={req.ActionCode} | REFUS (DB=0, AccesControle=OFF) | Demandeur={SessionEmploye.Prenom} {SessionEmploye.Nom} ({SessionEmploye.Poste})");
                return AuthResult.Deny("Non autorisé (DB) et Accès contrôlé OFF.");
            }

            // 5) Autorisé DB et pas signature forcée => OK
            if (autoriseDb && !forceSignature)
            {
                AuditService.Success("Accès autorisé",
                    $"{req.Title} | Action={req.ActionCode} | OK (DB=1, Signature=0) | Demandeur={SessionEmploye.Prenom} {SessionEmploye.Nom} ({SessionEmploye.Poste})");
                return AuthResult.Ok();
            }

            // 6) Sinon => signature manager obligatoire
            using (var sig = new FrmSignatureManager(
                connectionString: ConfigSysteme.ConnectionString,
                typeAction: req.Title,
                permissionCode: req.ActionCode,
                reference: req.Reference ?? req.ActionCode,
                details: req.Details ?? "",
                idEmployeDemandeur: SessionEmploye.ID_Employe,
                roleDemandeur: SessionEmploye.Poste
            ))
            {
                var dr = sig.ShowDialog(owner);

                if (dr == DialogResult.OK && sig.Approved)
                {
                    AuditService.Success("Signature OK",
                        $"{req.Title} | Action={req.ActionCode} | Signature=OK | ValidéPar={sig.ManagerNom} ({sig.ManagerPoste}) | Demandeur={SessionEmploye.Prenom} {SessionEmploye.Nom} ({SessionEmploye.Poste})");
                    return new AuthResult
                    {
                        Allowed = true,
                        UsedSignature = true,
                        ManagerName = sig.ManagerNom,
                        ManagerRole = sig.ManagerPoste
                    };
                }

                AuditService.Fail("Signature refusée",
                    $"{req.Title} | Action={req.ActionCode} | Signature=REFUS | Demandeur={SessionEmploye.Prenom} {SessionEmploye.Nom} ({SessionEmploye.Poste})");

                return AuthResult.Deny("Signature refusée.");
            }
        }
    }
}