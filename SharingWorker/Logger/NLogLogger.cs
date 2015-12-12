using System;
using Caliburn.Micro;

namespace SharingWorker.Logger
{
    class NLogLogger : ILog
    {
        private readonly NLog.Logger logger;

        public NLogLogger(Type type)
        {
            logger = NLog.LogManager.GetLogger(type.Name);
        }

        public void Info(string format, params object[] args)
        {
            logger.Info(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            logger.Warn(format, args);
        }

        public void Error(Exception ex)
        {
            logger.ErrorException(ex.Message, ex);
        }
    }
}
