using MvvmBaseViewModels.Common;

using TestingSystem.Models;
namespace TestingSystem.ViewModels.Teacher
{
    public class TestEditViewModel : ViewModelBase
    {
        private readonly Category[] categories;
        private readonly Test test;
        

        public TestEditViewModel(Category[] categories, Test test)
        {
            this.categories = categories;
            this.test = test;
        }

    }
}