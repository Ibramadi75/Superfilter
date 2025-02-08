using Database.Models;

namespace Database;

public class Fake
{
    public List<User> Users { get; set; }
    
    public IQueryable<User> GetUsers()
    {
        return Users.AsQueryable();
    }
    
    public void GenerateUsers(int numberOfUsers)
    {
        var users = new List<User>();
        var carIdCounter = 1;

        for (int i = 1; i <= numberOfUsers; i++)
        {
            var user = new User
            {
                Id = i,
                Name = $"User {i}",
                MoneyAmount = 1000 + i * 500,
                Car = new Car
                {
                    Id = carIdCounter++,
                    Name = i % 2 == 0 ? "Ferrari" : "Lamborghini"
                }
            };
            users.Add(user);
        }

        Users = users;
    }
}