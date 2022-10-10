namespace TestingSystem.Models
{
    public class Student : User
    {
        public int Id { get; set; }

        public Student(string encryptedName, string encryptedPassword) : base(encryptedName, encryptedPassword)
        {
            //empty
        }
        public Student(int id, string encryptedName, string encryptedPassword) : this(encryptedName, encryptedPassword)
        {
            Id = id;
        }


    }
}