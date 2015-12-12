using System;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using MahApps.Metro.Controls;

namespace SharingWorker
{
    public sealed class MahWindowManager : WindowManager
    {
        private static readonly ResourceDictionary[] resources;
        static MahWindowManager()
        {
            resources = new ResourceDictionary[] 
            {
                new ResourceDictionary { Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml", UriKind.RelativeOrAbsolute) },
                new ResourceDictionary { Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml", UriKind.RelativeOrAbsolute) },
                new ResourceDictionary { Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Colors.xaml", UriKind.RelativeOrAbsolute) },
                new ResourceDictionary { Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/Emerald.xaml", UriKind.RelativeOrAbsolute) },
                new ResourceDictionary { Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/BaseDark.xaml", UriKind.RelativeOrAbsolute) },
            };
        }

        protected override Window EnsureWindow(object model, object view, bool isDialog)
        {
            var window = view as MetroWindow;
            if (window == null)
            {
                window = new MetroWindow
                {
                    Content = view, 
                    SizeToContent = SizeToContent.WidthAndHeight,
                    GlowBrush = resources[3]["AccentColorBrush"] as SolidColorBrush,
                };
                
                foreach (ResourceDictionary resourceDictionary in resources)
                {
                    window.Resources.MergedDictionaries.Add(resourceDictionary);
                }
                window.SetValue(View.IsGeneratedProperty, true);

                var owner = this.InferOwnerOf(window);
                if (owner != null)
                {
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    window.Owner = owner;
                }
                else
                {
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }
            else
            {
                var owner2 = this.InferOwnerOf(window);
                if (owner2 != null && isDialog)
                {
                    window.Owner = owner2;
                }
            }
            return window;
        }
    }
}