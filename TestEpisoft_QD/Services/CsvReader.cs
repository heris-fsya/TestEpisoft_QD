using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TestEpisoft_QD.Models;

namespace TestEpisoft_QD.Services
{

    public class CsvReader
    {
        public List<Transaction> ReadTransactions(string path, string source)
        {
            var transactions = new List<Transaction>();

            if (!File.Exists(path))
            {
                Console.WriteLine($"Erreur : fichier introuvable -> {path}");
                return transactions;
            }

            var lines = File.ReadAllLines(path);

            if (lines.Length == 0)
            {
                Console.WriteLine($"Erreur : fichier vide -> {path}");
                return transactions;
            }

            // V�rification du header
            if (!lines[0].Contains("Date") || !lines[0].Contains("Amount"))
            {
                Console.WriteLine($"Erreur : header invalide dans {path}");
                return transactions;
            }

            int idCounter = 1;

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var columns = line.Split(',');

                if (columns.Length < 3)
                {
                    Console.WriteLine($"Ligne invalide (colonnes manquantes) : {line}");
                    continue;
                }

                // Date
                if (!DateTime.TryParse(columns[0], out DateTime date))
                {
                    Console.WriteLine($"Date invalide : {columns[0]}");
                    continue;
                }

                // Montant
                if (!decimal.TryParse(columns[2], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount))
                {
                    Console.WriteLine($"Montant invalide : {columns[2]}");
                    continue;
                }

                var transaction = new Transaction
                {
                    Id = $"{source}_{idCounter}",
                    Date = date,
                    Description = columns[1],
                    Amount = amount,
                    Source = source
                };

                transactions.Add(transaction);
                idCounter++;
            }

            return transactions;
        }
    }
}