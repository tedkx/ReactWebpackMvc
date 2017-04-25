using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ReactWebpackTemplate.Startup))]
namespace ReactWebpackTemplate
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            //app.UseWebpackDevServer();
        }
    }
}
