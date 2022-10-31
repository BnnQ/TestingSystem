using Egor92.MvvmNavigation.Abstractions;
using HappyStudio.Mvvm.Input.Wpf;
using MaterialDesignThemes.Wpf;
using MvvmBaseViewModels.Common;
using MvvmBaseViewModels.Navigation;
using System.Windows;
using System.Windows.Controls;
using TestingSystem.Constants.Teacher;

namespace TestingSystem.ViewModels.Teacher
{
    public class MainContainerViewModel : ViewModelBase
    {
        private readonly INavigationManager navigationManager;
        public Models.Teacher Teacher { get; init; }


        public MainContainerViewModel(INavigationManager navigationManager, Models.Teacher teacher)
        {
            this.navigationManager = navigationManager;
            Teacher = teacher;
        }


        #region Commands

        #region Navigation
        private string lastNavigationKey = NavigationKeys.Main;

        private RelayCommand switchToMainNavigationCommand = null!;
        public RelayCommand SwitchToMainNavigationCommand
        {
            get => switchToMainNavigationCommand ??= new(() =>
            {
                navigationManager.Navigate(lastNavigationKey = NavigationKeys.Main,
                    new NavigationArgs(this));
                DrawerHost.CloseDrawerCommand.Execute(Dock.Left, null);
            }, () => navigationManager.CanNavigate(NavigationKeys.Main) && lastNavigationKey != NavigationKeys.Main);
        }

        private RelayCommand switchToStatisticsNavigationCommand = null!;
        public RelayCommand SwitchToStatisticsNavigationCommand
        {
            get => switchToStatisticsNavigationCommand ??= new(() =>
            {
                navigationManager.Navigate(lastNavigationKey = NavigationKeys.Statistics, 
                    new NavigationArgs(this));
                DrawerHost.CloseDrawerCommand.Execute(Dock.Left, null);
            }, () => navigationManager.CanNavigate(NavigationKeys.Statistics) && lastNavigationKey != NavigationKeys.Statistics);
        }

        private RelayCommand switchToAboutNavigationCommand = null!;
        public RelayCommand SwitchToAboutNavigationCommand
        {
            get => switchToAboutNavigationCommand ??= new(() =>
            {
                navigationManager.Navigate(lastNavigationKey = NavigationKeys.About, 
                    new NavigationArgs(this));
                DrawerHost.CloseDrawerCommand.Execute(Dock.Left, null);
            }, () => navigationManager.CanNavigate(NavigationKeys.About) && lastNavigationKey != NavigationKeys.About);
        }
        #endregion

        #region PopupBox
        private RelayCommand exitCommand = null!;
        public RelayCommand ExitCommand
        {
            get => exitCommand ??= new(() => Application.Current?.Shutdown());
        }
        #endregion

        #endregion

    }
}