using System.Windows.Threading;
using NLog;

namespace SharingWorker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        
        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Warn(e.Exception.Message);
        }
    }
}
