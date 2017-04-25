using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReactWebpackTemplate.Helpers
{
    public static class GeneralHelper
    {
        const string DefaultLoggerName = "DefaultLogger";

        public static ILog Logger
        {
            get { return LogManager.GetLogger(DefaultLoggerName); }
        }
    }
}