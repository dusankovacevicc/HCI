using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using NetworkService.Services;

namespace NetworkService.Converters
{
    /// <summary>Returns Visible when the bound bool is true, Collapsed otherwise.</summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is bool b && b;
            if (Invert)
            {
                flag = !flag;
            }

            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    /// <summary>Maps an entity's validity (true/false) to a status brush.</summary>
    public class ValidToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool valid = value is bool b && b;
            string key = valid ? "ValidBrush" : "DangerBrush";
            return Application.Current.TryFindResource(key) ?? Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    /// <summary>Resolves an EntityType.ImageKey to the predefined DrawingImage resource.</summary>
    public class ImageKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string key = value as string;
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            return Application.Current.TryFindResource(key);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    /// <summary>Maps a ToastType to the matching background brush.</summary>
    public class ToastTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string key = "AccentBrush";
            if (value is ToastType type)
            {
                key = type switch
                {
                    ToastType.Success => "ValidBrush",
                    ToastType.Error => "DangerBrush",
                    ToastType.Warning => "AccentBrush",
                    _ => "PanelAltBrush"
                };
            }

            return Application.Current.TryFindResource(key) ?? Brushes.DimGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    /// <summary>True -> Visible only when the value is null (used for placeholders).</summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
