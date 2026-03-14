using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TestEpisoft_QD.Models;

namespace TestEpisoft_QD.Services
{
    public class ReportGenerator
    {
        public void GenerateReport(
            string path,
            List<Transaction> bankTransactions,
            List<Transaction> accountingTransactions,
            List<MatchResult> matches)
        {
            int totalBank = bankTransactions.Count;
            int totalAccounting = accountingTransactions.Count;
            int matched = matches.Count;

            int unmatchedBank = totalBank - matched;
            int unmatchedAccounting = totalAccounting - matched;

            int weakMatches = matches.Count(m => m.Score < 85);

            var ambiguous = matches.Where(m => m.IsAmbiguous).ToList();

            var report = new List<string>();

            report.Add("=== RECONCILIATION REPORT ===");
            report.Add("");

            report.Add($"Nb total banque : {totalBank}");
            report.Add($"Nb total compta : {totalAccounting}");
            report.Add($"Nb matchés : {matched}");
            report.Add($"Nb non matchés banque : {unmatchedBank}");
            report.Add($"Nb non matchés compta : {unmatchedAccounting}");
            report.Add($"Nb de matchs faibles (score < 85) : {weakMatches}");

            report.Add("");
            report.Add("=== CAS AMBIGUS ===");

            if (ambiguous.Count == 0)
            {
                report.Add("Aucun");
            }
            else
            {
                foreach (var a in ambiguous)
                {
                    report.Add($"BankId : {a.BankTransaction.Id} -> AccountingId choisi : {a.AccountingTransaction.Id}");
                }
            }

            File.WriteAllLines(path, report, Encoding.UTF8);
        }
    }
}