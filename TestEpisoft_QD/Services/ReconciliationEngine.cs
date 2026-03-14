using System;
using System.Collections.Generic;
using System.Linq;
using TestEpisoft_QD.Models;

namespace TestEpisoft_QD.Services
{
    public class ReconciliationEngine
    {
        public List<MatchResult> Reconcile(
            List<Transaction> bankTransactions,
            List<Transaction> accountingTransactions)
        {
            var allCandidates = new List<MatchResult>();

            // Générer tous les candidats
            foreach (var bank in bankTransactions)
            {
                foreach (var acc in accountingTransactions)
                {
                    var match = Evaluate(bank, acc);

                    if (match != null)
                        allCandidates.Add(match);
                }
            }

            // Tri global
            var sortedCandidates = allCandidates
                .OrderByDescending(c => c.Score)
                .ThenBy(c => Math.Abs((c.BankTransaction.Date - c.AccountingTransaction.Date).Days))
                .ThenBy(c => Math.Abs(c.BankTransaction.Amount - c.AccountingTransaction.Amount))
                .ThenBy(c => c.AccountingTransaction.Id)
                .ToList();

            var usedBank = new HashSet<string>();
            var usedAccounting = new HashSet<string>();

            var results = new List<MatchResult>();

            foreach (var candidate in sortedCandidates)
            {
                if (usedBank.Contains(candidate.BankTransaction.Id))
                    continue;

                if (usedAccounting.Contains(candidate.AccountingTransaction.Id))
                    continue;

                // trouver tous les candidats équivalents
                var sameCandidates = sortedCandidates
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

                results.Add(candidate);

                usedBank.Add(candidate.BankTransaction.Id);
                usedAccounting.Add(candidate.AccountingTransaction.Id);
            }

            return results;
        }

        private MatchResult Evaluate(Transaction bank, Transaction acc)
        {
            var dateDiff = Math.Abs((bank.Date - acc.Date).Days);
            var amountDiff = Math.Abs(bank.Amount - acc.Amount);

            int rule = 0;

            if (dateDiff == 0 && amountDiff == 0)
                rule = 1;
            else if (amountDiff == 0 && dateDiff <= 1)
                rule = 2;
            else if (dateDiff == 0 && amountDiff <= 5)
                rule = 3;
            else if (dateDiff <= 2 && amountDiff <= 5)
                rule = 4;
            else
                return null;

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

        private MatchResult CreateMatch(Transaction bank, Transaction acc, int score, string rule)
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