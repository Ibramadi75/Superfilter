namespace Database.Models;

public class User
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int MoneyAmount { get; init; }
    public DateTime? BornDate { get; init; }
    public DateTimeOffset? RegistrationDate { get; init; }
    public DateTimeOffset LastLoginDate { get; init; }
    
    // New numeric properties for testing
    public long LongValue { get; init; }
    public long? NullableLongValue { get; init; }
    public decimal DecimalValue { get; init; }
    public decimal? NullableDecimalValue { get; init; }
    public double DoubleValue { get; init; }
    public double? NullableDoubleValue { get; init; }
    public float FloatValue { get; init; }
    public float? NullableFloatValue { get; init; }

    // Relations
    public int CarId { get; init; }
    public Car? Car { get; init; }
    public int HouseId { get; init; }
    public House? House { get; init; }
}

public class Car
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset? ManufactureDate { get; init; }

    public User? User { get; init; }
    public int UserId { get; init; }

    // Relations
    public Brand? Brand { get; init; }
    public int? BrandId { get; init; } // Clé étrangère vers Brand
}

public class Brand
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Rate { get; init; }

    // Relation inverse
    public ICollection<Car> Cars { get; init; } = new List<Car>();
}

public class City
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;

    public int MayorId { get; init; }
    public User? Mayor { get; init; }
}

public class House
{
    public int Id { get; init; }
    public string Address { get; init; } = string.Empty;
    public int? Size { get; init; }

    // Relations
    public User User { get; init; } = null!;
    public int UserId { get; init; }

    public City City { get; init; } = null!;
    public int CityId { get; init; }
}