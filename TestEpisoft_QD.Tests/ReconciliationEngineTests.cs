using System;
using Xunit;
using TestEpisoft_QD.Models;
using TestEpisoft_QD.Services;
using System.Collections.Generic;

namespace TestEpisoft_QD.Tests
{
    /// <summary>
    /// Tests unitaires pour la classe <see cref="ReconciliationEngine"/>.
    /// Vérifie l'évaluation des correspondances et les utilitaires (tri, ambiguïté, règles).
    /// </summary>
    public class ReconciliationEngineTests
    {
        /// <summary>
        /// Crée une instance de l'engine avec une configuration de tolérances
        /// réutilisable dans plusieurs tests.
        /// </summary>
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

        /// <summary>
        /// Teste qu'une transaction identique sur la date et le montant retourne un score à 100
        /// et que la règle appliquée est "Exact Match".
        /// </summary>
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

            // Act
            var result = engine.Evaluate(bank, acc);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Score);
            Assert.Equal("Exact Match", result.RuleApplied);
        }

        /// <summary>
        /// Même montant mais date décalée d'un jour => score attendu 85 (règle de tolérance date).
        /// </summary>
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

            // Act
            var result = engine.Evaluate(bank, acc);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(85, result.Score);
        }

        /// <summary>
        /// Même date mais montant dans la tolérance spécifique => score attendu 70.
        /// </summary>
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

            // Act
            var result = engine.Evaluate(bank, acc);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(70, result.Score);
        }

        /// <summary>
        /// Cas où ni la date ni le montant ne sont strictement proches mais entrent dans la
        /// tolérance globale => score attendu 55.
        /// </summary>
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

            // Act
            var result = engine.Evaluate(bank, acc);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(55, result.Score);
        }

        /// <summary>
        /// Différence trop importante => pas de correspondance (retourne null).
        /// </summary>
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

            // Act
            var result = engine.Evaluate(bank, acc);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Vérifie que la méthode <see cref="ReconciliationEngine.MarkAmbiguous"/> marque correctement
        /// un candidat comme ambigu lorsque plusieurs correspondances équivalentes existent.
        /// </summary>
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

            // Act: marquer l'ambiguïté à partir d'un candidat parmi la liste
            engine.MarkAmbiguous(candidate1, all);

            // Assert: le candidat devient ambigu et connaît les identifiants candidats possibles
            Assert.True(candidate1.IsAmbiguous);
            Assert.Equal(2, candidate1.CandidateAccountingIds.Count);
        }

        /// <summary>
        /// Vérifie que le tri des candidats ordonne par score décroissant.
        /// </summary>
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

            // Act
            var sorted = engine.SortCandidates(candidates);

            // Assert: le premier élément doit avoir le score le plus élevé
            Assert.Equal(100, sorted[0].Score);
        }

        /// <summary>
        /// Teste la détermination de règle : zéro décalage => règle 1 (Exact Match).
        /// </summary>
        [Fact]
        public void DetermineRule_ExactMatch_ShouldReturnRule1()
        {
            var engine = CreateEngine();

            var rule = engine.DetermineRule(0, 0);

            Assert.Equal(1, rule);
        }

        /// <summary>
        /// Teste la détermination de règle : décalage date dans la tolérance => règle 2.
        /// </summary>
        [Fact]
        public void DetermineRule_DateTolerance_ShouldReturnRule2()
        {
            var engine = CreateEngine();

            var rule = engine.DetermineRule(1, 0);

            Assert.Equal(2, rule);
        }
    }
}