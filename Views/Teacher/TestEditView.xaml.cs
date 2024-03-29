﻿using MvvmBaseViewModels.Helpers;
using System.Windows;
using TestingSystem.Constants;
using TestingSystem.Models;
using TestingSystem.ViewModels.Teacher;

namespace TestingSystem.Views.Teacher
{
    /// <summary>
    /// Interaction logic for TestEditView.xaml
    /// </summary>
    public partial class TestEditView : Window
    {
        private readonly TestEditViewModel viewModel;
        public TestEditView(Test test)
        {
            InitializeComponent();
            Tag = LoadStates.NotLoaded;

            viewModel = new TestEditViewModel(test);
            viewModel.Closed += (dialogResult) =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    if (dialogResult is not null)
                        DialogResult = dialogResult;

                    Close();
                });
            };
            viewModel.ErrorMessageOccurred += (exception) => DefaultMessageHandlers.HandleError(this, exception);
            viewModel.ErrorMessageOccurred += (_) => Application.Current?.Dispatcher.Invoke(Close);
            viewModel.CriticalErrorMessageOccured += (exception) =>
                DefaultMessageHandlers.HandleCriticalError(this, exception);

            viewModel.InitialLoaderBackgroundWorker.WorkCompleted += () =>
            {
                DataContext = viewModel;
                Tag = LoadStates.Loaded;
            };
            Dispatcher.ShutdownStarted += (_, _) =>
            {
                if (viewModel?.IsClosed == false)
                    viewModel.Close();
            };
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                if (Width > SystemParameters.PrimaryScreenWidth)
                    Width = SystemParameters.PrimaryScreenWidth;
                if (Height > SystemParameters.PrimaryScreenHeight)
                    Height = SystemParameters.PrimaryScreenHeight;

                firstTextBox.Focus();
            });
        }

    }
}