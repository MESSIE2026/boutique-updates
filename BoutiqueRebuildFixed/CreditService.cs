using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoutiqueRebuildFixed
{
    public class CreditService
    {
        private readonly string _cs;
        public CreditService(string cs) { _cs = cs; }

        // ===================== DTO =====================
        public class CreditRow
        {
            public int IdCredit { get; set; }
            public int IdClient { get; set; }
            public int IdVente { get; set; }
            public string ClientNom { get; set; }
            public string Telephone { get; set; }
            public decimal Total { get; set; }
            public decimal Reste { get; set; }
            public string Devise { get; set; }
            public DateTime DateCredit { get; set; }
            public DateTime? DateEcheance { get; set; }
            public string Statut { get; set; }
            public string RefVente { get; set; }
        }

        // ===================== CREATE CREDIT =====================
        // total = netFinal, acompte = sumPay, reste = netFinal - acompte
        public int CreateCreditVente(
            int idClient,
            int idVente,
            string refVente,
            decimal total,
            decimal acompte,
            decimal reste,
            DateTime? echeance,
            SqlConnection con,
            SqlTransaction trans)
        {
            // 1) insert header CreditVente
            int idCredit;
            using (var cmd = new SqlCommand(@"
INSERT INTO dbo.CreditVente (IdClient, DateCredit, Total, Reste, DateEcheance, Statut, RefVente, IdVente)
OUTPUT INSERTED.IdCredit
VALUES (@idClient, GETDATE(), @total, @reste, @echeance, @statut, @refVente, @idVente);", con, trans))
            {
                cmd.Parameters.Add("@idClient", SqlDbType.Int).Value = idClient;
                cmd.Parameters.Add("@idVente", SqlDbType.Int).Value = idVente;
                cmd.Parameters.Add("@refVente", SqlDbType.NVarChar, 60).Value = (refVente ?? "");
                var pT = cmd.Parameters.Add("@total", SqlDbType.Decimal); pT.Precision = 18; pT.Scale = 2; pT.Value = total;
                var pR = cmd.Parameters.Add("@reste", SqlDbType.Decimal); pR.Precision = 18; pR.Scale = 2; pR.Value = reste;
                cmd.Parameters.Add("@echeance", SqlDbType.Date).Value = (object)(echeance?.Date ?? (DateTime?)null) ?? DBNull.Value;
                cmd.Parameters.Add("@statut", SqlDbType.NVarChar, 20).Value = (reste <= 0m) ? "PAYE" : "OUVERT";

                idCredit = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // 2) optionnel: enregistrer acompte dans CreditPaiement (historique)
            if (acompte > 0m)
            {
                using (var cmdPay = new SqlCommand(@"
INSERT INTO dbo.CreditPaiement (IdCredit, DatePaiement, Montant, ModePaiement, Note)
VALUES (@idCredit, GETDATE(), @m, @mode, @note);", con, trans))
                {
                    cmdPay.Parameters.Add("@idCredit", SqlDbType.Int).Value = idCredit;
                    var pm = cmdPay.Parameters.Add("@m", SqlDbType.Decimal); pm.Precision = 18; pm.Scale = 2; pm.Value = acompte;
                    cmdPay.Parameters.Add("@mode", SqlDbType.NVarChar, 40).Value = "ACOMPTE";
                    cmdPay.Parameters.Add("@note", SqlDbType.NVarChar, 200).Value = "Acompte à la vente (" + (refVente ?? "") + ")";
                    cmdPay.ExecuteNonQuery();
                }
            }

            return idCredit;
        }

        private void InsertPaiementVente(
    int idVente,
    decimal montant,
    string modePaiement,
    string note,
    string devise,
    SqlConnection con,
    SqlTransaction trans)
        {
            if (idVente <= 0) throw new Exception("INVALID idVente (paiement vente)");

            devise = string.IsNullOrWhiteSpace(devise) ? "CDF" : devise.Trim().ToUpperInvariant();
            if (devise == "FC") devise = "CDF";

            using (var cmd = new SqlCommand(@"
INSERT INTO dbo.PaiementsVente
(
    IdVente,
    ModePaiement,
    Devise,
    Montant,
    DatePaiement,
    ReferenceTransaction,
    Statut
)
VALUES
(
    @idVente,
    @mode,
    @devise,
    @montant,
    GETDATE(),
    @ref,
    @statut
);", con, trans))
            {
                cmd.Parameters.Add("@idVente", SqlDbType.Int).Value = idVente;
                cmd.Parameters.Add("@mode", SqlDbType.NVarChar, 30).Value =
                    string.IsNullOrWhiteSpace(modePaiement) ? "CASH" : modePaiement.Trim().ToUpperInvariant();

                cmd.Parameters.Add("@devise", SqlDbType.NVarChar, 10).Value = devise;

                var pm = cmd.Parameters.Add("@montant", SqlDbType.Decimal);
                pm.Precision = 18; pm.Scale = 2; pm.Value = montant;

                cmd.Parameters.Add("@ref", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(note) ? $"REGLEMENT CREDIT VENTE #{idVente}" : note.Trim();

                cmd.Parameters.Add("@statut", SqlDbType.NVarChar, 20).Value = "VALIDE";

                cmd.ExecuteNonQuery();
            }
        }


        // ===================== LIST DEBTS =====================
        public List<CreditRow> GetCreditsOuverts(string filtreNom = null)
        {
            var list = new List<CreditRow>();

            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
SELECT
    cv.IdCredit,
    cv.IdClient,
    ISNULL(cv.IdVente,0) AS IdVente,
    ISNULL(c.Nom,'') AS ClientNom,
    ISNULL(c.Telephone,'') AS Telephone,
    ISNULL(cv.Total,0) AS Total,
    ISNULL(cv.Reste,0) AS Reste,
    ISNULL(cv.Statut,'') AS Statut,
    cv.DateCredit,
    cv.DateEcheance,
    ISNULL(cv.RefVente,'') AS RefVente
FROM dbo.CreditVente cv
LEFT JOIN dbo.Clients c ON c.ID_Clients = cv.IdClient
WHERE
    ISNULL(cv.Reste,0) > 0
    AND UPPER(ISNULL(cv.Statut,'')) NOT IN ('PAYE','PAYÉ','SOLDE','CLOTURE','CLOTURÉ')
    AND (@q IS NULL OR LTRIM(RTRIM(ISNULL(c.Nom,''))) LIKE '%' + @q + '%')
ORDER BY cv.DateCredit DESC;", con))
            {
                cmd.Parameters.Add("@q", SqlDbType.NVarChar, 120).Value =
                    string.IsNullOrWhiteSpace(filtreNom) ? (object)DBNull.Value : filtreNom.Trim();

                con.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new CreditRow
                        {
                            IdCredit = Convert.ToInt32(rd["IdCredit"]),
                            IdClient = Convert.ToInt32(rd["IdClient"]),
                            IdVente = Convert.ToInt32(rd["IdVente"]),
                            ClientNom = rd["ClientNom"].ToString(),
                            Telephone = rd["Telephone"].ToString(),
                            Total = Convert.ToDecimal(rd["Total"]),
                            Reste = Convert.ToDecimal(rd["Reste"]),
                            Statut = rd["Statut"].ToString(),
                            DateCredit = Convert.ToDateTime(rd["DateCredit"]),
                            DateEcheance = rd["DateEcheance"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rd["DateEcheance"]),
                            RefVente = rd["RefVente"].ToString()
                        });
                    }
                }
            }

            return list;
        }

        public void PayCredit(
    int idCredit,
    decimal montant,
    string modePaiement,
    string note,
    SqlConnection con,
    SqlTransaction trans
)
        {
            if (idCredit <= 0) throw new ArgumentException("IdCredit invalide.");
            if (montant <= 0m) throw new ArgumentException("Montant invalide.");

            modePaiement = (modePaiement ?? "").Trim();
            if (string.IsNullOrWhiteSpace(modePaiement)) modePaiement = "CASH";
            note = (note ?? "").Trim();

            // 1) Charger le crédit (CreditVente)
            decimal resteAvant = 0m;
            decimal total = 0m;
            int idVente = 0;
            string statut = "OUVERT";
            string devise = "CDF";

            using (var cmd = new SqlCommand(@"
SELECT TOP 1
    ISNULL(Total,0) AS Total,
    ISNULL(Reste,0) AS Reste,
    ISNULL(Statut,'OUVERT') AS Statut,
    ISNULL(IdVente,0) AS IdVente
FROM dbo.CreditVente
WHERE IdCredit=@id;", con, trans))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCredit;
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) throw new Exception("Crédit introuvable (CreditVente).");
                    total = Convert.ToDecimal(rd["Total"]);
                    resteAvant = Convert.ToDecimal(rd["Reste"]);
                    statut = Convert.ToString(rd["Statut"])?.Trim().ToUpperInvariant();
                    idVente = Convert.ToInt32(rd["IdVente"]);
                }
            }

            if (statut == "PAYE" || statut == "PAYÉ") throw new Exception("Ce crédit est déjà PAYÉ.");
            if (resteAvant <= 0m) throw new Exception("Reste déjà à 0.");

            montant = Math.Round(montant, 2);
            if (montant > resteAvant) throw new Exception($"Montant trop élevé. Reste = {resteAvant:N2}");

            // 2) Devise depuis la vente
            using (var cmdDev = new SqlCommand(@"
SELECT TOP 1 ISNULL(v.Devise,'CDF')
FROM dbo.Vente v
WHERE v.ID_Vente = @v;", con, trans))
            {
                cmdDev.Parameters.Add("@v", SqlDbType.Int).Value = idVente;
                var o = cmdDev.ExecuteScalar();
                if (o != null && o != DBNull.Value) devise = o.ToString().Trim();
            }
            if (string.Equals(devise, "FC", StringComparison.OrdinalIgnoreCase)) devise = "CDF";
            if (string.IsNullOrWhiteSpace(devise)) devise = "CDF";

            // 3) Historique paiement
            using (var cmdIns = new SqlCommand(@"
INSERT INTO dbo.CreditPaiement (IdCredit, DatePaiement, Montant, ModePaiement, Note)
VALUES (@id, GETDATE(), @m, @mode, @note);", con, trans))
            {
                cmdIns.Parameters.Add("@id", SqlDbType.Int).Value = idCredit;
                var pm = cmdIns.Parameters.Add("@m", SqlDbType.Decimal);
                pm.Precision = 18; pm.Scale = 2; pm.Value = montant;
                cmdIns.Parameters.Add("@mode", SqlDbType.NVarChar, 40).Value = modePaiement.Trim().ToUpperInvariant();
                cmdIns.Parameters.Add("@note", SqlDbType.NVarChar, 200).Value =
                    string.IsNullOrWhiteSpace(note) ? (object)DBNull.Value : note;
                cmdIns.ExecuteNonQuery();
            }

            // 4) Update reste/statut
            using (var cmdUp = new SqlCommand(@"
UPDATE dbo.CreditVente
SET Reste = ROUND(Reste - @m, 2),
    Statut = CASE WHEN ROUND(Reste - @m, 2) <= 0 THEN 'PAYE' ELSE 'OUVERT' END
WHERE IdCredit=@id;", con, trans))
            {
                cmdUp.Parameters.Add("@id", SqlDbType.Int).Value = idCredit;
                var pm = cmdUp.Parameters.Add("@m", SqlDbType.Decimal);
                pm.Precision = 18; pm.Scale = 2; pm.Value = montant;
                cmdUp.ExecuteNonQuery();
            }

            // 5) Caisse (PaiementsVente) : tu as déjà InsertPaiementVente(...) dans ce service
            InsertPaiementVente(idVente, montant, modePaiement, note, devise, con, trans);
        }

        public DataTable GetPaiementsCredit(int idCredit)
        {
            var dt = new DataTable();

            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(@"
SELECT IdPaiement, IdCredit, DatePaiement, Montant, ModePaiement, Note
FROM dbo.CreditPaiement
WHERE IdCredit = @id
ORDER BY DatePaiement DESC, IdPaiement DESC;", con))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCredit;
                con.Open();
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }

            return dt;
        }


        // ===================== PAY DEBT =====================
        // Encaissement d'une dette: ajoute CreditPaiement, diminue Reste, et met Statut PAYE si 0
        public void EncaisserPaiementCredit(
    int idCredit,
    int idVente,
    decimal montant,
    string modePaiement,
    string note,
    SqlConnection con,
    SqlTransaction trans)
        {
            if (idCredit <= 0) throw new Exception("IdCredit invalide.");
            if (idVente <= 0) throw new Exception("INVALID idVente");
            if (montant <= 0m) throw new Exception("Montant invalide.");

            decimal resteAvant = 0m;
            decimal total = 0m;
            string devise = "CDF";

            using (var cmdGet = new SqlCommand(@"
SELECT TOP 1 ISNULL(Reste,0) AS Reste, ISNULL(Total,0) AS Total
FROM dbo.CreditVente WHERE IdCredit=@id;", con, trans))
            {
                cmdGet.Parameters.Add("@id", SqlDbType.Int).Value = idCredit;
                using (var rd = cmdGet.ExecuteReader())
                {
                    if (!rd.Read()) throw new Exception("Crédit introuvable (CreditVente).");
                    resteAvant = Convert.ToDecimal(rd["Reste"]);
                    total = Convert.ToDecimal(rd["Total"]);
                }
            }

            // Devise depuis la vente originale si possible
            using (var cmdDev = new SqlCommand(@"
SELECT TOP 1 ISNULL(v.Devise,'CDF')
FROM dbo.CreditVente cv
LEFT JOIN dbo.Vente v ON v.ID_Vente = cv.IdVente
WHERE cv.IdCredit=@id;", con, trans))
            {
                cmdDev.Parameters.Add("@id", SqlDbType.Int).Value = idCredit;
                var o = cmdDev.ExecuteScalar();
                if (o != null && o != DBNull.Value) devise = o.ToString().Trim();
            }

            devise = string.IsNullOrWhiteSpace(devise) ? "CDF" : devise.Trim().ToUpperInvariant();
            if (devise == "FC") devise = "CDF";

            if (resteAvant <= 0m) throw new Exception("Ce crédit est déjà soldé.");
            if (montant > resteAvant) throw new Exception("Montant > Reste. Reste = " + resteAvant.ToString("N2"));

            // 1) Historique paiements crédit
            using (var cmdIns = new SqlCommand(@"
INSERT INTO dbo.CreditPaiement (IdCredit, DatePaiement, Montant, ModePaiement, Note)
VALUES (@id, GETDATE(), @m, @mode, @note);", con, trans))
            {
                cmdIns.Parameters.Add("@id", SqlDbType.Int).Value = idCredit;

                var pm = cmdIns.Parameters.Add("@m", SqlDbType.Decimal);
                pm.Precision = 18; pm.Scale = 2; pm.Value = montant;

                cmdIns.Parameters.Add("@mode", SqlDbType.NVarChar, 40).Value = (modePaiement ?? "CASH").Trim().ToUpperInvariant();
                cmdIns.Parameters.Add("@note", SqlDbType.NVarChar, 200).Value =
                    string.IsNullOrWhiteSpace(note) ? (object)DBNull.Value : note.Trim();

                cmdIns.ExecuteNonQuery();
            }

            // 2) Diminuer reste + statut
            using (var cmdUp = new SqlCommand(@"
UPDATE dbo.CreditVente
SET Reste = ROUND(Reste - @m, 2),
    Statut = CASE WHEN ROUND(Reste - @m, 2) <= 0 THEN 'PAYE' ELSE 'OUVERT' END
WHERE IdCredit=@id;", con, trans))
            {
                cmdUp.Parameters.Add("@id", SqlDbType.Int).Value = idCredit;

                var pm = cmdUp.Parameters.Add("@m", SqlDbType.Decimal);
                pm.Precision = 18; pm.Scale = 2; pm.Value = montant;

                cmdUp.ExecuteNonQuery();
            }

            // 3) Caisse (PaiementsVente) ✅ (avec devise)
            InsertPaiementVente(idVente, montant, modePaiement, note, devise, con, trans);
        }
    }
 }
