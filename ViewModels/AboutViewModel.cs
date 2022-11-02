using HappyStudio.Mvvm.Input.Wpf;
using MvvmBaseViewModels.Common;
using System;
using System.Diagnostics;
using System.Reflection;

namespace TestingSystem.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        public Uri GithubUrl { get; init; }
        public Uri IconUrl { get; init; }
        public Uri DesignLibraryUrl { get; init; }
        public string ProgramVersion { get; init; }

        public AboutViewModel()
        {
            GithubUrl = new Uri("https://github.com/BnnQ/TestingSystem", UriKind.Absolute);
            IconUrl = new Uri("https://icons8.com/icon/GGgmJKaq5zTO/test", UriKind.Absolute);
            DesignLibraryUrl = new Uri("https://github.com/ButchersBoy/MaterialDesignInXamlToolkit", UriKind.Absolute);
            ProgramVersion = (FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion)!;
        }

        #region Commands
        private RelayCommand<Uri> openUrlInBrowserCommand = null!;
        public RelayCommand<Uri> OpenUrlInBrowserCommand
        {
            get => openUrlInBrowserCommand ??= new((url) =>
            {
                ProcessStartInfo processStartInfo = new();
                processStartInfo.FileName = url!.AbsoluteUri;
                processStartInfo.UseShellExecute = true;

                Process.Start(processStartInfo);
            }, (url) => url is not null && !string.IsNullOrWhiteSpace(url.AbsoluteUri));
        }
        #endregion
    }
}