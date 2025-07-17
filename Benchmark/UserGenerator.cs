public static class UserGenerator
{
    public static List<User> GenerateUsers(int count)
    {
        Random random = new(42);
        string[] countries = ["USA", "Canada", "France", "Germany", "UK", "Japan", "Australia"];
        string[] firstNames = ["John", "Jane", "Alice", "Bob", "Charlie", "Diana", "Eva", "Frank"];
        string[] lastNames = ["Smith", "Johnson", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor"];

        List<User> users = new(count);

        for (int i = 0; i < count; i++)
        {
            string firstName = firstNames[random.Next(firstNames.Length)];
            string lastName = lastNames[random.Next(lastNames.Length)];
            string name = $"{firstName} {lastName} {i}";
            int age = random.Next(18, 80);
            string country = countries[random.Next(countries.Length)];

            users.Add(new User(name, age, country));
        }

        return users;
    }
}