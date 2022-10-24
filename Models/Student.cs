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

        public Student(string name, string hashedPassword, string fullName) : base(name, hashedPassword)
        {
            FullName = fullName;
        }


    }
}