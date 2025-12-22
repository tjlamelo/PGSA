using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PGSA_Licence3.Controllers.Auth
{
    public class LoginController : Controller
    {
        // GET: LoginController
        public ActionResult Index()
        {
            return View("~/Views/Auth/Login.cshtml");
        }
       


    }
}
