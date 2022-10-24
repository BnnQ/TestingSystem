namespace TestingSystem.Models
{
    public class User
    {
        public string Name { get; set; }
        public string HashedPassword { get; set; }


        public User(string name, string hashedPassword)
        {
            Name = name;
            HashedPassword = hashedPassword;
        }

    }
}