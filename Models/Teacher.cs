using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TestingSystem.Models
{
    [ObservableObject]
    public partial class Teacher : User
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

        private ICollection<Test> ownedTests = null!;
        public virtual ICollection<Test> OwnedTests
        {
            get => ownedTests;
            set
            {
                if (ownedTests != value)
                {
                    if (value is ObservableCollection<Test>)
                        ownedTests = value;
                    else
                        ownedTests = new ObservableCollection<Test>(value);

                    OnPropertyChanged(nameof(OwnedTests));
                }
            }
        }
        

        public Teacher(string encryptedName, string encryptedPassword, string fullName) : base(encryptedName, encryptedPassword)
        {
            OwnedTests = new ObservableCollection<Test>();
            FullName = fullName;
        }
        public Teacher(string encryptedName, string encryptedPassword, string fullName, ICollection<Test> ownedTests) 
            : this(encryptedName, encryptedPassword, fullName)
        {
            OwnedTests = ownedTests;
        }
        public Teacher(int id, string encryptedName, string encryptedPassword, string fullName) 
            : this(encryptedName, encryptedPassword, fullName)
        {
            Id = id;
        }
        public Teacher(int id, string encryptedName, string encryptedPassword, string fullName, ICollection<Test> ownedTests)
            : this(encryptedName, encryptedPassword, fullName, ownedTests)
        {
            Id = id;
        }

    }
}