using HappyStudio.Mvvm.Input.Wpf;
using System.Windows;
using System.Windows.Input;

namespace TestingSystem.Helpers
{
    public static class CursorOverrider
    {
        private static RelayCommand<Cursor> overrideCursorCommand = null!;
        public static RelayCommand<Cursor> OverrideCursorCommand
        {
            get => overrideCursorCommand ??= new((cursorType) =>
            {
                Application.Current?.Dispatcher.Invoke(() => Mouse.OverrideCursor = cursorType);
            }, (cursorType) => cursorType is not null);
        }
    }
}