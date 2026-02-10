using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RainmeterLayoutManager.Helpers
{
    /// <summary>
    /// Provides placeholder text functionality for WPF TextBox controls.
    /// </summary>
    public static class TextBoxPlaceholderBehavior
    {
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached(
                "Placeholder",
                typeof(string),
                typeof(TextBoxPlaceholderBehavior),
                new PropertyMetadata(string.Empty, OnPlaceholderChanged));

        public static string GetPlaceholder(DependencyObject obj)
        {
            return (string)obj.GetValue(PlaceholderProperty);
        }

        public static void SetPlaceholder(DependencyObject obj, string value)
        {
            obj.SetValue(PlaceholderProperty, value);
        }

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.Loaded -= OnTextBoxLoaded;
                textBox.GotFocus -= OnTextBoxGotFocus;
                textBox.LostFocus -= OnTextBoxLostFocus;
                textBox.TextChanged -= OnTextBoxTextChanged;

                if (!string.IsNullOrEmpty(e.NewValue as string))
                {
                    textBox.Loaded += OnTextBoxLoaded;
                    textBox.GotFocus += OnTextBoxGotFocus;
                    textBox.LostFocus += OnTextBoxLostFocus;
                    textBox.TextChanged += OnTextBoxTextChanged;

                    UpdatePlaceholder(textBox);
                }
            }
        }

        private static void OnTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholder((TextBox)sender);
        }

        private static void OnTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            if (IsShowingPlaceholder(textBox))
            {
                textBox.Text = string.Empty;
                textBox.Foreground = SystemColors.ControlTextBrush;
            }
        }

        private static void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholder((TextBox)sender);
        }

        private static void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            // If user clears the text while focused, don't immediately show placeholder
            if (!textBox.IsFocused && string.IsNullOrEmpty(textBox.Text))
            {
                UpdatePlaceholder(textBox);
            }
        }

        private static void UpdatePlaceholder(TextBox textBox)
        {
            var placeholder = GetPlaceholder(textBox);

            if (string.IsNullOrEmpty(textBox.Text) && !textBox.IsFocused)
            {
                textBox.Text = placeholder;
                textBox.Foreground = new SolidColorBrush(Colors.Gray);
                textBox.FontStyle = FontStyles.Italic;
            }
            else if (IsShowingPlaceholder(textBox) && textBox.IsFocused)
            {
                textBox.Text = string.Empty;
                textBox.Foreground = SystemColors.ControlTextBrush;
                textBox.FontStyle = FontStyles.Normal;
            }
            else if (!IsShowingPlaceholder(textBox))
            {
                textBox.Foreground = SystemColors.ControlTextBrush;
                textBox.FontStyle = FontStyles.Normal;
            }
        }

        private static bool IsShowingPlaceholder(TextBox textBox)
        {
            var placeholder = GetPlaceholder(textBox);
            return textBox.Text == placeholder;
        }
    }
}
