using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Xaml;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public UIElement AddHeader()
        {
            return new TextBlock
            {
                Text = "Header Text",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5)
            };
        }

        public UIElement AddButton()
        {
            Button button = new Button
            {
                Content = "Click Me",
                Width = 100,
                Height = 50
            };

            // Apply dynamic resources for styling
            button.SetResourceReference(Button.BackgroundProperty, "ButtonBackground");
            button.SetResourceReference(Button.ForegroundProperty, "ButtonForeground");

            return button;
        }

        public UIElement AddTextBox()
        {
            TextBox textBox = new TextBox
            {
                Width = 200
            };

            // Apply dynamic resources for styling
            textBox.SetResourceReference(Control.BackgroundProperty, "WindowBackground");

            return textBox;
        }
        public void ShowWindow(List<UIElement> settings)
        {
            Window window = new()
            {
                Title = "My Custom Window",
                Width = 400,
                Height = 300,
                Topmost = true,
                WindowStartupLocation = WindowStartupLocation.CenterScreen, // Center window on screen
                Content = new StackPanel()
            };

            foreach (var element in settings)
            {
                ((StackPanel)window.Content).Children.Add(element);
            }
            SwitchTheme(true);
            window.ShowDialog(); // Keep window open until user closes it
        }

        public static void SwitchTheme(bool isDarkMode)
        {
            Uri themeUri = isDarkMode ? new Uri("pack://application:,,,/DarkTheme.xaml") : new Uri("pack://application:,,,/LightTheme.xaml");
            ResourceDictionary theme = Application.LoadComponent(themeUri) as ResourceDictionary;

            // Apply the theme to the current application
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(theme);
        }

    }
}
