using System;

namespace TestEpisoft.Models
{
    public class Transaction
    {
        public string Id { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public decimal Amount { get; set; }

        public string Source { get; set; } // Bank ou Accounting
    }
}