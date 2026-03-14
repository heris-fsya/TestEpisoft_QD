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
            var results = new List<MatchResult>();

            var availableAccounting = new List<Transaction>(accountingTransactions);

            foreach (var bank in bankTransactions)
            {
                var candidates = new List<MatchResult>();

                foreach (var acc in availableAccounting)
                {
                    var match = Evaluate(bank, acc);

                    if (match != null)
                        candidates.Add(match);
                }

                if (candidates.Count == 0)
                    continue;

                var best = candidates
                    .OrderByDescending(c => c.Score)
                    .ThenBy(c => Math.Abs((c.BankTransaction.Date - c.AccountingTransaction.Date).Days))
                    .ThenBy(c => Math.Abs(c.BankTransaction.Amount - c.AccountingTransaction.Amount))
                    .ThenBy(c => c.AccountingTransaction.Id)
                    .ToList();

                

                var chosen = best.First();

                var bestScore = chosen.Score;
                var bestDateDiff = Math.Abs((chosen.BankTransaction.Date - chosen.AccountingTransaction.Date).Days);
                var bestAmountDiff = Math.Abs(chosen.BankTransaction.Amount - chosen.AccountingTransaction.Amount);

                var ambiguousCandidates = best
                    .Where(c =>
                        c.Score == bestScore &&
                        Math.Abs((c.BankTransaction.Date - c.AccountingTransaction.Date).Days) == bestDateDiff &&
                        Math.Abs(c.BankTransaction.Amount - c.AccountingTransaction.Amount) == bestAmountDiff)
                    .ToList();
                chosen.CandidateAccountingIds = ambiguousCandidates
                .Select(c => c.AccountingTransaction.Id)
                .ToList();

                if (ambiguousCandidates.Count > 1)
                {
                    chosen.IsAmbiguous = true;
                }
            

                results.Add(chosen);

                availableAccounting.Remove(chosen.AccountingTransaction);
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