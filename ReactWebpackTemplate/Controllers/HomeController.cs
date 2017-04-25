using ReactWebpackTemplate.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReactWebpackTemplate.Controllers
{
    public class HomeController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            var model = new HomeViewModel()
            {
                Title = "Title",
                ScriptTags = WebpackDevServerConfig.Current.ScriptTags
            };

            return View(model);
        }
    }
}