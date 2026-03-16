using System.Collections.Generic;

namespace TestEpisoft_QD.Models
{
    public class MatchResult
          
    {
        public Transaction BankTransaction { get; set; }

        public Transaction AccountingTransaction { get; set; }

        public int Score { get; set; }

        public string RuleApplied { get; set; }

        public List<string> CandidateAccountingIds { get; set; }

        public bool IsAmbiguous { get; set; }
    }
}