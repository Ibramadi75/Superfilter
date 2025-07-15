namespace Database.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MoneyAmount { get; set; }
    public DateTime? BornDate { get; set; }
    public DateTimeOffset? RegistrationDate { get; set; }
    public DateTimeOffset LastLoginDate { get; set; }
    
    // New numeric properties for testing
    public long LongValue { get; set; }
    public long? NullableLongValue { get; set; }
    public decimal DecimalValue { get; set; }
    public decimal? NullableDecimalValue { get; set; }
    public double DoubleValue { get; set; }
    public double? NullableDoubleValue { get; set; }
    public float FloatValue { get; set; }
    public float? NullableFloatValue { get; set; }

    // Relations
    public int CarId { get; set; }
    public Car? Car { get; set; }
    public int HouseId { get; set; }
    public House? House { get; set; }
}

public class Car
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset? ManufactureDate { get; set; }

    public User User { get; set; }
    public int UserId { get; set; }

    // Relations
    public Brand? Brand { get; set; }
    public int? BrandId { get; set; } // Clé étrangère vers Brand
}

public class Brand
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Rate { get; set; }

    // Relation inverse
    public ICollection<Car> Cars { get; set; } = new List<Car>();
}

public class City
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int MayorId { get; set; }
    public User Mayor { get; set; }
}

public class House
{
    public int Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public int? Size { get; set; }

    // Relations
    public User User { get; set; } = null!;
    public int UserId { get; set; }

    public City City { get; set; } = null!;
    public int CityId { get; set; }
}