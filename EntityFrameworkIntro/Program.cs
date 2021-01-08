using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

var factory = new CookbookContextFactory();
using var context = factory.CreateDbContext(args);

Console.WriteLine("Add Porridge for breakfast");

var porridge = new Dish { Title = "Breakfast Porridge", Notes = "This is soooo goood", Stars = 4 };
context.Dishes.Add(porridge);
await context.SaveChangesAsync();

Console.WriteLine($"Added Porridge (id = {porridge.Id}) succesfully");

Console.WriteLine("Checking stars for Porridge");
var dishes = await context.Dishes
    .Where(d => d.Title.Contains("Porridge"))
    .ToListAsync(); // LINQ -> SQL
if (dishes.Count != 1) Console.Error.WriteLine("Something really bad happened. Porridge disappeared :-(");
Console.WriteLine($"Porridge was {dishes[0].Stars} stars");

Console.WriteLine("Change Porridge stars to 5");
porridge.Stars = 5;
await context.SaveChangesAsync();
Console.WriteLine("Changed stars"); 

Console.WriteLine("Removing Porridge from database");
context.Dishes.Remove(porridge);
await context.SaveChangesAsync();

Console.WriteLine("Porridge removed");

//Create the model class

 class Dish
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public int? Stars { get; set; }

    public List<DishIngredient> Ingredients { get; set; } = new();
}

class DishIngredient
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(50)]
    public string UnitOfMeasure { get; set; } = string.Empty;

    [Column(TypeName = "decimal")]
    public decimal Amount { get; set; }

    public Dish? Dish { get; set; }

    public int DishId { get; set; }
}

//Create database context

class CookbookContext : DbContext
{
    public DbSet<Dish> Dishes { get; set; }
    
    public DbSet<DishIngredient> Ingredients { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public CookbookContext(DbContextOptions<CookbookContext> options)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        : base(options)
    {}
}

//Create the factory

class CookbookContextFactory : IDesignTimeDbContextFactory<CookbookContext>
{
    public CookbookContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        var optionsBuilder = new DbContextOptionsBuilder<CookbookContext>();
        optionsBuilder
            // Uncomment the following line if you want to print generated
            // SQL statements on the console.
            .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            .UseSqlServer(configuration["ConnectionStrings:DefaultConnection"]);

        return new CookbookContext(optionsBuilder.Options);
    }
}