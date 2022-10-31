using System.Windows;
using System.Windows.Input;

namespace TestingSystem.Helpers
{
    public class EnterKeyTraversal
    {
        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        static void Ue_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement ue && ue is not null)
            {
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    ue.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            }
        }

        private static void Ue_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement ue && ue is not null)
            {
                ue.Unloaded -= Ue_Unloaded;
                ue.PreviewKeyDown -= Ue_PreviewKeyDown;
            }
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool),

            typeof(EnterKeyTraversal), new UIPropertyMetadata(false, IsEnabledChanged));

        static void IsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement ue && ue is not null)
            {
                if ((bool) e.NewValue)
                {
                    ue.Unloaded += Ue_Unloaded;
                    ue.PreviewKeyDown += Ue_PreviewKeyDown;
                }
                else
                {
                    ue.PreviewKeyDown -= Ue_PreviewKeyDown;
                }
            }
        }
    }
}