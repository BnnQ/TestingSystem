using CommunityToolkit.Mvvm.ComponentModel;
using Meziantou.Framework.WPF.Builders;
using Meziantou.Framework.WPF.Collections;
using System.Collections.Generic;

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
                    ownedTests = new ConcurrentObservableCollectionBuilder<Test>(value).Build();
                    OnPropertyChanged(nameof(OwnedTests));
                }
            }
        }
        

        public Teacher(string encryptedName, string encryptedPassword, string fullName) : base(encryptedName, encryptedPassword)
        {
            OwnedTests = new ConcurrentObservableCollection<Test>();
            FullName = fullName;
        }
        public Teacher(string encryptedName, string encryptedPassword, string fullName, ICollection<Test> ownedTests) 
            : this(encryptedName, encryptedPassword, fullName)
        {
            OwnedTests = ownedTests;
        }

    }
}