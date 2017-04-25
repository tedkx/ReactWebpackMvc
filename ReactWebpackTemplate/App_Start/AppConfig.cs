using ReactWebpackTemplate.Helpers;
using System;
using System.Configuration;
using System.Web.Configuration;

namespace ReactWebpackTemplate
{
    public static class AppConfig
    {
        public static bool UseWebpackDevServer { get; private set; }
        public static bool IsDebug { get; private set; }

        public static void Configure()
        {
            try
            {
                UseWebpackDevServer = ConfigurationManager.AppSettings["UseWebpackDevServer"].Equals("true", System.StringComparison.InvariantCultureIgnoreCase);

                var compilationSection = (CompilationSection)ConfigurationManager.GetSection(@"system.web/compilation");
                IsDebug = compilationSection.Debug;
            }
            catch(Exception ex)
            {
                GeneralHelper.Logger.Error("Application configuration", ex);
            }
        }
    }
}