namespace TestingSystem.Models
{
    public class User
    {
        public string EncryptedName { get; set; }
        public string EncryptedPassword { get; set; }


        public User(string encryptedName, string encryptedPassword)
        {
            EncryptedName = encryptedName;
            EncryptedPassword = encryptedPassword;
        }

    }
}