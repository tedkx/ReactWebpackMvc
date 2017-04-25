using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ReactWebpackTemplate
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("dist/{*pathInfo}");

            routes.MapRoute(
                name: "DataRoute",
                url: "api/{controller}/{id}",
                defaults: new { id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Index",
                url: "{*args}",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}
