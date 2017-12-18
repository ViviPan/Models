using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    internal class Account
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }

        public User User { get; set; }

        public override string ToString()
        {
            return $"User: {User.Username}\n| Transaction date: {TransactionDate}\t| Amount: {Amount:C}";
        }
    }
}
