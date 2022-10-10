using HappyStudio.Mvvm.Input.Wpf;
using MvvmBaseViewModels.Common;
using System.Collections.ObjectModel;
using TestingSystem.Models;

namespace TestingSystem.ViewModels.Teacher
{
    public class CategoryEditViewModel : ViewModelBase
    {
        private readonly Category category;

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

        private ObservableCollection<Test> tests = null!;
        public ObservableCollection<Test> Tests
        {
            get => tests;
            set
            {
                if (tests != value)
                {
                    tests = value;
                    OnPropertyChanged(nameof(Tests));
                }
            }
        }


        public CategoryEditViewModel(Category category)
        {
            this.category = category;

            Name = category.Name;
            Tests = new ObservableCollection<Test>(category.Tests);
        }


        #region Commands
        private RelayCommand okCommand = null!;
        public RelayCommand OkCommand
        {
            get => okCommand ??= new(() =>
            {
                category.Name = Name;
                category.Tests = Tests;
                Close(true);
            }, () => !string.IsNullOrWhiteSpace(Name));
        }

        private RelayCommand cancelCommand = null!;
        public RelayCommand CancelCommand
        {
            get => cancelCommand ??= new(() => Close(false));
        }
        #endregion

    }
}