using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public class CouponService
    {
        private readonly string _cs;

        public CouponService(string connectionString)
        {
            _cs = connectionString;
        }

        // ========================= DTO / RESULT =========================
        public class CouponPaiementLine
        {
            public string Mode { get; set; } = "";
            public string Devise { get; set; } = "";
            public decimal Montant { get; set; } = 0m;
            public string Reference { get; set; } = "";
        }

        public class CouponSplitResult
        {
            public bool IsValid { get; set; }
            public string Message { get; set; } = "";

            public decimal PromoTotale { get; set; }
            public decimal RemiseClient { get; set; }
            public decimal CreditPartenaire { get; set; }

            public int? IdPartenaire { get; set; } // partenaire final (depuis vente ou coupon)
        }

        public class CouponApplyResult
        {
            public bool Applied { get; set; }
            public string Message { get; set; } = "";

            public decimal RemiseClient { get; set; }
            public decimal CreditPartenaire { get; set; }
            public int? IdPartenaire { get; set; }
        }

        // ========================= API PUBLIQUE (PREVIEW) =========================
        public CouponSplitResult PreviewCouponSplit(string code, decimal montantBase, int idClient, int idPartenaire, string deviseVente)
        {
            using (var con = new SqlConnection(_cs))
            {
                con.Open();
                return PreviewCouponSplitTx(code, montantBase, idClient, idPartenaire, deviseVente, con, null);
            }
        }

        // ========================= COEUR PREVIEW (Tx) =========================
        public CouponSplitResult PreviewCouponSplitTx(
            string code, decimal montantBase, int idClient, int idPartenaire, string deviseVente,
            SqlConnection con, SqlTransaction trans)
        {
            var res = new CouponSplitResult { IsValid = false };

            code = (code ?? "").Trim().ToUpperInvariant();
            deviseVente = NormalizeDevise(deviseVente);

            if (string.IsNullOrWhiteSpace(code) || montantBase <= 0m)
            {
                res.Message = "Coupon vide ou montant invalide.";
                return res;
            }

            string type = "";
            decimal valeur = 0m;
            decimal minAchat = 0m;
            int? maxTotal = null;
            int? maxClient = null;
            bool actif = false;
            DateTime? debut = null;
            DateTime? fin = null;

            int couponPartenaire = 0;
            decimal partSharePct = 0m; // 0..100
            decimal pctClient = 100m;
            decimal pctPart = 0m;

            using (var cmd = new SqlCommand(@"
SELECT TOP 1
    Type, Valeur, MinAchat,
    UtilisationsMax, UtilisationsClientMax,
    Actif, DateDebut, DateFin,
    IdPartenaire, PartenaireSharePct
FROM dbo.Coupon
WHERE UPPER(Code)=@c;", con, trans))
            {
                cmd.Parameters.Add("@c", SqlDbType.NVarChar, 50).Value = code;

                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read())
                    {
                        res.Message = "Coupon introuvable.";
                        return res;
                    }

                    type = (r.IsDBNull(0) ? "" : r.GetString(0)).Trim().ToUpperInvariant();
                    valeur = r.IsDBNull(1) ? 0m : Convert.ToDecimal(r.GetValue(1));
                    minAchat = r.IsDBNull(2) ? 0m : Convert.ToDecimal(r.GetValue(2));

                    maxTotal = r.IsDBNull(3) ? (int?)null : Convert.ToInt32(r.GetValue(3));
                    maxClient = r.IsDBNull(4) ? (int?)null : Convert.ToInt32(r.GetValue(4));

                    actif = !r.IsDBNull(5) && Convert.ToBoolean(r.GetValue(5));
                    debut = r.IsDBNull(6) ? (DateTime?)null : Convert.ToDateTime(r.GetValue(6));
                    fin = r.IsDBNull(7) ? (DateTime?)null : Convert.ToDateTime(r.GetValue(7));

                    couponPartenaire = r.IsDBNull(8) ? 0 : Convert.ToInt32(r.GetValue(8));
                    partSharePct = r.IsDBNull(9) ? 0m : Convert.ToDecimal(r.GetValue(9));
                }
            }

            if (!actif) { res.Message = "Coupon inactif."; return res; }

            var today = DateTime.Today;
            if (debut.HasValue && today < debut.Value.Date) { res.Message = "Coupon pas encore valide."; return res; }
            if (fin.HasValue && today > fin.Value.Date) { res.Message = "Coupon expiré."; return res; }

            if (montantBase < minAchat) { res.Message = "Montant < minimum achat."; return res; }

            if (partSharePct < 0m) partSharePct = 0m;
            if (partSharePct > 100m) partSharePct = 100m;

            pctPart = partSharePct;
            pctClient = 100m - pctPart;

            // partenaire final : priorité vente, sinon coupon
            int finalPartenaire = (idPartenaire > 0) ? idPartenaire : couponPartenaire;
            res.IdPartenaire = (finalPartenaire > 0) ? (int?)finalPartenaire : null;

            // limites globales
            if (maxTotal.HasValue)
            {
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.CouponUsage WHERE UPPER(Code)=@c;", con, trans))
                {
                    cmd.Parameters.Add("@c", SqlDbType.NVarChar, 50).Value = code;
                    int used = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                    if (used >= maxTotal.Value) { res.Message = "Coupon épuisé."; return res; }
                }
            }

            // limites client
            if (maxClient.HasValue && idClient > 0)
            {
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.CouponUsage WHERE UPPER(Code)=@c AND IdClient=@cl;", con, trans))
                {
                    cmd.Parameters.Add("@c", SqlDbType.NVarChar, 50).Value = code;
                    cmd.Parameters.Add("@cl", SqlDbType.Int).Value = idClient;
                    int usedClient = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                    if (usedClient >= maxClient.Value) { res.Message = "Déjà utilisé par ce client."; return res; }
                }
            }

            // promo totale
            decimal promoTotale;

            if (type == "POURCENT")
            {
                promoTotale = Math.Round(montantBase * (valeur / 100m), 2);
            }
            else if (type == "FIXE")
            {
                promoTotale = Math.Round(valeur, 2);
            }
            else
            {
                res.Message = "Type coupon invalide.";
                return res;
            }

            if (promoTotale > montantBase) promoTotale = montantBase;
            if (promoTotale < 0m) promoTotale = 0m;

            decimal remiseClient = Math.Round(promoTotale * (pctClient / 100m), 2);
            decimal creditPart = Math.Round(promoTotale - remiseClient, 2);

            if (finalPartenaire <= 0 && creditPart > 0m)
            {
                remiseClient = promoTotale;
                creditPart = 0m;
            }

            res.IsValid = promoTotale > 0m;
            res.PromoTotale = promoTotale;
            res.RemiseClient = remiseClient;
            res.CreditPartenaire = creditPart;
            res.Message = res.IsValid ? "OK" : "Promo nulle";
            return res;
        }

        // ========================= API PUBLIQUE (APPLY) =========================
        public CouponApplyResult ApplyCouponSplitAndSaveTx(
            int idClient,
            string codeCoupon,
            decimal baseAmountAvantCoupon,
            decimal remiseCouponReelle,
            int idVente,
            string deviseVente,
            int idPartenaire,
            string refTransaction,
            IEnumerable<CouponPaiementLine> payLines,
            SqlConnection con,
            SqlTransaction trans)
        {
            var outRes = new CouponApplyResult
            {
                Applied = false,
                Message = "",
                RemiseClient = 0m,
                CreditPartenaire = 0m,
                IdPartenaire = (idPartenaire > 0) ? (int?)idPartenaire : null
            };

            codeCoupon = (codeCoupon ?? "").Trim().ToUpperInvariant();
            deviseVente = NormalizeDevise(deviseVente);

            if (string.IsNullOrWhiteSpace(codeCoupon)) { outRes.Message = "Coupon vide."; return outRes; }
            if (baseAmountAvantCoupon <= 0m) { outRes.Message = "Montant base invalide."; return outRes; }
            if (remiseCouponReelle <= 0m) { outRes.Message = "Remise coupon nulle."; return outRes; }

            var prev = PreviewCouponSplitTx(codeCoupon, baseAmountAvantCoupon, idClient, idPartenaire, deviseVente, con, trans);
            if (prev == null || !prev.IsValid || prev.PromoTotale <= 0m)
            {
                outRes.Message = prev?.Message ?? "Coupon invalide.";
                return outRes;
            }

            decimal promoUtilisee = Math.Round(remiseCouponReelle, 2);
            if (promoUtilisee > baseAmountAvantCoupon) promoUtilisee = baseAmountAvantCoupon;
            if (promoUtilisee < 0m) promoUtilisee = 0m;
            if (promoUtilisee <= 0m) { outRes.Message = "Promo utilisée nulle."; return outRes; }

            decimal ratioClient = prev.PromoTotale > 0m ? (prev.RemiseClient / prev.PromoTotale) : 1m;
            if (ratioClient < 0m) ratioClient = 0m;
            if (ratioClient > 1m) ratioClient = 1m;

            decimal remiseClient = Math.Round(promoUtilisee * ratioClient, 2);
            decimal creditPart = Math.Round(promoUtilisee - remiseClient, 2);

            int finalPartenaire = (idPartenaire > 0) ? idPartenaire : (prev.IdPartenaire ?? 0);
            if (finalPartenaire <= 0)
            {
                remiseClient = promoUtilisee;
                creditPart = 0m;
            }

            string modePaiementResume = BuildModePaiementResume(payLines);

            SaveCouponUsageAndPartnerCreditTx(
                con, trans,
                codeCoupon,
                idClient, idVente,
                remiseClient,
                finalPartenaire, creditPart,
                refTransaction,
                modePaiementResume
            );

            outRes.Applied = true;
            outRes.Message = "OK";
            outRes.RemiseClient = remiseClient;
            outRes.CreditPartenaire = creditPart;
            outRes.IdPartenaire = (finalPartenaire > 0) ? (int?)finalPartenaire : null;

            TryLogCouponPaiementsIfTableExists(con, trans, codeCoupon, idVente, refTransaction, payLines);

            return outRes;
        }

        // ========================= DB WRITE (INTERNE) =========================
        private void SaveCouponUsageAndPartnerCreditTx(
            SqlConnection con, SqlTransaction trans,
            string codeCoupon,
            int idClient, int idVente,
            decimal remiseClient,
            int idPartenaire, decimal creditPartenaire,
            string refTransaction,
            string modePaiementResume)
        {
            // 1) CouponUsage (ta table + colonnes ajoutées)
            using (var cmd = new SqlCommand(@"
INSERT INTO dbo.CouponUsage
    (Code, IdClient, IdVente, DateUsage, MontantRemise, IdPartenaire, MontantCreditPartenaire, ReferenceTransaction, ModePaiement)
VALUES
    (@code,@cl,@v,GETDATE(),@rem,@pid,@pamt,@ref,@mp);", con, trans))
            {
                cmd.Parameters.Add("@code", SqlDbType.NVarChar, 50).Value = (codeCoupon ?? "").Trim().ToUpperInvariant();
                cmd.Parameters.Add("@cl", SqlDbType.Int).Value = idClient;
                cmd.Parameters.Add("@v", SqlDbType.Int).Value = idVente;

                var pRem = cmd.Parameters.Add("@rem", SqlDbType.Decimal);
                pRem.Precision = 18; pRem.Scale = 2; pRem.Value = Math.Round(remiseClient, 2);

                cmd.Parameters.Add("@pid", SqlDbType.Int).Value = (idPartenaire > 0) ? (object)idPartenaire : DBNull.Value;

                var pAmt = cmd.Parameters.Add("@pamt", SqlDbType.Decimal);
                pAmt.Precision = 18; pAmt.Scale = 2; pAmt.Value = Math.Round(creditPartenaire, 2);

                cmd.Parameters.Add("@ref", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(refTransaction) ? (object)DBNull.Value : refTransaction.Trim();

                cmd.Parameters.Add("@mp", SqlDbType.NVarChar, 200).Value =
                    string.IsNullOrWhiteSpace(modePaiementResume) ? (object)DBNull.Value : modePaiementResume;

                cmd.ExecuteNonQuery();
            }

            // 2) Crédit partenaire
            if (idPartenaire > 0 && creditPartenaire > 0m)
            {
                using (var cmd = new SqlCommand(@"
IF NOT EXISTS(SELECT 1 FROM dbo.PartenaireComptePromo WHERE IdPartenaire=@p)
    INSERT INTO dbo.PartenaireComptePromo(IdPartenaire,Solde,DateMaj) VALUES(@p,0,GETDATE());

UPDATE dbo.PartenaireComptePromo
SET Solde = ISNULL(Solde,0) + @m,
    DateMaj = GETDATE()
WHERE IdPartenaire=@p;

INSERT INTO dbo.PartenairePromoMvt(IdPartenaire,DateMvt,CodeCoupon,IdVente,Montant,Note)
VALUES(@p,GETDATE(),@c,@v,@m,@n);", con, trans))
                {
                    cmd.Parameters.Add("@p", SqlDbType.Int).Value = idPartenaire;
                    cmd.Parameters.Add("@c", SqlDbType.NVarChar, 50).Value = (codeCoupon ?? "").Trim().ToUpperInvariant();
                    cmd.Parameters.Add("@v", SqlDbType.Int).Value = idVente;

                    var pM = cmd.Parameters.Add("@m", SqlDbType.Decimal);
                    pM.Precision = 18; pM.Scale = 2; pM.Value = Math.Round(creditPartenaire, 2);

                    cmd.Parameters.Add("@n", SqlDbType.NVarChar, 200).Value = "Crédit promo via coupon";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ========================= OPTIONNELS (LOG) =========================
        private void TryLogCouponPaiementsIfTableExists(
            SqlConnection con, SqlTransaction tx,
            string codeCoupon, int idVente, string refTransaction,
            IEnumerable<CouponPaiementLine> payLines)
        {
            if (payLines == null) return;
            var list = payLines
                .Where(line => line != null && line.Montant > 0m)
                 .ToList();
            if (list.Count == 0) return;

            using (var check = new SqlCommand("SELECT CASE WHEN OBJECT_ID('dbo.CouponUsagePaiements','U') IS NULL THEN 0 ELSE 1 END;", con, tx))
            {
                int ok = Convert.ToInt32(check.ExecuteScalar() ?? 0);
                if (ok == 0) return;
            }

            foreach (var p in list)
            {
                using (var cmd = new SqlCommand(@"
INSERT INTO dbo.CouponUsagePaiements(IdVente, CodeCoupon, RefTransaction, ModePaiement, Devise, Montant, DateCreation)
VALUES(@v,@c,@r,@m,@d,@amt,GETDATE());", con, tx))
                {
                    cmd.Parameters.Add("@v", SqlDbType.Int).Value = idVente;
                    cmd.Parameters.Add("@c", SqlDbType.NVarChar, 50).Value = (codeCoupon ?? "").Trim().ToUpperInvariant();

                    cmd.Parameters.Add("@r", SqlDbType.NVarChar, 80).Value =
                        string.IsNullOrWhiteSpace(refTransaction) ? (object)DBNull.Value : refTransaction.Trim();

                    cmd.Parameters.Add("@m", SqlDbType.NVarChar, 40).Value = (p.Mode ?? "").Trim().ToUpperInvariant();
                    cmd.Parameters.Add("@d", SqlDbType.NVarChar, 10).Value = NormalizeDevise(p.Devise);

                    var pA = cmd.Parameters.Add("@amt", SqlDbType.Decimal);
                    pA.Precision = 18; pA.Scale = 2; pA.Value = Math.Round(p.Montant, 2);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ========================= HELPERS =========================
        private static string BuildModePaiementResume(IEnumerable<CouponPaiementLine> payLines)
        {
            string mp = "INCONNU";
            if (payLines != null)
            {
                var modes = payLines
                    .Where(x => x != null)
                    .Select(x => (x.Mode ?? "").Trim().ToUpperInvariant())
                    .Where(x => x.Length > 0)
                    .Distinct()
                    .ToList();

                if (modes.Count > 0) mp = string.Join(" + ", modes);
            }
            return mp;
        }

        private static string NormalizeDevise(string d)
        {
            d = (d ?? "CDF").Trim().ToUpperInvariant();
            if (d == "FC") d = "CDF";
            return d;
        }
    }
}