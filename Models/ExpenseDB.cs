using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
namespace ExpenseTracker.Models;
public class ExpenseTrackerDB:DbContext
{
    public ExpenseTrackerDB(DbContextOptions options):base(options)
    {

    }
    public DbSet<Expense> expense { get; set; }
    // public DbSet<User> user {get;set;}
    // public DbSet<Role> role{get;set;}
}