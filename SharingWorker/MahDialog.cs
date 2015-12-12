using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace SharingWorker
{
    class MahDialog
    {
        public static async Task<MessageDialogResult> ShowMessage(string title, string message, MessageDialogStyle dialogStyle, MetroWindow parent = null)
        {
            var window = parent ?? (MetroWindow)Application.Current.MainWindow;
            window.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
            return await window.ShowMessageAsync(title, message, dialogStyle, window.MetroDialogOptions);
        }

        public static async Task<string> ShowInput(string title, string message, MetroWindow parent = null)
        {
            var window = parent ?? (MetroWindow)Application.Current.MainWindow;
            window.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
            return await window.ShowInputAsync(title, message, window.MetroDialogOptions);
        }
    }
}
