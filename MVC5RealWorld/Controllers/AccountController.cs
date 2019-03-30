using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC5RealWorld.Models.ViewModel;
using MVC5RealWorld.Models.EntityManager;
using System.Web.Security;

namespace MVC5RealWorld.Controllers
{
    public class AccountController : Controller
    {
        //This class returns the "SignUp.cshtml" View when that
        //action is required
        public ActionResult SignUp()
        {
            return View();
        }

        //This signUp class is decorated with the [httpPost] attribute
        //this attribute specifies that the overload 
        //of the "SignUp" method can be invoked only for POST request
        //The method also responsible for inserting new entry
        //to the database and automatically authenticate the users
        //using FormsAuthentication.SetAuthCokkie() method
        //This method creates an authentication ticket for the supplied
        //user name and adds it to the cookies collection of the response or 
        //to the URL if you are using cookieless authentication. 
        //After authenticating, we then redirect the users to the “Welcome.cshtml” page.
        [HttpPost]
        public ActionResult SignUp(UserSignUpView USV)
        {
            if (ModelState.IsValid) {
                UserManager UM = new UserManager();
                if (!UM.IsLoginNameExist(USV.LoginName))
                {
                    UM.AddUserAccount(USV);
                    FormsAuthentication.SetAuthCookie(USV.FirstName, false);
                    return RedirectToAction("Welcome", "Home");
                }
                else
                    ModelState.AddModelError("","Login Name already taken.");
            }
            return View();
        }


        //The first LogIN method returns the LofIn.cshtml View
        //we will create this view on the next step.
        
        public ActionResult LogIn() {
            return View();
        }

        //The second LogIn is decorated with the "HttpPost" attribute
        //specifies the overload of the Login method that can be invoked
        //only for POST request

        //This second method will be triggered once the Button "Login" is clicked.
        //First it will check if the required fields are supplied, so it checks for
        //ModelState.IsValid condition.
        //Then, it will create an instance of the UserManager class and call the GetUsetPassword()
        //method by passing the user LoginName value supplied by the user.
        //
        //If the password returns an empty string then it will display an error
        //to the view.
        //If the password supplied is equal to the password retrieved from the 
        //database then it will redirect the user to the "Welcome" page
        //otherwise it will display an error stating that the login name or password
        //supplied is invalid

        [HttpPost]
        public ActionResult LogIn(UserLoginView ULV, string returnUrl) {
            if (ModelState.IsValid) {
                UserManager UM = new UserManager();
                string password = UM.GetUserPassword(ULV.LoginName);

                if (string.IsNullOrEmpty(password))
                    ModelState.AddModelError("", "The user login or parrword provided is incorrect.");

                else {
                    if (ULV.Password.Equals(password))
                    {
                        FormsAuthentication.SetAuthCookie(ULV.LoginName, false);
                        return RedirectToAction("Welcome", "Home");
                    }
                    else {
                        ModelState.AddModelError("","The password provided is incorrect.");
                    }
                }
            }
            //If this code is reached, something is failed, redisplay form
            return View(ULV);
        }

        //The FormsAuthentication.SignOut method removes the
        //forms-authentication ticket from the browser.
        //We then redirect the user to the "Index" page after
        //signing out

        [Authorize]
        public ActionResult SignOut() {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index","Home");
        }
    }
}