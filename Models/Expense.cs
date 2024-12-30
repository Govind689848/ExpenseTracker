using System;
namespace ExpenseTracker.Models;
public class Expense{
    public Guid? Id {get;set;}
        public string? ExpenseType {get;set;}
        public string? ExpenseName{get;set;}
        public string? BrandName { get; set; }
        public string? QuantityType { get; set; }
        public int Quantity { get; set; }
        public double Amount { get; set; }
        public DateTime DOP { get; set; }
        public bool IsEssentialProduct { get; set; }
        public int RemindMe { get; set; }
        public Guid? UserId { get; set; }
}
