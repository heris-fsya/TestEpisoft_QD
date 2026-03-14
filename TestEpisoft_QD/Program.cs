using System;
using System.Collections.Generic;
using TestEpisoft.Models;
using TestEpisoft.Services;

namespace TestEpisoft
{
    class Program
    {
        static void Main(string[] args)
        {
            var reader = new CsvReader();

            var bankTransactions = reader.ReadTransactions("Data/bank.csv", "Bank");
            var accountingTransactions = reader.ReadTransactions("Data/accounting.csv", "Accounting");

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
        }
    }
}