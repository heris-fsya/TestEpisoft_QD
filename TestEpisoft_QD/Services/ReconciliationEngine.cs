using System;
using System.Collections.Generic;
using System.Linq;
using TestEpisoft_QD.Models;

namespace TestEpisoft_QD.Services
{


    public class ReconciliationEngine
    {
        private Dictionary<string, int> _config;

        public List<MatchResult> Reconcile(
            List<Transaction> bankTransactions,
            List<Transaction> accountingTransactions)
        {
            var candidates = GenerateCandidates(bankTransactions, accountingTransactions);

            var sortedCandidates = SortCandidates(candidates);

            var results = SelectBestMatches(sortedCandidates);

            return results;
        }

        public ReconciliationEngine(Dictionary<string, int> config)
        {
            _config = config;
        }

        // Générer tous les matchs possibles
        public List<MatchResult> GenerateCandidates(
            List<Transaction> bankTransactions,
            List<Transaction> accountingTransactions)
        {
            var allCandidates = new List<MatchResult>();

            foreach (var bank in bankTransactions)
            {
                foreach (var acc in accountingTransactions)
                {
                    var match = Evaluate(bank, acc);

                    if (match != null)
                        allCandidates.Add(match);
                }
            }

            return allCandidates;
        }

        //  Trier les candidats
        public List<MatchResult> SortCandidates(List<MatchResult> candidates)
        {
            return candidates
                .OrderByDescending(c => c.Score)
                .ThenBy(c => Math.Abs((c.BankTransaction.Date - c.AccountingTransaction.Date).Days))
                .ThenBy(c => Math.Abs(c.BankTransaction.Amount - c.AccountingTransaction.Amount))
                .ThenBy(c => c.AccountingTransaction.Id)
                .ToList();
        }

        // Sélectionner les meilleurs matchs
        public List<MatchResult> SelectBestMatches(List<MatchResult> sortedCandidates)
        {
            var usedBank = new HashSet<string>();
            var usedAccounting = new HashSet<string>();

            var results = new List<MatchResult>();

            foreach (var candidate in sortedCandidates)
            {
                if (usedBank.Contains(candidate.BankTransaction.Id))
                    continue;

                if (usedAccounting.Contains(candidate.AccountingTransaction.Id))
                    continue;

                MarkAmbiguous(candidate, sortedCandidates);

                results.Add(candidate);

                usedBank.Add(candidate.BankTransaction.Id);
                usedAccounting.Add(candidate.AccountingTransaction.Id);
            }

            return results;
        }

        //Détecter les cas ambigus
        public void MarkAmbiguous(MatchResult candidate, List<MatchResult> allCandidates)
        {
            var sameCandidates = allCandidates
                .Where(c =>
                    c.BankTransaction.Id == candidate.BankTransaction.Id &&
                    c.Score == candidate.Score &&
                    Math.Abs((c.BankTransaction.Date - c.AccountingTransaction.Date).Days)
                    == Math.Abs((candidate.BankTransaction.Date - candidate.AccountingTransaction.Date).Days) &&
                    Math.Abs(c.BankTransaction.Amount - c.AccountingTransaction.Amount)
                    == Math.Abs(candidate.BankTransaction.Amount - candidate.AccountingTransaction.Amount))
                .ToList();

            candidate.IsAmbiguous = sameCandidates.Count > 1;

            candidate.CandidateAccountingIds = sameCandidates
                .Select(c => c.AccountingTransaction.Id)
                .ToList();
        }

        // Appliquer les règles
        public MatchResult Evaluate(Transaction bank, Transaction acc)
        {
            var dateDiff = Math.Abs((bank.Date - acc.Date).Days);
            var amountDiff = Math.Abs(bank.Amount - acc.Amount);

            int rule = DetermineRule(dateDiff, amountDiff);

            if (rule == 0)
                return null;

            return CreateMatchFromRule(rule, bank, acc);
        }

        //  Calcule de la règle
        public int DetermineRule(int dateDiff, decimal amountDiff)
        {
            if (dateDiff == 0 && amountDiff == 0)
                return 1;

            else if (amountDiff == 0 && dateDiff <= _config["DATE_TOLERANCE_AMOUNT"])
                return 2;

            else if (dateDiff == 0 && amountDiff <= _config["AMOUNT_TOLERANCE_DATE"])
                return 3;

            else if (dateDiff <= _config["DATE_TOLERANCE_GLOBAL"] &&
         amountDiff <= _config["AMOUNT_TOLERANCE_GLOBAL"])
                return 4;

            return 0;
        }

        // Créer le résultat selon la règle
        public MatchResult CreateMatchFromRule(int rule, Transaction bank, Transaction acc)
        {
            switch (rule)
            {
                case 1:
                    return CreateMatch(bank, acc, 100, "Exact Match");

                case 2:
                    return CreateMatch(bank, acc, 85, "Amount match ±1 day");

                case 3:
                    return CreateMatch(bank, acc, 70, "Date match ±5 amount");

                case 4:
                    return CreateMatch(bank, acc, 55, "Tolerance date + amount");

                default:
                    return null;
            }
        }

        //  Création match 
        public MatchResult CreateMatch(Transaction bank, Transaction acc, int score, string rule)
        {
            return new MatchResult
            {
                BankTransaction = bank,
                AccountingTransaction = acc,
                Score = score,
                RuleApplied = rule
            };
        }
    }
}