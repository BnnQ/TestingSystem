using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TestingSystem.Models
{
    public class Category : ObservableObject
    {
        public int Id { get; set; }
        
        private string name = null!;
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private ICollection<Test> tests = null!;
        public virtual ICollection<Test> Tests
        {
            get => tests;
            set
            {
                if (tests != value)
                {
                    if (value is ObservableCollection<Test>)
                        tests = value;
                    else
                        tests = new ObservableCollection<Test>(value);

                    OnPropertyChanged(nameof(Tests));
                }
            }
        }

        
        public Category()
        {
            Tests = new ObservableCollection<Test>();
        }
        public Category(string name) : this()
        {
            Name = name;
        }
        public Category(string name, ICollection<Test> tests) : this(name)
        {
            Tests = tests;
        }
        public Category(int id, string name) : this(name)
        {
            Id = id;
        }
        public Category(int id, string name, ICollection<Test> tests) : this(name, tests)
        {
            Id = id;
        }

    }
}