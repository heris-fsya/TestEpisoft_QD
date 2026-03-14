using System.Collections.Generic;
using System.IO;
using System.Text;
using TestEpisoft_QD.Models;

namespace TestEpisoft_QD.Services
{
    public class MatchExporter
    {
        public void ExportMatches(string path, List<MatchResult> matches)
        {
            var lines = new List<string>();

            lines.Add("BankId,AccountingId,Score,RuleApplied");

            foreach (var m in matches)
            {
                lines.Add($"{m.BankTransaction.Id},{m.AccountingTransaction.Id},{m.Score},{m.RuleApplied}");
            }

            File.WriteAllLines(path, lines, Encoding.UTF8);
        }
    }
}