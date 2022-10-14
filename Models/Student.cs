using CommunityToolkit.Mvvm.ComponentModel;

namespace TestingSystem.Models
{
    [ObservableObject]
    public partial class Student : User
    {
        public int Id { get; set; }

        private string fullName = null!;
        public string FullName
        {
            get => fullName;
            set
            {
                if (fullName != value)
                {
                    fullName = value;
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        public Student(string encryptedName, string encryptedPassword, string fullName) : base(encryptedName, encryptedPassword)
        {
            FullName = fullName;
        }
        public Student(int id, string encryptedName, string encryptedPassword, string fullName) 
            : this(encryptedName, encryptedPassword, fullName)
        {
            Id = id;
        }


    }
}