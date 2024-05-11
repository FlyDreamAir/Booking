using FlyDreamAir.Data.Db;
using Microsoft.EntityFrameworkCore;

namespace FlyDreamAir.Data.Seeders;

public class AddOnsSeeder
{
    private readonly ApplicationDbContext _dbContext;

    public AddOnsSeeder(
        DbContextOptions<ApplicationDbContext> dbContextOptions
    )
    {
        _dbContext = new ApplicationDbContext(dbContextOptions);
    }

    public async Task Seed()
    {
        // Luggage
        await UpdateLuggage(20, 50);
        await UpdateLuggage(40, 80);

        // Meals
        await UpdateMeal("Curry", "Vegan", 10.0m, new Uri("https://www.justonecookbook.com/wp-content/uploads/2020/02/Vegetarian-Curry-4360-I.jpg"));
        await UpdateMeal("Banh mi", "Beef", 9.0m, new Uri("https://images.getrecipekit.com/20230813061131-andy-20cooks-20-20roast-20pork-20banh-20mi.jpg?aspect_ratio=4:3&quality=90&"));

        await _dbContext.SaveChangesAsync();
    }

    private async Task UpdateLuggage(
        decimal amount,
        decimal price
    )
    {
        await _dbContext.Luggage.Where(l => l.Amount == amount)
            .ExecuteDeleteAsync();

        _dbContext.Entry(new Luggage()
        {
            Name = $"Additional Luggage - {amount}kg",
            Type = nameof(Luggage),
            Price = price,
            ImageSrc = new Uri("https://media.istockphoto.com/id/474510508/photo/purple-suitcase-isolated-on-white-background.jpg?s=612x612&w=0&k=20&c=vUvZv4JfS43vodCyXJb_JlH9mKbPcBtdgr0mmrjjejY="),
            Amount = amount
        }).State = EntityState.Added;
    }

    private async Task UpdateMeal(
        string dishName,
        string description,
        decimal price,
        Uri imageSrc
    )
    {
        await _dbContext.Meals.Where(m => m.DishName == dishName)
            .ExecuteDeleteAsync();

        _dbContext.Entry(new Meal()
        {
            Name = $"Hot Meal - {dishName}",
            Type = nameof(Meal),
            Price = price,
            ImageSrc = imageSrc,
            DishName = dishName,
            Description = description
        }).State = EntityState.Added;
    }
}
