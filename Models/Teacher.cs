using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TestingSystem.Models
{
    [ObservableObject]
    public partial class Teacher : User
    {
        public int Id { get; set; }

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
        

        public Teacher(string encryptedName, string encryptedPassword) : base(encryptedName, encryptedPassword)
        {
            OwnedTests = new ObservableCollection<Test>();
        }
        public Teacher(string encryptedName, string encryptedPassword, ICollection<Test> ownedTests) 
            : this(encryptedName, encryptedPassword)
        {
            OwnedTests = ownedTests;
        }
        public Teacher(int id, string encryptedName, string encryptedPassword) : this(encryptedName, encryptedPassword)
        {
            Id = id;
        }
        public Teacher(int id, string encryptedName, string encryptedPassword, ICollection<Test> ownedTests)
            : this(encryptedName, encryptedPassword, ownedTests)
        {
            Id = id;
        }

    }
}