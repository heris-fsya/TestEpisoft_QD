using System;
using System.Collections.Generic;
using TestEpisoft_QD.Models;
using TestEpisoft_QD.Services;

namespace TestEpisoft_QD
{
    class Program
    {
        static void Main(string[] args)
        {
            var reader = new CsvReader();

            var bankTransactions = reader.ReadTransactions("Data/bank.csv", "Bank");
            var accountingTransactions = reader.ReadTransactions("Data/accounting.csv", "Accounting");
            var configLoader = new RuleConfigLoader();
            var config = configLoader.Load("rules.config");

            var engine = new ReconciliationEngine(config);

            var matches = engine.Reconcile(bankTransactions, accountingTransactions);

            Console.WriteLine("=== BANK TRANSACTIONS ===");

            foreach (var t in bankTransactions)
            {
                Console.WriteLine($"{t.Id} | {t.Date:yyyy-MM-dd} | {t.Description} | {t.Amount} | {t.Source}");
            }

            Console.WriteLine();
            Console.WriteLine("=== ACCOUNTING TRANSACTIONS ===");

            foreach (var t in accountingTransactions)
            {
                Console.WriteLine($"{t.Id} | {t.Date:yyyy-MM-dd} | {t.Description} | {t.Amount} | {t.Source}");
            }

            Console.WriteLine();
            Console.WriteLine($"Total Bank : {bankTransactions.Count}");
            Console.WriteLine($"Total Accounting : {accountingTransactions.Count}");

            Console.ReadLine();

            Console.WriteLine();
            Console.WriteLine("=== MATCH RESULTS ===");

            foreach (var m in matches)
            {
                Console.WriteLine(
                    $"{m.BankTransaction.Id} -> {m.AccountingTransaction.Id} | " +
                    $"Score: {m.Score} | Rule: {m.RuleApplied} | Ambiguous: {m.IsAmbiguous}");
            }

            Console.WriteLine();
            Console.WriteLine($"Total Matches : {matches.Count}");


            var exporter = new MatchExporter();
            exporter.ExportMatches("matches.csv", matches);

            var report = new ReportGenerator();
            report.GenerateReport(
                "report.txt",
                bankTransactions,
                accountingTransactions,
                matches);

            Console.WriteLine("Fichiers g�n�r�s : matches.csv et report.txt");
        }
    }
}