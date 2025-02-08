namespace Database.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MoneyAmount { get; set; }
    public Car Car { get; set; }
}

public class Car
{
    public int Id { get; set; }
    public string Name { get; set; }
}