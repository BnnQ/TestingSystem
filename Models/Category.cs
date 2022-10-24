using CommunityToolkit.Mvvm.ComponentModel;
using Meziantou.Framework.WPF.Builders;
using Meziantou.Framework.WPF.Collections;
using System.Collections.Generic;

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
                    tests = new ConcurrentObservableCollectionBuilder<Test>(value).Build();
                    OnPropertyChanged(nameof(Tests));
                }
            }
        }

        
        public Category()
        {
            Tests = new ConcurrentObservableCollection<Test>();
        }
        public Category(string name) : this()
        {
            Name = name;
        }
        public Category(string name, ICollection<Test> tests) : this(name)
        {
            Tests = tests;
        }

    }
}