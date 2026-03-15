using System;
using Xunit;
using TestEpisoft_QD.Models;
using TestEpisoft_QD.Services;

namespace TestEpisoft_QD.Tests
{
    public class ReconciliationEngineTests
    {
        [Fact]
        public void ExactMatch_ShouldReturnScore100()
        {
            // Arrange
            var engine = new ReconciliationEngine();

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

            // Act
            var result = engine.Evaluate(bank, acc);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Score);
            Assert.Equal("Exact Match", result.RuleApplied);
        }
    }

        [Fact]
        public void MontantIdentique_DatePlusUnJour_Score85()
        {
            // Arrange
            var engine = new ReconciliationEngine();

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

            // Act
            var result = engine.Evaluate(bank, acc);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(85, result.Score);
        }

    }

            [Fact]
public void DateIdentique_MontantplusCinqMax_Score70()
{
    // Arrange
    var engine = new ReconciliationEngine();

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

    // Act
    var result = engine.Evaluate(bank, acc);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(70, result.Score);
}

    }


        [Fact]
public void MontantplusCinqMax_DatePlusDeuxJour_Score55()
{
    // Arrange
    var engine = new ReconciliationEngine();

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

    // Act
    var result = engine.Evaluate(bank, acc);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(55, result.Score);
}

    }

    [Fact]
public void DifferenceTropGrande_DoiventPasMatcher()
{
    var engine = new ReconciliationEngine();

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
    var engine = new ReconciliationEngine();

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
    var engine = new ReconciliationEngine();

    var bank = new Transaction { Id = "B1", Date = DateTime.Now, Amount = -50 };

    var candidates = new List<MatchResult>
            {
                new MatchResult { BankTransaction = bank, AccountingTransaction = new Transaction{Id="A1"}, Score = 50 },
                new MatchResult { BankTransaction = bank, AccountingTransaction = new Transaction{Id="A2"}, Score = 100 }
            };

    var sorted = engine.SortCandidates(candidates);

    Assert.Equal(100, sorted[0].Score);
}

public void DetermineRule_ExactMatch_ReturnRule1()
{
    var engine = new ReconciliationEngine();

    var rule = engine.DetermineRule(0, 0);

    Assert.Equal(1, rule);
}


[Fact]
public void DetermineRule_DateTolerance_ReturnRule2()
{
    var engine = new ReconciliationEngine();

    var rule = engine.DetermineRule(1, 0);

    Assert.Equal(2, rule);
}

}