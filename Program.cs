using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Models;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
var builder = WebApplication.CreateBuilder(args);
//string? connectionString = builder.Configuration.GetConnectionString("MSSQLConnection");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options=>
                {
                    options.TokenValidationParameters=new TokenValidationParameters
                    {
                        ValidateIssuer=true,
                        ValidateAudience=true,
                        ValidateLifetime=true,
                        ValidateIssuerSigningKey=true,
                        ValidIssuer=builder.Configuration["Jwt:Issuer"],
                        ValidAudience=builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))

                    };
                     options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine("Authentication failed: " + context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine("Token validated successfully.");
                            return Task.CompletedTask;
                        }
                    };
                });
builder.Services.AddAuthorization();
builder.Services.AddDbContext<ExpenseTrackerDB>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQLConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,               // Number of retry attempts
            maxRetryDelay: TimeSpan.FromSeconds(10), // Delay between retries
            errorNumbersToAdd: null        // Add specific SQL error numbers to retry
        )
    )
);


builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin",
                builder => builder.WithOrigins("*")
                                  .AllowAnyHeader()
                                  .AllowAnyMethod());
        });
builder.Services.AddSwaggerGen(c =>
     {
       c.SwaggerDoc("v1", new OpenApiInfo { Title = "Todo API", Description = "Keep track of your tasks", Version = "v1" });
     });

var app = builder.Build();
app.UseCors("AllowSpecificOrigin");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
   app.UseSwaggerUI(c =>
    {
      c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API V1");
    });
} // end of if (app.Environment.IsDevelopment()) block
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/Expense/{id}",  (ExpenseTrackerDB db, Guid id) =>  
db.expense.Where(e=>e.UserId==id).ToList()
).RequireAuthorization();

app.MapPost("/Expense", async (ExpenseTrackerDB db, Expense expense) =>
{
    await db.expense.AddAsync(expense);
    await db.SaveChangesAsync();
    return Results.Ok();
}).RequireAuthorization();

app.MapPut("/Expense/{id}", async (ExpenseTrackerDB db, Expense updatedExpense, Guid id) =>
{
    //Console.WriteLine(id);
    var expense = await db.expense.FindAsync(id);
    //Console.WriteLine(updatedExpense.);
    if (expense is null)
        return Results.NotFound();
    expense.ExpenseType = updatedExpense.ExpenseType;
    expense.ExpenseName = updatedExpense.ExpenseName;
    expense.BrandName = updatedExpense.BrandName;
    expense.QuantityType = updatedExpense.QuantityType;
    expense.Quantity = updatedExpense.Quantity;
    expense.Amount = updatedExpense.Amount;
    expense.RemindMe = updatedExpense.RemindMe;
    expense.UserId = updatedExpense.UserId;
    await db.SaveChangesAsync();
    //Console.WriteLine(expense.UserId+"  "+updatedExpense.UserId);
    return Results.Ok();

}).RequireAuthorization();
app.MapDelete("/Expense/{id}", async (ExpenseTrackerDB db, Guid id) =>
{
    var expense = await db.expense.FindAsync(id);
    if (expense is null)
        return Results.NotFound();
    db.expense.Remove(expense);
    await db.SaveChangesAsync();
    return Results.Ok();
}).RequireAuthorization();

app.Run();
