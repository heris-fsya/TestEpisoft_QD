using System;
using Xunit;
using TestEpisoft_QD.Models;
using TestEpisoft_QD.Services;
using System.Collections.Generic;

namespace TestEpisoft_QD.Tests
{
    public class ReconciliationEngineTests
    {
        private ReconciliationEngine CreateEngine()
        {
            var config = new Dictionary<string, int>
            {
                { "DATE_TOLERANCE_AMOUNT", 1 },
                { "AMOUNT_TOLERANCE_DATE", 5 },
                { "DATE_TOLERANCE_GLOBAL", 2 },
                { "AMOUNT_TOLERANCE_GLOBAL", 5 }
            };

            return new ReconciliationEngine(config);
        }

        [Fact]
        public void ExactMatch_ShouldReturnScore100()
        {
            var engine = CreateEngine();

            var bank = new Transaction
            {
                Id = "B1",
                Date = new DateTime(2023, 10, 1),
                Amount = -42.99m
            };

            var acc = new Transaction
            {
                Id = "A1",
                Date = new DateTime(2023, 10, 1),
                Amount = -42.99m
            };

            var result = engine.Evaluate(bank, acc);

            Assert.NotNull(result);
            Assert.Equal(100, result.Score);
            Assert.Equal("Exact Match", result.RuleApplied);
        }

        [Fact]
        public void SameAmount_DatePlusMinusOneDay_ShouldReturnScore85()
        {
            var engine = CreateEngine();

            var bank = new Transaction
            {
                Id = "B1",
                Date = new DateTime(2023, 10, 2),
                Amount = -42.99m
            };

            var acc = new Transaction
            {
                Id = "A1",
                Date = new DateTime(2023, 10, 1),
                Amount = -42.99m
            };

            var result = engine.Evaluate(bank, acc);

            Assert.NotNull(result);
            Assert.Equal(85, result.Score);
        }

        [Fact]
        public void SameDate_AmountTolerance_ShouldReturnScore70()
        {
            var engine = CreateEngine();

            var bank = new Transaction
            {
                Id = "B1",
                Date = new DateTime(2023, 10, 2),
                Amount = -42.99m
            };

            var acc = new Transaction
            {
                Id = "A1",
                Date = new DateTime(2023, 10, 2),
                Amount = -47.99m
            };

            var result = engine.Evaluate(bank, acc);

            Assert.NotNull(result);
            Assert.Equal(70, result.Score);
        }

        [Fact]
        public void GlobalTolerance_ShouldReturnScore55()
        {
            var engine = CreateEngine();

            var bank = new Transaction
            {
                Id = "B1",
                Date = new DateTime(2023, 10, 3),
                Amount = -42.99m
            };

            var acc = new Transaction
            {
                Id = "A1",
                Date = new DateTime(2023, 10, 1),
                Amount = -47.99m
            };

            var result = engine.Evaluate(bank, acc);

            Assert.NotNull(result);
            Assert.Equal(55, result.Score);
        }

        [Fact]
        public void TooLargeDifference_ShouldNotMatch()
        {
            var engine = CreateEngine();

            var bank = new Transaction
            {
                Id = "B1",
                Date = new DateTime(2023, 10, 1),
                Amount = -100
            };

            var acc = new Transaction
            {
                Id = "A1",
                Date = new DateTime(2023, 10, 10),
                Amount = -50
            };

            var result = engine.Evaluate(bank, acc);

            Assert.Null(result);
        }

        [Fact]
        public void MarkAmbiguous_ShouldDetectAmbiguity()
        {
            var engine = CreateEngine();

            var bank = new Transaction { Id = "B1", Date = new DateTime(2023, 10, 1), Amount = -50 };
            var acc1 = new Transaction { Id = "A1", Date = new DateTime(2023, 10, 1), Amount = -50 };
            var acc2 = new Transaction { Id = "A2", Date = new DateTime(2023, 10, 1), Amount = -50 };

            var candidate1 = new MatchResult
            {
                BankTransaction = bank,
                AccountingTransaction = acc1,
                Score = 100
            };

            var candidate2 = new MatchResult
            {
                BankTransaction = bank,
                AccountingTransaction = acc2,
                Score = 100
            };

            var all = new List<MatchResult> { candidate1, candidate2 };

            engine.MarkAmbiguous(candidate1, all);

            Assert.True(candidate1.IsAmbiguous);
            Assert.Equal(2, candidate1.CandidateAccountingIds.Count);
        }

        [Fact]
        public void SortCandidates_ShouldOrderByScore()
        {
            var engine = CreateEngine();

            var bank = new Transaction { Id = "B1", Date = DateTime.Now, Amount = -50 };

            var candidates = new List<MatchResult>
            {
                new MatchResult { BankTransaction = bank, AccountingTransaction = new Transaction{Id="A1"}, Score = 50 },
                new MatchResult { BankTransaction = bank, AccountingTransaction = new Transaction{Id="A2"}, Score = 100 }
            };

            var sorted = engine.SortCandidates(candidates);

            Assert.Equal(100, sorted[0].Score);
        }

        [Fact]
        public void DetermineRule_ExactMatch_ShouldReturnRule1()
        {
            var engine = CreateEngine();

            var rule = engine.DetermineRule(0, 0);

            Assert.Equal(1, rule);
        }

        [Fact]
        public void DetermineRule_DateTolerance_ShouldReturnRule2()
        {
            var engine = CreateEngine();

            var rule = engine.DetermineRule(1, 0);

            Assert.Equal(2, rule);
        }
    }
}